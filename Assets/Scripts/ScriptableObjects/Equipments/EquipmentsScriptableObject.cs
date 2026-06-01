using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EquipmentsData", menuName = "ScriptableObjects/EquipmentsScriptableObject")]
    public class EquipmentsScriptableObject : EncounterScriptableObject
    {
        [Tooltip("Damage reduction when used against a monster. Replaces old 'Damage' field. TDD §4.2")]
        public int Power;
    }
}
