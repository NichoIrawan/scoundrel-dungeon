using Assets.Scripts;
using Assets.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotManager : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _selectedEffigiesLight;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private bool _isSelected;

    public void Interact()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            _selectedEffigiesLight.SetActive(true);
            roomManager.SelectedEffigies.Add(gameObject);

        }
        else
        {
            _selectedEffigiesLight.SetActive(false);
            roomManager.SelectedEffigies.Remove(gameObject);
        }
    }
}
