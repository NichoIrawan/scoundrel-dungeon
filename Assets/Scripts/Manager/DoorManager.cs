using Assets.Scripts;
using Assets.Scripts.SceneController;
using Unity.VisualScripting;
using UnityEngine;

public class DoorManager : MonoBehaviour, IInteractable
{
    [SerializeField] private int roomId;

    public void Interact()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Room, setActive: true)
            .WithOverlay()
            .Perform();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
