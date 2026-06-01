using Assets.Scripts.Manager;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.SceneController;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private RunManager runManager;

    private void Start()
    {
        ResolveReferences();
    }

    public bool OnRegisterClicked(string username, string password)
    {
        try
        {
            DataPersistenceManager.Instance.CreateAccount(username, password);
            DataPersistenceManager.Instance.Login(username, password);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenuManager] Register failed: {ex.Message}");
            return false;
        }
    }

    public bool OnLoginClicked(string username, string password)
    {
        try
        {
            DataPersistenceManager.Instance.Login(username, password);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenuManager] Login failed: {ex.Message}");
            return false;
        }
    }

    public void OnLogoutClicked()
    {
        DataPersistenceManager.Instance.Logout();
    }

    public void OnNewGameClicked()
    {
        ResolveReferences();

        var seed = System.DateTime.UtcNow.Ticks.ToString();
        dungeonManager?.GenerateDungeon(seed);
        runManager?.InitializeNewRun();
        DataPersistenceManager.Instance.SaveGame();
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Run, SceneDatabase.Scenes.Run)
            .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Hallway, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .Perform();
    }

    public void OnContinueClicked()
    {
        if (!DataPersistenceManager.Instance.HasGameData)
        {
            Debug.LogWarning("[MainMenuManager] No save data found. Cannot continue.");
            return;
        }

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Run, SceneDatabase.Scenes.Run)
            .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Hallway, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .Perform();
    }

    public void OnLanguageSelected(string locale)
    {
        LocalizationManager.Instance?.SwitchLanguage(locale);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    private void ResolveReferences()
    {
        if (dungeonManager == null) dungeonManager = FindAnyObjectByType<DungeonManager>();
        if (runManager == null) runManager = FindAnyObjectByType<RunManager>();
    }
}
