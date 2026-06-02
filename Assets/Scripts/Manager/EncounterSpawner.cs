using Assets.Scripts.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class EncounterSpawner : MonoBehaviour
    {
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private GameObject encounterPrefab;
        [SerializeField] private List<SlotManager> slots;

        private void Start()
        {
            ResolveReferences();
            BindEncountersToSlots();
        }

        public void BindEncountersToSlots()
        {
            ResolveReferences();

            if (roomManager == null || dungeonManager == null) return;

            var encounterIds = roomManager.CurrentEncounterIds;

            // Clean up old encounter instances from all slots
            foreach (var slot in slots)
            {
                foreach (Transform child in slot.transform)
                {
                    var encounter = child.GetComponent<Encounter>();
                    if (encounter != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            foreach (var slot in slots)
            {
                int idx = slot.SlotIndex;
                if (idx < 0 || idx >= encounterIds.Length) continue;

                var encounterId = encounterIds[idx];
                var so = dungeonManager.GetEncounter(encounterId);
                slot.SetEncounterData(so);

                if (so != null && encounterPrefab != null)
                {
                    var instance = Instantiate(encounterPrefab, slot.transform);
                    var encounter = instance.GetComponent<Encounter>();
                    if (encounter != null)
                    {
                        encounter.Config = so;
                    }
                }
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

