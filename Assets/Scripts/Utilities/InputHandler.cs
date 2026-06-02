using Assets.Scripts.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (rayHit.collider == null) return;

        
        if (rayHit.collider.gameObject.TryGetComponent(out IInteractable interactable)) 
            interactable.Interact();
    }
}
