using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EnemiesData", menuName = "ScriptableObjects/EnemiesScriptableObject")]
    public class EnemiesScriptableObject : Effigies
    {
        public int Power;
    }
}