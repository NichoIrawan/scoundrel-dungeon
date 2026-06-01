using UnityEngine;
using System.Collections;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "ConsumablesData", menuName = "ScriptableObjects/ConsumablesScriptableObject")]
    public class ConsumablesScriptableObject: Effigies
	{
        public int HealAmount;
    }
}