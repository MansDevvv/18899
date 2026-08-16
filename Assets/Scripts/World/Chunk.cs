using UnityEngine;

/// <summary>
/// Чанк мира - содержит тайлы и отвечает за их визуализацию.
/// Используется для оптимизации рендеринга больших миров.
/// </summary>
public class Chunk : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 16; // Размер чанка (16x16 тайлов)
    
    [HideInInspector]
    public int chunkCoordX;
    [HideInInspector]
    public int chunkCoordY;
    
    private GameObject[,] tileObjects;
    private Transform tilesParent;
    
    /// <summary>
    /// Инициализация чанка
    /// </summary>
    public void Initialize(int coordX, int coordY, int size)
    {
        chunkCoordX = coordX;
        chunkCoordY = coordY;
        chunkSize = size;
        
        tileObjects = new GameObject[chunkSize, chunkSize];
        
        // Создаем родительский объект для всех тайлов в чанке
        tilesParent = new GameObject($"Chunk_{coordX}_{coordY}_Tiles").transform;
        tilesParent.SetParent(transform);
    }
    
    /// <summary>
    /// Создание тайла в чанке
    /// </summary>
    public void CreateTile(int localX, int localY, GameObject prefab, TileType type)
    {
        if (localX < 0 || localX >= chunkSize || localY < 0 || localY >= chunkSize)
            return;
            
        // Удаляем старый тайл если есть
        if (tileObjects[localX, localY] != null)
        {
            if (Application.isPlaying)
                Destroy(tileObjects[localX, localY]);
            else
                DestroyImmediate(tileObjects[localX, localY]);
        }
        
        // Создаем новый тайл
        if (prefab != null)
        {
            Vector3 worldPos = new Vector3(
                chunkCoordX * chunkSize + localX,
                chunkCoordY * chunkSize + localY,
                0
            );
            
            GameObject tile = Instantiate(prefab, worldPos, Quaternion.identity, tilesParent);
            tile.name = $"Tile_{type}_{localX}_{localY}";
            tileObjects[localX, localY] = tile;
        }
    }
    
    /// <summary>
    /// Удаление тайла
    /// </summary>
    public void RemoveTile(int localX, int localY)
    {
        if (localX < 0 || localX >= chunkSize || localY < 0 || localY >= chunkSize)
            return;
            
        if (tileObjects[localX, localY] != null)
        {
            if (Application.isPlaying)
                Destroy(tileObjects[localX, localY]);
            else
                DestroyImmediate(tileObjects[localX, localY]);
                
            tileObjects[localX, localY] = null;
        }
    }
    
    /// <summary>
    /// Очистка всего чанка
    /// </summary>
    public void Clear()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                RemoveTile(x, y);
            }
        }
    }
    
    /// <summary>
    /// Получить глобальные координаты из локальных
    /// </summary>
    public Vector2Int GetWorldPosition(int localX, int localY)
    {
        return new Vector2Int(
            chunkCoordX * chunkSize + localX,
            chunkCoordY * chunkSize + localY
        );
    }
}
