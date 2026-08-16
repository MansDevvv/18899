using UnityEngine;

namespace WorldGeneration
{
    [System.Serializable]
    public struct TileData
    {
        public TileType tileType;
        public int resourceAmount; // Количество ресурса (для деревьев и камней)
        
        public TileData(TileType type, int amount = 0)
        {
            tileType = type;
            resourceAmount = amount;
        }
    }
}
