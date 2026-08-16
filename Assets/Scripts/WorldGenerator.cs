using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    [Header("Размер мира")]
    public int worldWidth = 100;
    public int worldHeight = 100;

    [Header("Настройки чанков")]
    public int chunkSize = 16;

    [Header("Спрайты тайлов")]
    public Sprite groundSprite;      // Земля/пол (базовый)
    public Sprite stoneSprite1;      // Камень 1
    public Sprite stoneSprite2;      // Камень 2
    public Sprite woodSprite1;       // Дерево 1
    public Sprite woodSprite2;       // Дерево 2
    public Sprite grassSprite1;      // Трава 1 (декор)
    public Sprite grassSprite2;      // Трава 2 (декор)
    public Sprite rockSprite1;       // Декоративный камень 1
    public Sprite rockSprite2;       // Декоративный камень 2

    [Header("Параметры генерации")]
    [Range(0f, 1f)]
    public float stoneFrequency = 0.15f;
    [Range(0f, 1f)]
    public float woodFrequency = 0.08f;
    [Range(0f, 1f)]
    public float grassFrequency = 0.12f;
    [Range(0f, 1f)]
    public float rockFrequency = 0.05f;
    
    public int seed = -1;

    private List<GameObject> chunks = new List<GameObject>();

    void Awake()
    {
        if (seed == -1)
            seed = Random.Range(0, 10000);
        
        GenerateWorld();
    }

    [ContextMenu("Генерировать мир")]
    public void GenerateWorld()
    {
        ClearWorld();
        
        System.Random random = new System.Random(seed);
        float[,] noiseMap = GenerateNoiseMap(worldWidth, worldHeight, random.Next());

        for (int chunkX = 0; chunkX < Mathf.CeilToInt((float)worldWidth / chunkSize); chunkX++)
        {
            for (int chunkY = 0; chunkY < Mathf.CeilToInt((float)worldHeight / chunkSize); chunkY++)
            {
                CreateChunk(chunkX, chunkY, noiseMap);
            }
        }
    }

    void ClearWorld()
    {
        foreach (GameObject chunk in chunks)
        {
            if (chunk != null)
                DestroyImmediate(chunk);
        }
        chunks.Clear();
    }

    void CreateChunk(int chunkX, int chunkY, float[,] noiseMap)
    {
        GameObject chunkObj = new GameObject($"Chunk_{chunkX}_{chunkY}");
        chunkObj.transform.parent = transform;
        chunkObj.layer = LayerMask.NameToLayer("Ground");
        
        int startX = chunkX * chunkSize;
        int startY = chunkY * chunkSize;
        int width = Mathf.Min(chunkSize, worldWidth - startX);
        int height = Mathf.Min(chunkSize, worldHeight - startY);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;
                
                float noiseValue = noiseMap[worldX, worldY];
                
                // Базовая земля
                GameObject tile = CreateTile(new Vector3Int(worldX, worldY, 0), groundSprite, chunkObj.transform);
                
                // Добавляем объекты на основе шума и частоты
                if (noiseValue > 1f - stoneFrequency && stoneSprite1 != null)
                {
                    SetTileSprite(tile, Random.value > 0.5f ? stoneSprite1 : stoneSprite2);
                }
                else if (noiseValue > 1f - stoneFrequency - woodFrequency && woodSprite1 != null)
                {
                    SetTileSprite(tile, Random.value > 0.5f ? woodSprite1 : woodSprite2);
                }
                else if (Random.value < grassFrequency && grassSprite1 != null)
                {
                    SetTileSprite(tile, Random.value > 0.5f ? grassSprite1 : grassSprite2);
                }
                else if (Random.value < rockFrequency && rockSprite1 != null)
                {
                    SetTileSprite(tile, Random.value > 0.5f ? rockSprite1 : rockSprite2);
                }
            }
        }
        
        chunks.Add(chunkObj);
    }

    GameObject CreateTile(Vector3Int position, Sprite sprite, Transform parent)
    {
        GameObject tile = new GameObject($"Tile_{position.x}_{position.y}");
        tile.transform.parent = parent;
        tile.transform.localPosition = new Vector3(position.x, position.y, 0);
        
        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "Ground";
        
        return tile;
    }

    void SetTileSprite(GameObject tile, Sprite sprite)
    {
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer != null && sprite != null)
            renderer.sprite = sprite;
    }

    float[,] GenerateNoiseMap(int width, int height, int seed)
    {
        float[,] noiseMap = new float[width, height];
        System.Random random = new System.Random(seed);
        
        float scale = 0.1f;
        int octaves = 4;
        float persistence = 0.5f;
        float lacunarity = 2f;
        
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-10000, 10000);
            float offsetY = random.Next(-10000, 10000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + octaveOffsets[i].x) * scale * frequency;
                    float sampleY = (y + octaveOffsets[i].y) * scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Нормализация
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    void OnValidate()
    {
        // Обновление параметров в инспекторе
    }
}
