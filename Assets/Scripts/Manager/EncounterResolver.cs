using Assets.Scripts.SaveSystem;
using Assets.Scripts.ScriptableObjects;
using System;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class EncounterResolver : MonoBehaviour
    {
        [SerializeField] private RunManager runManager;

        private RunManager RunManager
        {
            get
            {
                if (runManager == null)
                    runManager = FindAnyObjectByType<RunManager>();
                return runManager;
            }
        }

       
        public EncounterResult ResolveMonsterWithHealth(EnemiesScriptableObject monster)
        {
            if (monster == null) throw new ArgumentNullException(nameof(monster));

            return new EncounterResult
            {
                HealthDelta = -monster.Strength
            };
        }

        public EncounterResult TryResolveMonsterWithWeapon(
            EnemiesScriptableObject monster,
            EquipmentsScriptableObject weapon)
        {
            if (monster == null) throw new ArgumentNullException(nameof(monster));
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));

            if (!CanUseEquippedWeaponAgainst(monster.Strength))
            {
                Debug.LogWarning($"[EncounterResolver] Weapon '{weapon.EncounterId}' cannot be used against '{monster.EncounterId}' (Strength={monster.Strength}). Weapon rule violated.");
                return null;
            }

            int damage = Mathf.Max(0, monster.Strength - weapon.Power);

            return new EncounterResult
            {
                HealthDelta = -damage,
                LastKilledByWeaponStrength = monster.Strength
            };
        }

        public EncounterResult ResolveWeapon(EquipmentsScriptableObject weapon)
        {
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));

            return new EncounterResult
            {
                EquippedWeaponId = weapon.EncounterId,
                ResetWeaponChain = true 
            };
        }

        public EncounterResult ResolveUsePotion(ConsumablesScriptableObject potion, RoomSession session)
        {
            if (potion == null) throw new ArgumentNullException(nameof(potion));
            if (session == null) throw new ArgumentNullException(nameof(session));

            int healDelta = session.PotionUsedInRoom ? 0 : potion.HealValue;
            session.PotionUsedInRoom = true;

            return new EncounterResult
            {
                HealthDelta = healDelta
            };
        }

        public EncounterResult ResolveStorePotion(ConsumablesScriptableObject potion)
        {
            if (potion == null) throw new ArgumentNullException(nameof(potion));

            return new EncounterResult
            {
                StoredPotionId = potion.EncounterId
            };
        }

        public EncounterResult TryUseStoredPotion(DungeonManager dungeonManager, RoomSession session)
        {
            var player = RunManager?.Player;
            if (player == null || string.IsNullOrEmpty(player.StoredPotionId) || session.PotionUsedInRoom)
                return null;

            var potion = dungeonManager?.GetEncounter(player.StoredPotionId) as ConsumablesScriptableObject;
            if (potion == null) return null;

            session.PotionUsedInRoom = true;
            return new EncounterResult
            {
                HealthDelta = potion.HealValue,
                StoredPotionId = string.Empty
            };
        }

        public bool ValidateEscape(PlayerState player, RoomSession session)
        {
            if (player == null || session == null) return false;
            return player.CanEscape && session.ResolvedCount == 0;
        }

        public bool CanUseEquippedWeaponAgainst(int monsterStrength)
        {
            var player = RunManager?.Player;
            if (player == null || string.IsNullOrEmpty(player.EquippedWeaponId))
                return false;

            if (!player.LastKilledByWeaponStrength.HasValue)
                return true;

            return monsterStrength < player.LastKilledByWeaponStrength.Value;
        }

        public void ResolveWeaponDefeat(int enemyStrength)
        {
            RunManager?.RecordWeaponDefeat(enemyStrength);
        }

        public bool IsPlayerDead()
        {
            var player = RunManager?.Player;
            return player?.Health <= 0;
        }

        private EncounterScriptableObject GetEncounterSO(DungeonManager dm, string id)
            => dm?.GetEncounter(id);

        public EnemiesScriptableObject GetMonster(DungeonManager dm, string id)
            => GetEncounterSO(dm, id) as EnemiesScriptableObject;

        public EquipmentsScriptableObject GetWeapon(DungeonManager dm, string id)
            => GetEncounterSO(dm, id) as EquipmentsScriptableObject;

        public ConsumablesScriptableObject GetPotion(DungeonManager dm, string id)
            => GetEncounterSO(dm, id) as ConsumablesScriptableObject;
    }
}
