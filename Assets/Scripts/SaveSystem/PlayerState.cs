using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class PlayerState
    {
        public int Health = 35;
        public string EquippedWeaponId;
        public string StoredPotionId;
        public int? LastKilledByWeaponStrength;
        public bool CanEscape = true;
    }
}
