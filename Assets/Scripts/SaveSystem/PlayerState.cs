using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class PlayerState
    {
        public int Health = 20;
        public string EquippedWeaponId;
        public string StoredPotionId;
        public int LastDefeatedEnemyStrength;
        public bool PotionUsedThisRoom;
    }
}
