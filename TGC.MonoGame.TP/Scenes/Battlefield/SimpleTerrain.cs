using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    /// <summary>
    ///     Permite crear la malla de un terreno en base a una textura de Heightmap.
    /// </summary>
    public class SimpleTerrain
    {
        private readonly Texture2D modelTexture;
        private readonly Texture2D normalTexture;
        private readonly Effect Effect;
        private float m_scaleXZ = 1;
        private float m_scaleY = 1;
        private VertexBuffer vbTerrain;
        private int primitiveCount;
        private Matrix world;
        private Vector2 tiling;

        public SimpleTerrain(GraphicsDevice graphicsDevice, Texture2D heightMap, Texture2D diffuseMap, Texture2D normalMap, Effect effect, float scaleXZ, float scaleY)
        {
            // matriz de mundo
            world = Matrix.Identity;

            // shader Blinn�Phong con normal map
            Effect = effect;

            // cargo el heightmap
            LoadHeightmap(graphicsDevice, heightMap, scaleXZ, scaleY, Vector3.Zero);
            // texturas con colores y normales
            modelTexture = diffuseMap;
            normalTexture = normalMap;
            tiling = Vector2.One * 32;
        }

        /// <summary>
        ///     Valor de Y para cada par (X,Z) del Heightmap
        /// </summary>
        public int[,] HeightmapData { get; private set; }

        /// <summary>
        ///     Centro del terreno
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        ///     Renderiza el terreno
        /// </summary>
        public void Draw(Matrix view, Matrix projection)
        {
            var graphicsDevice = Effect.GraphicsDevice;
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            Effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            Effect.Parameters["WrapTexture"].SetValue(modelTexture);
            Effect.Parameters["NormalTexture"].SetValue(normalTexture);
            Effect.Parameters["Tiling"].SetValue(tiling);
            Effect.Parameters["ambientColor"].SetValue(Vector3.One);
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.6f, 1f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(Vector3.One);
            Effect.Parameters["KAmbient"].SetValue(0.3f);
            Effect.Parameters["KDiffuse"].SetValue(0.6f);
            Effect.Parameters["KSpecular"].SetValue(0f);
            Effect.Parameters["shininess"].SetValue(1.0f);
            graphicsDevice.SetVertexBuffer(vbTerrain);

            //Render con shader
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
            }
        }

        public void DrawShadowMap(Matrix view, Matrix projection, Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;
            effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            graphicsDevice.SetVertexBuffer(vbTerrain);

            //Render con shader
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
            }
        }

        /// <summary>
        ///     Crea la malla de un terreno en base a un Heightmap
        /// </summary>
        /// <param name="heightmap">Imagen de Heightmap</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void LoadHeightmap(GraphicsDevice graphicsDevice, Texture2D heightmap, float scaleXZ, float scaleY, Vector3 center)
        {
            Center = center;

            m_scaleXZ = scaleXZ;
            m_scaleY = scaleY;

            float tx_scale = 1; // 50f;

            //cargar heightmap
            HeightmapData = LoadHeightMap(heightmap);
            var width = HeightmapData.GetLength(0);
            var length = HeightmapData.GetLength(1);

            //Cargar vertices
            var totalVertices = 2 * 3 * (HeightmapData.GetLength(0) - 1) * (HeightmapData.GetLength(1) - 1);
            var dataIdx = 0;
            var data = new VertexPositionNormalTexture[totalVertices];

            center.X = center.X * scaleXZ - width / 2f * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - length / 2f * scaleXZ;

            var N = new Vector3[width, length];
            for (var i = 0; i < width - 1; i++)
            {
                for (var j = 0; j < length - 1; j++)
                {
                    var v1 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v2 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);
                    var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                        center.Z + j * scaleXZ);
                    N[i, j] = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
                }
            }

            for (var i = 0; i < width - 1; i++)
            {
                for (var j = 0; j < length - 1; j++)
                {
                    //Vertices
                    var v1 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v2 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);
                    var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);

                    //Coordendas de textura
                    var t1 = new Vector2(i / (float) width, j / (float) length) * tx_scale;
                    var t2 = new Vector2(i / (float) width, (j + 1) / (float) length) * tx_scale;
                    var t3 = new Vector2((i + 1) / (float) width, j / (float) length) * tx_scale;
                    var t4 = new Vector2((i + 1) / (float) width, (j + 1) / (float) length) * tx_scale;

                    //Cargar triangulo 1
                    data[dataIdx] = new VertexPositionNormalTexture(v1, N[i, j], t1);
                    data[dataIdx + 1] = new VertexPositionNormalTexture(v2, N[i, j + 1], t2);
                    data[dataIdx + 2] = new VertexPositionNormalTexture(v4, N[i + 1, j + 1], t4);

                    //Cargar triangulo 2
                    data[dataIdx + 3] = new VertexPositionNormalTexture(v1, N[i, j], t1);
                    data[dataIdx + 4] = new VertexPositionNormalTexture(v4, N[i + 1, j + 1], t4);
                    data[dataIdx + 5] = new VertexPositionNormalTexture(v3, N[i + 1, j], t3);

                    dataIdx += 6;
                }
            }

            //Crear vertexBuffer
            vbTerrain = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, totalVertices, BufferUsage.WriteOnly);
            vbTerrain.SetData(data);

            //Guardo cantidad de tri�ngulos
            primitiveCount = vbTerrain.VertexCount / 3;
        }

        /// <summary>
        ///     Carga los valores del Heightmap en una matriz
        /// </summary>
        protected int[,] LoadHeightMap(Texture2D texture)
        {
            var width = texture.Width;
            var height = texture.Height;
            var rawData = new Color[width * height];
            texture.GetData(rawData);
            var heightmap = new int[width, height];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    //(j, i) invertido para primero barrer filas y despues columnas
                    var pixel = rawData[j * texture.Width + i];
                    var intensity = pixel.R;
                    heightmap[i, j] = (int) intensity;
                }
            }

            return heightmap;
        }

        public float Height(float x, float z)
        {
            var width = HeightmapData.GetLength(0);
            var length = HeightmapData.GetLength(1);

            var pos_i = x / m_scaleXZ + width / 2.0f;
            var pos_j = z / m_scaleXZ + length / 2.0f;
            var pi = (int) pos_i;
            var fracc_i = pos_i - pi;
            var pj = (int) pos_j;
            var fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else if (pi >= width)
                pi = width - 1;

            if (pj < 0)
                pj = 0;
            else if (pj >= length)
                pj = length - 1;

            var pi1 = pi + 1;
            var pj1 = pj + 1;
            if (pi1 >= width)
                pi1 = width - 1;
            if (pj1 >= length)
                pj1 = length - 1;

            // 2x2 percent closest filtering usual:
            var H0 = HeightmapData[pi, pj];
            var H1 = HeightmapData[pi1, pj];
            var H2 = HeightmapData[pi, pj1];
            var H3 = HeightmapData[pi1, pj1];
            var H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) + (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;

            return H * m_scaleY;
        }
    }
}