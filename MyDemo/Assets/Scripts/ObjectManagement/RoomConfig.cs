using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "RoomConfig", menuName = "Configs/Room Config")]
public class RoomConfig : ScriptableObject
{
    public string roomName;

    public float objectDensity;

    public bool treasureChestRoom;

    public bool monsterRoom;

    public bool keyRoom;
}
