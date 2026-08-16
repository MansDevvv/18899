using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Генератор мира с системой чанков для оптимизации.
/// Поддерживает генерацию через шум Перлина и настройку размера мира в инспекторе.
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [Tooltip("Ширина мира в тайлах")]
    public int worldWidth = 100;
    
    [Tooltip("Высота мира в тайлах")]
    public int worldHeight = 100;
    
    [Header("Chunk Settings")]
    [Tooltip("Размер чанка (например 16 = чанк 16x16 тайлов)")]
    public int chunkSize = 16;
    
    [Header("Tile Prefabs")]
    [Tooltip("Префабы для каждого типа тайла")]
    public List<TilePrefab> tilePrefabs = new List<TilePrefab>();
    
    [Header("Generation Settings")]
    [Tooltip("Сид для генерации (одинаковый сид = одинаковый мир)")]
    public int seed = 12345;
    
    [Tooltip("Частота шума для ландшафта")]
    [Range(0.01f, 0.5f)]
    public float noiseScale = 0.1f;
    
    [Tooltip("Высота порога для камня")]
    [Range(0f, 1f)]
    public float stoneThreshold = 0.6f;
    
    [Tooltip("Шанс появления дерева (0-1)")]
    [Range(0f, 0.5f)]
    public float treeChance = 0.1f;
    
    [Tooltip("Шанс появления декоративной травы")]
    [Range(0f, 0.3f)]
    public float grassChance = 0.15f;
    
    [Tooltip("Шанс появления декоративного камня")]
    [Range(0f, 0.1f)]
    public float decorStoneChance = 0.05f;
    
    [Tooltip("Порог для выбора между деревом 1 и 2")]
    [Range(0f, 1f)]
    public float treeTypeThreshold = 0.5f;
    
    [Tooltip("Порог для выбора между камнем ресурсом 1 и 2")]
    [Range(0f, 1f)]
    public float rockTypeThreshold = 0.5f;
    
    [Tooltip("Порог для выбора между травой 1 и 2")]
    [Range(0f, 1f)]
    public float grassTypeThreshold = 0.5f;
    
    [Tooltip("Порог для выбора между декоративным камнем 1 и 2")]
    [Range(0f, 1f)]
    public float decorStoneTypeThreshold = 0.5f;
    
    // Словарь для быстрого доступа к префабам по типу тайла
    private Dictionary<TileType, GameObject> prefabDictionary;
    
    // Чанки мира
    private Dictionary<Vector2Int, Chunk> chunks;
    
    // Количество чанков по осям
    private int chunksX;
    private int chunksY;
    
    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        // Очищаем старый мир
        ClearWorld();
        
        // Инициализируем словарь префабов
        InitializePrefabDictionary();
        
        // Вычисляем количество чанков
        chunksX = Mathf.CeilToInt((float)worldWidth / chunkSize);
        chunksY = Mathf.CeilToInt((float)worldHeight / chunkSize);
        
        chunks = new Dictionary<Vector2Int, Chunk>();
        
        // Инициализация Random с сидом для воспроизводимости
        Random.InitState(seed);
        
        // Создаем и заполняем чанки
        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                CreateChunk(cx, cy);
            }
        }
        
        Debug.Log($"Мир сгенерирован: {worldWidth}x{worldHeight} тайлов, {chunks.Count} чанков");
    }
    
    /// <summary>
    /// Создание одного чанка
    /// </summary>
    private void CreateChunk(int chunkX, int chunkY)
    {
        // Создаем объект чанка
        GameObject chunkObj = new GameObject($"Chunk_{chunkX}_{chunkY}");
        chunkObj.transform.SetParent(transform);
        
        Chunk chunk = chunkObj.AddComponent<Chunk>();
        chunk.Initialize(chunkX, chunkY, chunkSize);
        
        Vector2Int chunkKey = new Vector2Int(chunkX, chunkY);
        chunks[chunkKey] = chunk;
        
        // Заполняем чанк тайлами
        FillChunk(chunk, chunkX, chunkY);
    }
    
    /// <summary>
    /// Заполнение чанка тайлами на основе шума Перлина
    /// </summary>
    private void FillChunk(Chunk chunk, int chunkX, int chunkY)
    {
        int actualWidth = Mathf.Min(chunkSize, worldWidth - chunkX * chunkSize);
        int actualHeight = Mathf.Min(chunkSize, worldHeight - chunkY * chunkSize);
        
        for (int x = 0; x < actualWidth; x++)
        {
            for (int y = 0; y < actualHeight; y++)
            {
                int worldX = chunkX * chunkSize + x;
                int worldY = chunkY * chunkSize + y;
                
                // Получаем тип тайла через шум
                TileType tileType = GetTileTypeForPosition(worldX, worldY);
                
                // Получаем префаб для этого типа
                if (prefabDictionary.TryGetValue(tileType, out GameObject prefab))
                {
                    chunk.CreateTile(x, y, prefab, tileType);
                }
            }
        }
    }
    
    /// <summary>
    /// Определение типа тайла для позиции на основе шума Перлина
    /// </summary>
    private TileType GetTileTypeForPosition(int x, int y)
    {
        // Генерируем несколько значений шума для разных характеристик
        float terrainNoise = Mathf.PerlinNoise(
            (x + seed) * noiseScale, 
            (y + seed) * noiseScale
        );
        
        float detailNoise1 = Mathf.PerlinNoise(
            (x + seed) * noiseScale * 2.5f, 
            (y + seed) * noiseScale * 2.5f
        );
        
        float detailNoise2 = Mathf.PerlinNoise(
            (x + seed) * noiseScale * 4f, 
            (y + seed) * noiseScale * 4f
        );
        
        // Определяем базовый тип поверхности
        if (terrainNoise > stoneThreshold)
        {
            // Каменный пол
            return TileType.StoneFloor;
        }
        
        // Земля - проверяем дополнительные объекты
        float randomValue = Random.value;
        
        // Проверяем деревья
        if (randomValue < treeChance)
        {
            return detailNoise1 > treeTypeThreshold ? TileType.Tree2 : TileType.Tree1;
        }
        
        // Проверяем камни-ресурсы
        if (randomValue < treeChance + 0.03f)
        {
            return detailNoise1 > rockTypeThreshold ? TileType.Rock2 : TileType.Rock1;
        }
        
        // Проверяем декоративную траву
        if (randomValue < treeChance + 0.03f + grassChance)
        {
            return detailNoise2 > grassTypeThreshold ? TileType.Grass2 : TileType.Grass1;
        }
        
        // Проверяем декоративные камни
        if (randomValue < treeChance + 0.03f + grassChance + decorStoneChance)
        {
            return detailNoise2 > decorStoneTypeThreshold ? TileType.DecorStone2 : TileType.DecorStone1;
        }
        
        // По умолчанию земля
        return TileType.Ground;
    }
    
    /// <summary>
    /// Инициализация словаря префабов
    /// </summary>
    private void InitializePrefabDictionary()
    {
        prefabDictionary = new Dictionary<TileType, GameObject>();
        
        foreach (var tilePrefab in tilePrefabs)
        {
            if (!prefabDictionary.ContainsKey(tilePrefab.tileType))
            {
                prefabDictionary[tilePrefab.tileType] = tilePrefab.prefab;
            }
            else
            {
                Debug.LogWarning($"Дубликат префаба для типа {tilePrefab.tileType}");
            }
        }
    }
    
    /// <summary>
    /// Очистка всего мира
    /// </summary>
    public void ClearWorld()
    {
        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
    }
    
    [ContextMenu("Regenerate World")]
    private void OnValidate()
    {
        // Валидация параметров
        worldWidth = Mathf.Max(1, worldWidth);
        worldHeight = Mathf.Max(1, worldHeight);
        chunkSize = Mathf.Max(1, chunkSize);
    }
}
