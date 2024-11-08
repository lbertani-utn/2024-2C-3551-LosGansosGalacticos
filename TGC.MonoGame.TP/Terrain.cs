using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    internal class Terrain
    {
        public const string ContentFolderModels = "Models/";
        public const string ContentFolderTextures = "Textures/";
        private Vector3 _defaultColor;
        private Matrix _world;

        public readonly SpacialMap spacialMap;

        private readonly Effect effect;
        private readonly GraphicsDevice _graphicsDevice;

        private static Texture2D HeightData;
        private static Color[] Texels;
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;

        private readonly int primitiveCount;
        private readonly int quadsCount;
        private static float HeightScale;
        private static (float X, float Z) TerrainSize;

        public Terrain(Game game, Effect effect, GraphicsDevice GraphicsDevice, (float X, float Z) terrainSize, float heightScale)
        {
            //HeightData = game.Content.Load<Texture2D>(ContentFolderModels + "terrain/heightmap");
            HeightData = game.Content.Load<Texture2D>(ContentFolderTextures + "Rolling Hills Height Map/Rolling Hills Height Map 256");
            Texels = new Color[HeightData.Width * HeightData.Height];
            _defaultColor = new Vector3(42 / 255.0f, 120 / 255.0f, 49 / 255.0f);
            primitiveCount = HeightData.Width * HeightData.Height * 2;
            quadsCount = (HeightData.Width - 1) * (HeightData.Height - 1);
            this.effect = effect;

            _world = Matrix.Identity;
            _graphicsDevice = GraphicsDevice;

            vertexBuffer = new(_graphicsDevice, VertexPosition.VertexDeclaration, HeightData.Width * HeightData.Height, BufferUsage.None);
            indexBuffer = new(_graphicsDevice, IndexElementSize.SixteenBits, 6 * quadsCount, BufferUsage.None);

            GenerateTerrainMesh(TerrainSize = terrainSize, HeightScale = heightScale);

            spacialMap = new(terrainSize, (25, 25));
        }

        public static float GetPositionHeight(float x, float z, float outOfTerrainHeight = 0.0f, bool verbose = false)
        {
            if (x < (-TerrainSize.X / 2) || x > (TerrainSize.X / 2) || z < (-TerrainSize.Z / 2) || z > (TerrainSize.Z / 2))
            {
                return outOfTerrainHeight;
            }
            else
            {
                float xFloatIndex = (x + (TerrainSize.X / 2)) * (HeightData.Width - 1) / TerrainSize.X; // Unscale position and move it to range from 0 to HeightData.Width - 1
                int xIndex = (int)xFloatIndex;
                float xIndexFracPart = xFloatIndex - xIndex;
                int nextXIndex = xIndex + 1;

                float zFloatIndex = (z + (TerrainSize.Z / 2)) * (HeightData.Height - 1) / TerrainSize.Z; // Unscale position and move it to range from 0 to HeightData.Width - 1
                int zIndex = (int)zFloatIndex;
                float zIndexFracPart = zFloatIndex - zIndex;
                int nextZIndex = zIndex + 1;

                //     (xIndex; zIndex) -> a__b <- (xIndex + 1; zIndex)
                //                         | /|
                //                         |/_|
                // (xIndex; zIndex + 1) -> c  d <- (xIndex + 1; zIndex + 1)

                float heightA = Texels[zIndex * HeightData.Width + xIndex].R * HeightScale;
                float heightB = Texels[zIndex * HeightData.Width + nextXIndex].R * HeightScale;
                float heightC = Texels[nextZIndex * HeightData.Width + xIndex].R * HeightScale;
                float heightD = Texels[nextZIndex * HeightData.Width + nextXIndex].R * HeightScale;

                return (heightA * (1 - xIndexFracPart) + heightB * xIndexFracPart) * (1 - zIndexFracPart) + (heightC * (1 - xIndexFracPart) + heightD * xIndexFracPart) * zIndexFracPart;
            }
        }

        public void GenerateTerrainMesh((float X, float Z) terrainSize, float heightScale) {
            HeightData.GetData(Texels);

            float scaleX = terrainSize.X / (HeightData.Width - 1);
            float scaleY = heightScale;
            float scaleZ = terrainSize.Z / (HeightData.Height - 1);
            float offsetX = scaleX * (HeightData.Width - 1) / 2;
            float offsetZ = scaleZ * (HeightData.Height - 1) / 2;

            VertexPosition[] vertices = new VertexPosition[HeightData.Width * HeightData.Height];

            for (int j = 0; j < HeightData.Height; j++) {
                for (int i = 0; i < HeightData.Width; i++) {
                    float height = Texels[j * HeightData.Width + i].R * scaleY;
                    // Console.Write(height.ToString() + ";");
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
            this.effect.Parameters["World"].SetValue(_world);
            this.effect.Parameters["DiffuseColor"].SetValue(_defaultColor);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.SetVertexBuffer(vertexBuffer);
            _graphicsDevice.Indices = indexBuffer;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.BlendState = BlendState.Opaque;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }
        }
    }
}
