using Unity.Netcode;
using UnityEngine;

namespace TankCode.Players
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("reference data")]
        [SerializeField] private PlayerInputSo playerInput;
        [SerializeField] private Transform bodyTrm;

        [Header("Setting values")] 
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float turingRate = 30f;
        
        private new Rigidbody2D rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (IsOwner == false) return;

            HandleRotate();
        }

        private void FixedUpdate()
        {
            if (IsOwner == false) return;

            HandleMovement();
        }

        private void HandleRotate()
        {
            float zRotation = playerInput.MovementKey.x * -turingRate * Time.deltaTime;
            bodyTrm.Rotate(0,0, zRotation);
        }
        
        private void HandleMovement()
        {
            rigidbody.linearVelocity = bodyTrm.up * (playerInput.MovementKey.y * moveSpeed);
        }
        
        
        
    }
}