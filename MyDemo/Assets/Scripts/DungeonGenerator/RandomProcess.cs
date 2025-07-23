
public class RandomProcess
{
    public static RoomType GetRandomRoomType()
    {
        return (RoomType)UnityEngine.Random.Range(1, 5);
    }
}