using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    internal class Terrain
    {
        public const string ContentFolderModels = "Models/";
        private Vector3 _defaultColor;
        private Matrix _world;

        private readonly Effect effect;

        private static Texture2D HeightData;
        private static Color[] Texels;
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;

        private readonly int primitiveCount;
        private readonly int quadsCount;
        private static float HeightScale;
        private static Vector2 TerrainSize;

        public Terrain(Game game, Effect effect, GraphicsDevice GraphicsDevice, Vector2 terrainSize, float heightScale)
        {
            HeightData = game.Content.Load<Texture2D>(ContentFolderModels + "terrain/heightmap");
            Texels = new Color[HeightData.Width * HeightData.Height];
            _defaultColor = new Vector3(42 / 255.0f, 120 / 255.0f, 49 / 255.0f);
            primitiveCount = HeightData.Width * HeightData.Height * 2;
            quadsCount = (HeightData.Width - 1) * (HeightData.Height - 1);
            this.effect = effect;

            _world = Matrix.Identity;

            vertexBuffer = new(GraphicsDevice, VertexPosition.VertexDeclaration, HeightData.Width * HeightData.Height, BufferUsage.None);
            indexBuffer = new(GraphicsDevice, IndexElementSize.SixteenBits, 6 * quadsCount, BufferUsage.None);

            GenerateTerrainMesh(TerrainSize = terrainSize, HeightScale = heightScale);
        }

        public static float GetPositionHeight(float x, float z, float outOfTerrainHeight = 0.0f) {
            if (x < (-TerrainSize.X / 2) || x > (TerrainSize.X / 2) || z < (-TerrainSize.Y / 2) || z > (TerrainSize.Y / 2)) {
                return outOfTerrainHeight;
            } else {
                float xFloatIndex = (x + (TerrainSize.X / 2)) * (HeightData.Width - 1) / TerrainSize.X; // Unscale position and move it to range from 0 to HeightData.Width - 1
                int xIndex = (int) xFloatIndex;
                float xIndexFracPart = xFloatIndex - xIndex;
                int nextXIndex = xIndex + 1;

                float zFloatIndex = (z + (TerrainSize.Y / 2)) * (HeightData.Height - 1) / TerrainSize.Y; // Unscale position and move it to range from 0 to HeightData.Width - 1
                int zIndex = (int) zFloatIndex;
                float zIndexFracPart = zFloatIndex - zIndex;
                int nextZIndex = zIndex + 1;

                float height;
                float nextHeightX = Texels[zIndex * HeightData.Width + nextXIndex].R * HeightScale;
                float nextHeightZ = Texels[nextZIndex * HeightData.Width + xIndex].R * HeightScale;

                //     (xIndex; zIndex) -> a__b <- (xIndex + 1; zIndex)
                //                         | /|
                //                         |/_|
                // (xIndex; zIndex + 1) -> c  d <- (xIndex + 1; zIndex + 1)

                // Me fijo si debo calcular la altura en el triangulo superior del quad o en el inferior
                if (zIndexFracPart <= 1 - xIndexFracPart) {
                    height = Texels[zIndex * HeightData.Width + xIndex].R * HeightScale;
                } else {
                    height = Texels[nextZIndex * HeightData.Width + nextXIndex].R * HeightScale;
                    xIndexFracPart = 1 - xIndexFracPart;
                    zIndexFracPart = 1 - zIndexFracPart;
                }

                // if (height == nextHeightX && nextHeightX == nextHeightZ) {}
                // else {
                //     Console.WriteLine("XFloatIndex: " + xFloatIndex);
                //     Console.WriteLine("ZFloatIndex: " + zFloatIndex);
                //     Console.WriteLine("X: " + xIndex + ", Z: " + zIndex + ", Height: " + Texels[zIndex * HeightData.Width + xIndex].R * HeightScale);
                //     Console.WriteLine("X + 1: " + nextXIndex + ", Z: " + zIndex + ", Height: " + Texels[zIndex * HeightData.Width + nextXIndex].R * HeightScale);
                //     Console.WriteLine("X: " + xIndex + ", Z + 1: " + nextZIndex + ", Height: " + Texels[nextZIndex * HeightData.Width + xIndex].R * HeightScale);
                //     Console.WriteLine("X + 1: " + nextXIndex + ", Z + 1: " + nextZIndex + ", Height: " + Texels[nextZIndex * HeightData.Width + nextXIndex].R * HeightScale);
                //     Console.WriteLine("Height Output: " + (height + (nextHeightX - height) * xIndexFracPart + (nextHeightZ - height) * zIndexFracPart));
                // }

                return height + (nextHeightX - height) * xIndexFracPart + (nextHeightZ - height) * zIndexFracPart;
            }
        }

        public void GenerateTerrainMesh(Vector2 terrainSize, float heightScale) {
            HeightData.GetData(Texels);

            float scaleX = terrainSize.X / (HeightData.Width - 1);
            float scaleY = heightScale;
            float scaleZ = terrainSize.Y / (HeightData.Height - 1);
            float offsetX = scaleX * (HeightData.Width - 1) / 2;
            float offsetZ = scaleZ * (HeightData.Height - 1) / 2;

            VertexPosition[] vertices = new VertexPosition[HeightData.Width * HeightData.Height];

            for (int j = 0; j < HeightData.Height; j++) {
                for (int i = 0; i < HeightData.Width; i++) {
                    float height = Texels[j * HeightData.Width + i].R * scaleY;
                    // Console.Write(height.ToString("0.##") + "\t");
                    Vector3 position = new(
                        i * scaleX - offsetX,
                        height,
                        j * scaleZ - offsetZ
                    );
                    VertexPosition vertex = new(position);
                    vertices[j * HeightData.Width + i] = vertex;
                }
                // Console.Write(Environment.NewLine);
            }

            vertexBuffer.SetData(vertices);
            ushort[] indexes = new ushort[6 * quadsCount]; // for each quad: 2 triangles with 3 vertices each

                //  0 __  ...  __a__ b ...  __  MaxWidth - 1
                //   | /| ... | /| /|  ... | /|
                //   |/_| ... |/_|/_|  ... |/_|
                //            ^  c   d
                //            |
                //    a + (MaxWidth - 1)
                //
                // c = a + (MaxWidth - 1) + 1 = a + MaxWidth
                // d = a + (MaxWidth - 1) + 2 = a + MaxWidth + 1

            for (int i = 0; i < (quadsCount + HeightData.Height - 2);) {
                indexes[(i - i / HeightData.Width) * 6]   =   (ushort) i;                            // a
                indexes[(i - i / HeightData.Width) * 6 + 1] = (ushort)(i + HeightData.Width);        // c
                indexes[(i - i / HeightData.Width) * 6 + 2] = (ushort)(i + 1);                       // b

                indexes[(i - i / HeightData.Width) * 6 + 3] = (ushort)(i + 1);                       // b
                indexes[(i - i / HeightData.Width) * 6 + 4] = (ushort)(i + HeightData.Width);        // c
                indexes[(i - i / HeightData.Width) * 6 + 5] = (ushort)(i + HeightData.Width + 1);    // d

                i+= (((i + 2) % HeightData.Width) == 0) ? 2 : 1;
            }

            indexBuffer.SetData(indexes);
        }

        public void Draw(GraphicsDevice GraphicsDevice, Effect effect)
        {
            effect.Parameters["World"].SetValue(_world);
            effect.Parameters["DiffuseColor"].SetValue(_defaultColor);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }
        }
    }
}
