using Assets.Scripts.SceneController;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Hallway, setActive: true)
            .Load(SceneDatabase.Slots.Run, SceneDatabase.Scenes.Run)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .Perform();
    }

    public void OnContinueClicked()
    {

    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
