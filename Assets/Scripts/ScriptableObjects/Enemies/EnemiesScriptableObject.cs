using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EnemiesData", menuName = "ScriptableObjects/EnemiesScriptableObject")]
    public class EnemiesScriptableObject : EncounterScriptableObject
    {
        [Tooltip("Damage dealt to the player. Replaces old 'Power' field. TDD §4.2")]
        public int Strength;
    }
}
