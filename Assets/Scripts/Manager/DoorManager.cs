using Assets.Scripts.Manager;
using Assets.Scripts.SceneController;
using Assets.Scripts.Utilities;
using UnityEngine;

public class DoorManager : MonoBehaviour, IInteractable
{
    [SerializeField] private int targetNodeId = -1;
    [SerializeField] private bool isOpen = true;
    [SerializeField] private HallwayManager hallwayManager;

    public bool IsOpen => isOpen;
    public int TargetNodeId => targetNodeId;

    public void SetDoor(int nodeId, bool open)
    {
        targetNodeId = nodeId;
        isOpen = open;
        gameObject.name = open ? $"Door → Node {nodeId}" : "Door [Locked]";

        RefreshVisual();
    }

    public void Interact()
    {
        if (!isOpen)
        {
            Debug.Log("[DoorManager] This door is locked.");
            return;
        }

        ResolveHallwayManager();

        if (hallwayManager != null)
        {
            hallwayManager.OnDoorSelected(targetNodeId);
        }
        else
        {
            Debug.LogWarning("[DoorManager] No HallwayManager found. Falling back to direct scene load.");
            SceneController.Instance
                ?.NewTransition()
                .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Room, setActive: true)
                .WithOverlay()
                .Perform();
        }
    }

    private void RefreshVisual()
    {
        // TODO: Set concrete UI (sprite swap, collider toggle) done in the Unity Editor.
    }

    private void ResolveHallwayManager()
    {
        if (hallwayManager != null) return;
        hallwayManager = FindAnyObjectByType<HallwayManager>();
    }
}
