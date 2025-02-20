using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    public class CollisionData {
        public CollisionData((int x, int z) gridPosition, (int x, int z) gridSize, WorldEntity entity) {
            this.gridPosition = gridPosition;
            this.gridSize = gridSize;
            this.entity = entity;
        }
        public readonly (int x, int z) gridPosition;
        public readonly (int x, int z) gridSize;
        public readonly WorldEntity entity;
    }

    public class SpacialMap {
        HashSet<WorldEntity>[][] grid;
        private readonly float cellSizeX;
        private readonly float cellSizeZ;
        private readonly int gridSizeX;
        private readonly int gridSizeZ;

        public SpacialMap((float X, float Z) terrainSize, (int X, int Z) gridSize) {
            gridSizeX = gridSize.X;
            gridSizeZ = gridSize.Z;

            cellSizeX = terrainSize.X / gridSizeX;
            cellSizeZ = terrainSize.Z / gridSizeZ;

            grid = new HashSet<WorldEntity>[gridSizeX][];

            for (int i = 0; i < gridSizeX; i++) {
                grid[i] = new HashSet<WorldEntity>[gridSizeZ];
                for (int j = 0; j < gridSizeZ; j++) {
                    grid[i][j] = new HashSet<WorldEntity>();
                }
            }
        }

        public void Add(WorldEntity entity) {
            entity.SetGridIndices(0,0,0,0);
            Update(entity);
        }

        public void Update(WorldEntity entity) {
            BoundingBox box = entity.GetHitBox();

            int minX = (int) Math.Floor(box.Min.X / cellSizeX + gridSizeX / 2.0f);
            int maxX = (int) Math.Floor(box.Max.X  / cellSizeX + gridSizeX / 2.0f);
            int minZ = (int) Math.Floor(box.Min.Z / cellSizeZ + gridSizeZ / 2.0f);
            int maxZ = (int) Math.Floor(box.Max.Z / cellSizeZ + gridSizeZ / 2.0f);

            if (minX >= 0 && maxX < gridSizeX && minZ >= 0 && maxZ < gridSizeZ) // Está dentro del terreno definido por la grilla?
            {
                var (Min, Max) = entity.GetGridIndices();
                if (minX < Min.gridX || minZ < Min.gridZ || maxX > Max.gridX || maxZ > Max.gridZ) { // Hubo cambios en la posición del modelo respecto de la grilla?
                    Delete(entity);
                    for (int i = minX; i <= maxX; i++) {
                        for (int j = minZ; j <= maxZ; j++) {
                            grid[i][j].Add(entity);
                        }
                    }
                    entity.SetGridIndices(minX, minZ, maxX, maxZ);
                }
            }
        }

        public CollisionData[] GetNearbyEntities(WorldEntity worldEntity) {
            var (Min, Max) = worldEntity.GetGridIndices();
            var entities = new HashSet<CollisionData>();

            for (int i = Min.gridX; i <= Max.gridX; i++) {
                for (int j = Min.gridZ; j <= Max.gridZ; j++) {
                    foreach (WorldEntity entity in grid[i][j]) {
                        if (!worldEntity.Equals(entity))
                            entities.Add(new CollisionData((i, j), (gridSizeX, gridSizeZ), entity));
                    }
                }
            }

            return entities.ToArray();
        }

        public void Delete(WorldEntity worldEntity) {
            var (Min, Max) = worldEntity.GetGridIndices();

            for (int i = Min.gridX; i <= Max.gridX; i++) {
                for (int j = Min.gridZ; j <= Max.gridZ; j++) {
                    grid[i][j].Remove(worldEntity);
                }
            }
        }
    }
}
