using Assets.Scripts;
using Assets.Scripts.Manager;
using UnityEngine;

public class SlotManager : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _selectedEffigiesLight;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private bool _isSelected;
    [SerializeField] private string effigyName;

    public string EffigyName => effigyName;

    public void Interact()
    {
        SetSelected(!_isSelected);
        roomManager?.RegisterSlotSelection(this, _isSelected);
    }

    public void SetEffigyName(string value)
    {
        effigyName = value;
        gameObject.name = string.IsNullOrWhiteSpace(effigyName) ? "Empty Slot" : effigyName;
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        if (_selectedEffigiesLight != null)
        {
            _selectedEffigiesLight.SetActive(_isSelected);
        }
    }
}
