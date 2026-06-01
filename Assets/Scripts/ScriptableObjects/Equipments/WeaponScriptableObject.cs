using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponScriptableObject")]
    public class WeaponScriptableObject : EncounterScriptableObject
    {
        [Tooltip("Damage reduction when using this weapon. DamageTaken = max(0, MonsterStrength - WeaponPower).")]
        public int Power;
    }
}
