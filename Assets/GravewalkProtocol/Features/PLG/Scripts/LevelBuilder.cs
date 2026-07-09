using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] LayoutGeneratorRooms layoutGeneratorRooms;
    [SerializeField] MarchingSquares marchingSquares;
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] RoomDecorator roomDecorator;

    public Vector3 playerStart;

    void Start()
    {
        GenerateRandom();
    }

    [ContextMenu("Generate Random")]
    public void GenerateRandom()
    {
        SharedLevelData.Instance.GenerateSeed();
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
        Vector3 playerPosition = LevelPositionToWorldPosition(roomCenter);
        playerStart = playerPosition;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        NavMeshAgent playerNavMeshAgent = player.GetComponent<NavMeshAgent>();
        

        GameObject PSP = GameObject.FindGameObjectWithTag("PlayerSpawnPos");
        PSP.transform.position = playerPosition;
    }

    Vector3 LevelPositionToWorldPosition(Vector2 levelPosition)
    {
        int scale = SharedLevelData.Instance.Scale;
        return new Vector3((levelPosition.x - 1) * scale, 1, (levelPosition.y - 1) * scale);
    }
    
}