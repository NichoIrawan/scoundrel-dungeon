using Assets.Scripts.Manager;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.SceneController;
using UnityEngine;

public class HallwayManager : MonoBehaviour
{
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private RunManager runManager;
    [SerializeField] private DoorManager leftDoor;
    [SerializeField] private DoorManager rightDoor;

    private void Start()
    {
        ResolveReferences();
        RefreshDoors();
    }

    public void RefreshDoors()
    {
        ResolveReferences();
        if (dungeonManager == null) return;

        var state = dungeonManager.State;
        if (!state.Nodes.TryGetValue(dungeonManager.CurrentNode, out var currentNode)) return;

        int leftId = currentNode.LeftNode;
        int rightId = currentNode.RightNode;

        bool leftOpen = leftId >= 0 && state.Nodes.ContainsKey(leftId);
        bool rightOpen = rightId >= 0 && state.Nodes.ContainsKey(rightId);

        // If only one path exists (merge node), show it as Left and lock Right
        if (leftOpen && !rightOpen)
        {
            leftDoor?.SetDoor(leftId, open: true);
            rightDoor?.SetDoor(-1, open: false);
        }
        else if (!leftOpen && rightOpen)
        {
            leftDoor?.SetDoor(rightId, open: true);
            rightDoor?.SetDoor(-1, open: false);
        }
        else
        {
            leftDoor?.SetDoor(leftId, open: leftOpen);
            rightDoor?.SetDoor(rightId, open: rightOpen);
        }
    }

    public void OnDoorSelected(int targetNodeId)
    {
        ResolveReferences();
        if (dungeonManager == null) return;

        // Entering the Exit node
        if (targetNodeId == 6)
        {
            HandleExitReached();
            return;
        }

        dungeonManager.AdvanceToRoom(targetNodeId);
        EnterRoom();
    }

    public bool TryEnterNextRoom()
    {
        ResolveReferences();
        if (dungeonManager == null) return false;

        dungeonManager.AdvanceToNextRoom();
        return EnterRoom();
    }

    public int GetCurrentNode()
    {
        ResolveReferences();
        return dungeonManager != null ? dungeonManager.CurrentNode : -1;
    }

    public (int monsters, int weapons, int potions) GetDeckStats()
    {
        ResolveReferences();
        return dungeonManager?.GetRemainingDeckStats() ?? (0, 0, 0);
    }

        private void HandleExitReached()
    {
        if (dungeonManager.IsQueueEmpty())
        {
            HandleVictory();
        }
        else
        {
            DataPersistenceManager.Instance?.AutoSave();
            dungeonManager.DequeueNextUnresolvedRoom();
            EnterRoom();
        }
    }

    private void HandleVictory()
    {
        SceneController.Instance
            ?.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Unload(SceneDatabase.Slots.Run)
            .Unload(SceneDatabase.Slots.Phases)
            .WithOverlay()
            .Perform();
    }

    private bool EnterRoom()
    {
        return SceneController.Instance != null &&
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Room, setActive: true)
                .WithOverlay()
                .Perform() != null;
    }

    private void ResolveReferences()
    {
        if (dungeonManager == null) dungeonManager = FindAnyObjectByType<DungeonManager>();
        if (runManager == null) runManager = FindAnyObjectByType<RunManager>();
    }
}
