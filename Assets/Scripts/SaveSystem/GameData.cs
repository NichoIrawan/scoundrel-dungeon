using System;
using Assets.Scripts.Utilities;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class MetaData
    {
        public int SaveVersion = 1;
        public string SaveDate;
    }

    [Serializable]
    public class PlayerState
    {
        public int Health = 20;
        public string EquippedWeaponId;
        public string StoredPotionId;
        public int LastDefeatedEnemyStrength;
        public bool PotionUsedThisRoom;
    }

    [Serializable]
    public class RunState
    {
        public PlayerState Player = new();
    }

    [Serializable]
    public class DungeonNode
    {
        public int NodeId;
        public string[] EncounterIds = new string[4];
        public int LeftNode = -1;
        public int RightNode = -1;
        public bool IsResolved;
        public bool IsEscaped;
        public string[] SelectedEncounterIds = Array.Empty<string>();
    }

    [Serializable]
    public class DungeonState
    {
        public SerializedDictionary<int, DungeonNode> Nodes = new();
        public int CurrentNode;
        public int[] UnresolvedNodes = Array.Empty<int>();
    }

    [Serializable]
    public class GameData
    {
        public int Version = 1;
        public string Username;
        public string Seed;
        public int RoomUnit;
        public int CurrentRoomIndex;
        public string UpdatedAtUtc;
        public MetaData MetaData = new();
        public RunState RunState = new();
        public DungeonState DungeonState = new();

        // Legacy fields are kept so older saves can be upgraded into DungeonState.
        public SerializedDictionary<int, string[]> EffigiesInRoom = new();
        public SerializedDictionary<string, int> EffigiesInRun = new();
        public SerializedDictionary<int, string[]> SelectedEffigiesByRoom = new();

        public GameData()
        {
            Seed = "default_seed";
            RoomUnit = 7;
            CurrentRoomIndex = 0;
        }
    }
}
