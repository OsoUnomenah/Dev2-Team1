using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] LayoutGeneratorRooms layoutGeneratorRooms;
    [SerializeField] MarchingSquares marchingSquares;
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] RoomDecorator roomDecorator;
    [SerializeField] Room startRoom;
    [SerializeField] Vector2 startRoomCenter;
    [SerializeField] Vector3 playerStartPos;

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
        startRoom = level.Rooms[0];

    }

    public Vector3 LevelPositionToWorldPosition(Vector2 levelPosition)
    {
        int scale = SharedLevelData.Instance.Scale;
        return new Vector3((levelPosition.x - 1) * scale, 3, (levelPosition.y - 1) * scale);
    }

    /// <summary>
    /// Returns the center position of the start room in world coordinates. If no start room is found, returns Vector3.zero.
    /// </summary>
    /// <returns></returns>
    /// 
    public Vector3 GetStartRoomCenterPos()
    {
        if (startRoom != null)
        {
            startRoomCenter = startRoom.Area.center;
            playerStartPos = LevelPositionToWorldPosition(startRoomCenter);
            return playerStartPos;
        }
        else
        {
            Debug.Log("No start room found, returning zero vector");
            return Vector3.zero;
        }
       
    }
    
}