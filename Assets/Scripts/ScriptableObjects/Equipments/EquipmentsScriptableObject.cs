using UnityEngine;
using System.Collections;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EquipmentsData", menuName = "ScriptableObjects/EquipmentsScriptableObject")]
    public class EquipmentsScriptableObject: Effigies
	{
        public int Damage;
    }
}