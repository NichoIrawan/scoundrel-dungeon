using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _effigiesSlot;
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] public List<GameObject> SelectedEffigies = new();

        private readonly List<string> selectedEffigyNames = new();

        public IReadOnlyList<string> SelectedEffigyNames => selectedEffigyNames;

        private void Start()
        {
            PopulateCurrentRoom();
        }

        public void PopulateCurrentRoom()
        {
            ResolveDungeonManager();

            if (dungeonManager == null)
            {
                Debug.LogWarning("RoomManager could not find a DungeonManager.");
                return;
            }

            SelectedEffigies.Clear();
            selectedEffigyNames.Clear();

            var roomEffigies = dungeonManager.GetCurrentRoomEncounterIds();
            var savedSelection = new HashSet<string>(dungeonManager.GetSelectedEncounterIdsForCurrentRoom());

            for (int i = 0; i < _effigiesSlot.Count; i++)
            {
                var slotObject = _effigiesSlot[i];
                if (slotObject == null)
                {
                    continue;
                }

                var slotManager = slotObject.GetComponent<SlotManager>();
                if (slotManager == null)
                {
                    continue;
                }

                var effigyName = i < roomEffigies.Length ? roomEffigies[i] : string.Empty;
                var isSelected = savedSelection.Contains(effigyName);

                slotManager.SetEffigyName(effigyName);
                slotManager.SetSelected(isSelected);

                if (isSelected)
                {
                    SelectedEffigies.Add(slotObject);
                    selectedEffigyNames.Add(effigyName);
                }
            }
        }

        public void RegisterSlotSelection(SlotManager slotManager, bool isSelected)
        {
            if (slotManager == null)
            {
                return;
            }

            if (isSelected)
            {
                if (!SelectedEffigies.Contains(slotManager.gameObject))
                {
                    SelectedEffigies.Add(slotManager.gameObject);
                }

                if (!string.IsNullOrWhiteSpace(slotManager.EffigyName) &&
                    !selectedEffigyNames.Contains(slotManager.EffigyName))
                {
                    selectedEffigyNames.Add(slotManager.EffigyName);
                }
            }
            else
            {
                SelectedEffigies.Remove(slotManager.gameObject);
                selectedEffigyNames.Remove(slotManager.EffigyName);
            }

            ResolveDungeonManager();
            dungeonManager?.SetCurrentRoomSelection(selectedEffigyNames);
        }

        public bool TryAdvanceRoom()
        {
            ResolveDungeonManager();

            if (dungeonManager == null || !dungeonManager.AdvanceToNextRoom())
            {
                return false;
            }

            PopulateCurrentRoom();
            return true;
        }

        public bool TryEscapeRoom()
        {
            ResolveDungeonManager();

            if (dungeonManager == null || !dungeonManager.EscapeCurrentRoom())
            {
                return false;
            }

            PopulateCurrentRoom();
            return true;
        }

        private void ResolveDungeonManager()
        {
            if (dungeonManager != null)
            {
                return;
            }

            dungeonManager = FindAnyObjectByType<DungeonManager>();
        }
    }
}
