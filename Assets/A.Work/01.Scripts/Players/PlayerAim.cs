using System;
using Unity.Netcode;
using UnityEngine;

namespace TankCode.Players
{
    public class PlayerAim : NetworkBehaviour
    {
        [SerializeField] private PlayerInputSo playerInput;
        [SerializeField] private Transform turretTrm;

        private void LateUpdate()
        {
            if (IsOwner == false) return;
            
            Vector3 direction = (playerInput.GetWorldMousePosition() - transform.position).normalized;
            turretTrm.up = direction;
            //마우스 월드 좌표 구해와서 터렛 transform을 회전시킨다
        }
        
        
    }
}