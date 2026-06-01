using Assets.Scripts.SceneController;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class HallwayManager : MonoBehaviour
    {
        [SerializeField] private DungeonManager dungeonManager;

        public bool TryEnterNextRoom()
        {
            ResolveDungeonManager();

            return global::SceneController.Instance != null &&
                global::SceneController.Instance
                    .NewTransition()
                    .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Room, setActive: true)
                    .Perform() != null;
        }

        public int GetCurrentNode()
        {
            ResolveDungeonManager();
            return dungeonManager != null ? dungeonManager.CurrentNode : -1;
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
