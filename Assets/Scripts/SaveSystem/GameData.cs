using System;
using Assets.Scripts.Utilities;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class GameData
    {
        public int Version = 1;
        public string Username;
        public string Seed;
        public int RoomUnit;
        public int CurrentRoomIndex;
        public string UpdatedAtUtc;
        public SerializedDictionary<int, string[]> EffigiesInRoom = new();
        public SerializedDictionary<string, int> EffigiesInRun = new();

        public GameData()
        {
            Seed = "default_seed";
            RoomUnit = 7;
            CurrentRoomIndex = 0;
        }
    }
}
