using Assets.Scripts.Utilities;

namespace Assets.Scripts.SaveSystem
{
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
