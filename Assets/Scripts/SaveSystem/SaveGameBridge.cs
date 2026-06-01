namespace Assets.Scripts.SaveSystem
{
    public static class SaveGameBridge
    {
        public static string ActiveUsername { get; private set; }
        public static GameData ActiveGameData { get; private set; }

        public static bool HasActiveGameData => ActiveGameData != null;

        public static void SetActiveGameData(string username, GameData gameData)
        {
            ActiveUsername = username;
            ActiveGameData = gameData;
        }

        public static void Clear()
        {
            ActiveUsername = null;
            ActiveGameData = null;
        }

        public static void ClearActiveGameData() => Clear();
    }
}
