using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] LayoutGeneratorRooms layoutGeneratorRooms;
    [SerializeField] MarchingSquares marchingSquares;
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] RoomDecorator roomDecorator;

    void Start()
    {
        //GenerateRandom();
    }

    [ContextMenu("Generate Random")]
    public void GenerateRandom()
    {
        SharedLevelData.Instance.GenerateSeed();
        Generate();
    }

    [ContextMenu("Generate From Seed")]
     public void GenerateFromSeed()
    {
        SharedLevelData.Instance.SetSeed(SharedLevelData.Instance.levelSeed);
        Generate();
    }
    [ContextMenu("Generate")]
    public void Generate()
    {
        Level level = layoutGeneratorRooms.GenerateLevel();
        marchingSquares.CreateLevelGeometry();
        roomDecorator.PlaceItems(level);
        navMeshSurface.BuildNavMesh();
        Room startRoom = level.PlayerStartRoom;
        Vector2 roomCenter = startRoom.Area.center;
        gameManager.instance.playerSpawnPos.transform.position = LevelPositionToWorldPosition(roomCenter);


    }

    public Vector3 LevelPositionToWorldPosition(Vector2 levelPosition)
    {
        int scale = SharedLevelData.Instance.Scale;
        return new Vector3((levelPosition.x - 1) * scale, 3, (levelPosition.y - 1) * scale);
    }
    
}