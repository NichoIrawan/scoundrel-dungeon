using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class EncounterResolver : MonoBehaviour
    {
        [SerializeField] private RunManager runManager;

        public bool CanUseEquippedWeaponAgainst(int enemyStrength)
        {
            ResolveRunManager();

            var player = runManager?.Player;
            if (player == null || string.IsNullOrEmpty(player.EquippedWeaponId))
            {
                return false;
            }

            return player.LastDefeatedEnemyStrength <= 0 || enemyStrength < player.LastDefeatedEnemyStrength;
        }

        public void ResolveWeaponDefeat(int enemyStrength)
        {
            ResolveRunManager();
            runManager?.RecordWeaponDefeat(enemyStrength);
        }

        private void ResolveRunManager()
        {
            if (runManager != null)
            {
                return;
            }

            runManager = FindAnyObjectByType<RunManager>();
        }
    }
}
