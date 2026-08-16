using UnityEngine;

/// <summary>
/// Простой скрипт для тайла, можно расширять функциональностью
/// </summary>
public class Tile : MonoBehaviour
{
    [Header("Tile Info")]
    public TileType tileType;
    
    [Header("Walkable")]
    public bool isWalkable = true;
    
    [Header("Buildable")]
    public bool isBuildable = true;
    
    /// <summary>
    /// Инициализация тайла
    /// </summary>
    public void Initialize(TileType type)
    {
        tileType = type;
        
        // Настраиваем проходимость в зависимости от типа
        switch (type)
        {
            case TileType.Ground:
            case TileType.StoneFloor:
                isWalkable = true;
                isBuildable = true;
                break;
                
            case TileType.Tree1:
            case TileType.Tree2:
            case TileType.Rock1:
            case TileType.Rock2:
                isWalkable = false;
                isBuildable = false;
                break;
                
            case TileType.Grass1:
            case TileType.Grass2:
            case TileType.DecorStone1:
            case TileType.DecorStone2:
                isWalkable = true;
                isBuildable = false;
                break;
        }
    }
}
