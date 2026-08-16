using UnityEngine;

namespace WorldGeneration
{
    /// <summary>
    /// Чанк - часть мира, содержащая сетку тайлов.
    /// Используется для оптимизации рендеринга и генерации.
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        public int chunkSizeX = 16;
        public int chunkSizeY = 16;
        
        public TileData[,] tiles;
        public Vector2Int chunkCoord; // Координаты чанка в мире
        
        private SpriteRenderer[,] spriteRenderers;
        private WorldGenerator worldGenerator;
        
        public void Initialize(Vector2Int coord, int sizeX, int sizeY, WorldGenerator generator)
        {
            chunkCoord = coord;
            chunkSizeX = sizeX;
            chunkSizeY = sizeY;
            worldGenerator = generator;
            
            tiles = new TileData[chunkSizeX, chunkSizeY];
            spriteRenderers = new SpriteRenderer[chunkSizeX, chunkSizeY];
            
            name = $"Chunk_{coord.x}_{coord.y}";
        }
        
        public void SetTile(int localX, int localY, TileData data)
        {
            if (localX < 0 || localX >= chunkSizeX || localY < 0 || localY >= chunkSizeY)
                return;
                
            tiles[localX, localY] = data;
            UpdateTileVisual(localX, localY);
        }
        
        public TileData GetTile(int localX, int localY)
        {
            if (localX < 0 || localX >= chunkSizeX || localY < 0 || localY >= chunkSizeY)
                return new TileData(TileType.Empty);
                
            return tiles[localX, localY];
        }
        
        public void UpdateTileVisual(int localX, int localY)
        {
            if (localX < 0 || localX >= chunkSizeX || localY < 0 || localY >= chunkSizeY)
                return;
                
            TileData data = tiles[localX, localY];
            
            // Позиция в мировом пространстве
            float worldX = chunkCoord.x * chunkSizeX + localX;
            float worldY = chunkCoord.y * chunkSizeY + localY;
            
            SpriteRenderer sr;
            
            // Создаем или получаем существующий SpriteRenderer
            if (spriteRenderers[localX, localY] == null)
            {
                GameObject tileObj = new GameObject($"Tile_{localX}_{localY}");
                tileObj.transform.parent = transform;
                tileObj.transform.localPosition = new Vector3(localX, localY, 0);
                
                sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Ground";
                spriteRenderers[localX, localY] = sr;
            }
            else
            {
                sr = spriteRenderers[localX, localY];
            }
            
            // Устанавливаем спрайт в зависимости от типа тайла
            Sprite sprite = worldGenerator.GetSpriteForTile(data.tileType);
            sr.sprite = sprite;
            
            // Скрываем пустые тайлы
            sr.enabled = data.tileType != TileType.Empty;
        }
        
        public void UpdateAllVisuals()
        {
            for (int x = 0; x < chunkSizeX; x++)
            {
                for (int y = 0; y < chunkSizeY; y++)
                {
                    UpdateTileVisual(x, y);
                }
            }
        }
    }
}
