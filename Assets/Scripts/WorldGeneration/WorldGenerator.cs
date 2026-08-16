using UnityEngine;
using System.Collections.Generic;

namespace WorldGeneration
{
    /// <summary>
    /// Основной класс генерации мира с поддержкой чанков.
    /// Настраивается через инспектор Unity.
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Размеры мира")]
        [Tooltip("Ширина мира в тайлах")]
        public int worldWidth = 100;
        
        [Tooltip("Высота мира в тайлах")]
        public int worldHeight = 100;
        
        [Header("Размеры чанка")]
        [Tooltip("Ширина одного чанка в тайлах")]
        public int chunkSizeX = 16;
        
        [Tooltip("Высота одного чанка в тайлах")]
        public int chunkSizeY = 16;
        
        [Header("Спрайты для тайлов")]
        [Tooltip("Спрайт земли (пол)")]
        public Sprite groundSprite;
        
        [Tooltip("Спрайт каменного пола")]
        public Sprite stoneFloorSprite;
        
        [Tooltip("Спрайт дерева типа 1")]
        public Sprite woodType1Sprite;
        
        [Tooltip("Спрайт дерева типа 2")]
        public Sprite woodType2Sprite;
        
        [Tooltip("Спрайт камня типа 1")]
        public Sprite stoneType1Sprite;
        
        [Tooltip("Спрайт камня типа 2")]
        public Sprite stoneType2Sprite;
        
        [Tooltip("Спрайт травы типа 1")]
        public Sprite grassType1Sprite;
        
        [Tooltip("Спрайт травы типа 2")]
        public Sprite grassType2Sprite;
        
        [Tooltip("Спрайт декоративного камня 1")]
        public Sprite rockDecor1Sprite;
        
        [Tooltip("Спрайт декоративного камня 2")]
        public Sprite rockDecor2Sprite;
        
        [Header("Параметры генерации")]
        [Range(0f, 1f)]
        [Tooltip("Шанс появления дерева")]
        public float treeSpawnChance = 0.1f;
        
        [Range(0f, 1f)]
        [Tooltip("Шанс появления камня")]
        public float stoneSpawnChance = 0.05f;
        
        [Range(0f, 1f)]
        [Tooltip("Шанс появления декоративной травы")]
        public float grassSpawnChance = 0.15f;
        
        [Range(0f, 1f)]
        [Tooltip("Шанс появления декоративного камня")]
        public float rockDecorSpawnChance = 0.03f;
        
        [Header("Настройки шума Перлина")]
        [Tooltip("Масштаб шума для ландшафта")]
        public float noiseScale = 0.1f;
        
        [Tooltip("Смещение по X для шума")]
        public float noiseOffsetX = 0f;
        
        [Tooltip("Смещение по Y для шума")]
        public float noiseOffsetY = 0f;
        
        [Tooltip("Порог для каменного пола")]
        public float stoneFloorThreshold = 0.7f;
        
        private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
        private int totalChunksX;
        private int totalChunksY;
        
        void Start()
        {
            GenerateWorld();
        }
        
        /// <summary>
        /// Генерирует весь мир, разбивая его на чанки.
        /// </summary>
        public void GenerateWorld()
        {
            // Очищаем старый мир
            ClearWorld();
            
            // Вычисляем количество чанков
            totalChunksX = Mathf.CeilToInt((float)worldWidth / chunkSizeX);
            totalChunksY = Mathf.CeilToInt((float)worldHeight / chunkSizeY);
            
            Debug.Log($"Генерация мира {worldWidth}x{worldHeight} тайлов ({totalChunksX}x{totalChunksY} чанков)");
            
            // Генерируем каждый чанк
            for (int cx = 0; cx < totalChunksX; cx++)
            {
                for (int cy = 0; cy < totalChunksY; cy++)
                {
                    GenerateChunk(cx, cy);
                }
            }
            
            Debug.Log("Генерация мира завершена!");
        }
        
        /// <summary>
        /// Генерирует один чанк.
        /// </summary>
        private void GenerateChunk(int chunkX, int chunkY)
        {
            // Создаем GameObject для чанка
            GameObject chunkObj = new GameObject($"Chunk_{chunkX}_{chunkY}");
            chunkObj.transform.parent = transform;
            
            // Добавляем компонент Chunk и инициализируем его
            Chunk chunk = chunkObj.AddComponent<Chunk>();
            Vector2Int chunkCoord = new Vector2Int(chunkX, chunkY);
            chunk.Initialize(chunkCoord, chunkSizeX, chunkSizeY, this);
            
            // Заполняем чанк тайлами
            for (int x = 0; x < chunkSizeX; x++)
            {
                for (int y = 0; y < chunkSizeY; y++)
                {
                    // Глобальные координаты тайла
                    int globalX = chunkX * chunkSizeX + x;
                    int globalY = chunkY * chunkSizeY + y;
                    
                    // Проверяем границы мира
                    if (globalX >= worldWidth || globalY >= worldHeight)
                    {
                        chunk.SetTile(x, y, new TileData(TileType.Empty));
                        continue;
                    }
                    
                    // Генерируем тайл
                    TileData tileData = GenerateTile(globalX, globalY);
                    chunk.SetTile(x, y, tileData);
                }
            }
            
            // Сохраняем ссылку на чанк
            chunks[chunkCoord] = chunk;
        }
        
        /// <summary>
        /// Генерирует данные для одного тайла на основе шума Перлина и вероятностей.
        /// </summary>
        private TileData GenerateTile(int x, int y)
        {
            // Получаем значение шума Перлина для этого тайла
            float noiseValue = Mathf.PerlinNoise(
                (x + noiseOffsetX) * noiseScale,
                (y + noiseOffsetY) * noiseScale
            );
            
            // Определяем базовый тип поверхности
            TileType baseType = TileType.Ground;
            if (noiseValue > stoneFloorThreshold)
            {
                baseType = TileType.StoneFloor;
            }
            
            // Создаем тайл с базовым типом
            TileData tile = new TileData(baseType);
            
            // Проверяем возможность добавления объектов (деревья, камни, трава)
            float spawnRandom = Random.value;
            
            // Шанс появления дерева
            if (spawnRandom < treeSpawnChance && baseType == TileType.Ground)
            {
                tile.tileType = Random.value < 0.5f ? TileType.WoodType1 : TileType.WoodType2;
                tile.resourceAmount = Random.Range(10, 30); // Количество древесины
            }
            // Шанс появления камня-ресурса
            else if (spawnRandom < treeSpawnChance + stoneSpawnChance && baseType == TileType.Ground)
            {
                tile.tileType = Random.value < 0.5f ? TileType.StoneType1 : TileType.StoneType2;
                tile.resourceAmount = Random.Range(15, 50); // Количество камня
            }
            // Шанс появления декоративной травы
            else if (spawnRandom < treeSpawnChance + stoneSpawnChance + grassSpawnChance)
            {
                tile.tileType = Random.value < 0.5f ? TileType.GrassType1 : TileType.GrassType2;
            }
            // Шанс появления декоративного камня
            else if (spawnRandom < treeSpawnChance + stoneSpawnChance + grassSpawnChance + rockDecorSpawnChance)
            {
                tile.tileType = Random.value < 0.5f ? TileType.RockDecor1 : TileType.RockDecor2;
            }
            
            return tile;
        }
        
        /// <summary>
        /// Возвращает спрайт для указанного типа тайла.
        /// </summary>
        public Sprite GetSpriteForTile(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Ground:
                    return groundSprite;
                case TileType.StoneFloor:
                    return stoneFloorSprite;
                case TileType.WoodType1:
                    return woodType1Sprite;
                case TileType.WoodType2:
                    return woodType2Sprite;
                case TileType.StoneType1:
                    return stoneType1Sprite;
                case TileType.StoneType2:
                    return stoneType2Sprite;
                case TileType.GrassType1:
                    return grassType1Sprite;
                case TileType.GrassType2:
                    return grassType2Sprite;
                case TileType.RockDecor1:
                    return rockDecor1Sprite;
                case TileType.RockDecor2:
                    return rockDecor2Sprite;
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Получает данные тайла по глобальным координатам.
        /// </summary>
        public TileData GetTileAt(int globalX, int globalY)
        {
            if (globalX < 0 || globalX >= worldWidth || globalY < 0 || globalY >= worldHeight)
                return new TileData(TileType.Empty);
            
            int chunkX = globalX / chunkSizeX;
            int chunkY = globalY / chunkSizeY;
            int localX = globalX % chunkSizeX;
            int localY = globalY % chunkSizeY;
            
            Vector2Int chunkCoord = new Vector2Int(chunkX, chunkY);
            if (chunks.ContainsKey(chunkCoord))
            {
                return chunks[chunkCoord].GetTile(localX, localY);
            }
            
            return new TileData(TileType.Empty);
        }
        
        /// <summary>
        /// Очищает весь мир.
        /// </summary>
        private void ClearWorld()
        {
            foreach (var chunk in chunks.Values)
            {
                if (chunk != null)
                {
                    DestroyImmediate(chunk.gameObject);
                }
            }
            chunks.Clear();
        }
        
        /// <summary>
        /// Регенерирует мир (вызывается из инспектора или кода).
        /// </summary>
        [ContextMenu("Regenerate World")]
        public void RegenerateWorld()
        {
            GenerateWorld();
        }
    }
}
