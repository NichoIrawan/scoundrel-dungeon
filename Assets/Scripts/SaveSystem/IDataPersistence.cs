namespace Assets.Scripts.SaveSystem
{
    public interface IDataPersistence
    {
        void LoadData(GameData data);
        void SaveData(ref GameData data);
    }
}
