using Assets.Scripts.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class EncounterSpawner : MonoBehaviour
    {
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private DungeonManager dungeonManager;

        private void Start()
        {
            ResolveReferences();
            BindEncountersToSlots();
        }

        public void BindEncountersToSlots()
        {
            ResolveReferences();

            if (roomManager == null || dungeonManager == null) return;

            var slotManagers = roomManager.GetComponentsInChildren<SlotManager>(includeInactive: true);
            var encounterIds = roomManager.CurrentEncounterIds;

            foreach (var slot in slotManagers)
            {
                int idx = slot.SlotIndex;
                if (idx < 0 || idx >= encounterIds.Length) continue;

                var encounterId = encounterIds[idx];
                var so = dungeonManager.GetEncounter(encounterId);
                slot.SetEncounterData(so);
            }
        }

        public IReadOnlyList<string> GetPresentedEncounterIds()
        {
            ResolveReferences();
            return roomManager != null
                ? roomManager.CurrentEncounterIds
                : new string[0];
        }

        private void ResolveReferences()
        {
            if (roomManager == null) roomManager = FindAnyObjectByType<RoomManager>();
            if (dungeonManager == null) dungeonManager = FindAnyObjectByType<DungeonManager>();
        }
    }
}

