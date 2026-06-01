using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _effigiesSlot;
        [SerializeField] public List<GameObject> SelectedEffigies;
    }
}