using Assets.Scripts.Manager;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.SceneController;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button _continueButton;

    private void Start()
    {
    }

    private void Update()
    {
        if (_continueButton != null && DataPersistenceManager.Instance != null)
        {
            _continueButton.interactable = DataPersistenceManager.Instance.HasGameData;
        }
    }

    public void OnLogoutClicked()
    {
        DataPersistenceManager.Instance.Logout();
    }

    public void OnNewGameClicked()
    {
        var seed = System.DateTime.UtcNow.Ticks.ToString();
        var persistenceManager = DataPersistenceManager.Instance;
        persistenceManager.NewGame();

        // Store seed in GameData for DungeonManager to use when loading
        SaveGameBridge.ActiveGameData.Seed = seed;
        persistenceManager.SaveGame();

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
}
