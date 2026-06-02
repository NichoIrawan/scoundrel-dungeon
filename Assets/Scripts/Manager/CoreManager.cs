using Assets.Scripts;
using Assets.Scripts.SceneController;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    [SerializeField] private EncounterRegistry registry;
    private void Start()
    {
        registry.Initialize();
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Perform();
    }
}
