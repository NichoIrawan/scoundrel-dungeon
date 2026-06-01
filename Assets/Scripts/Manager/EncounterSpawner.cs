using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class EncounterSpawner : MonoBehaviour
    {
        [SerializeField] private RoomManager roomManager;

        public IReadOnlyList<string> GetPresentedEncounterIds()
        {
            ResolveRoomManager();
            return roomManager != null ? roomManager.SelectedEffigyNames : new List<string>();
        }

        private void ResolveRoomManager()
        {
            if (roomManager != null)
            {
                return;
            }

            roomManager = FindAnyObjectByType<RoomManager>();
        }
    }
}
