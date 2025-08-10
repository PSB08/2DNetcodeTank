using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TankCode.Players
{
    [UnityEngine.CreateAssetMenu(fileName = "PlayerInput", menuName = "SO/PlayerInput", order = 0)]
    public class PlayerInputSo : ScriptableObject, Controls.IPlayerActions
    {
        public event Action<bool> OnFire;

        public Vector2 MovementKey { get; private set; }
        public Vector2 AimPosition { get; private set; }

        private Controls _controls;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementKey = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnFire?.Invoke(true);
            else if (context.canceled)
            {
                OnFire?.Invoke(false);
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            AimPosition = context.ReadValue<Vector2>();
        }

        public Vector3 GetWorldMousePosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(AimPosition);
            worldPosition.z = 0;
            return worldPosition;
        }
        
        
    }    
}
