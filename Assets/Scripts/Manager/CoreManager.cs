using Assets.Scripts.SceneController;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    private void Start()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Perform();
    }
}
