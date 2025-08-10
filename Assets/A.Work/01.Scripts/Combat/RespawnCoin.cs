using System;
using UnityEngine;
using UnityEngine.Events;

namespace TankCode.Combat
{
    public class RespawnCoin : Coin
    {
        public UnityEvent<RespawnCoin> OnCollected;
        
        private Vector2 _prevPosition;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _prevPosition = transform.position;  //내 처음 위치를 기록
        }

        public override int Collect()
        { 
            if (isCollected) return 0;  //이미 수집된 코인이면 0리턴

            if (!IsServer)  //클라는 그냥 먹고 사라지게만 처리, 실질적인 코인을 올리는 작업은 서버가
            {
                SetVisible(false);
                return 0;
            }
            
            //여기부턴 서버가 처리한다
            isCollected = true;
            OnCollected?.Invoke(this);
            isActive.Value = false;  //액티브 끄기

            return coinValue;  //실질적인 코인 값 리턴
        }

        //리셋은 서버만
        public void ResetCoin()
        {
            isCollected = false;
            isActive.Value = true;
            SetVisible(true);
        }

        private void Update()
        {
            if (IsServer) return;  //서버는 업데이트 안함

            if (Vector2.Distance(_prevPosition, transform.position) >= 0.5f)  //내가 서버로부터 움직였다면
            {
                _prevPosition = transform.position;  //현재 위치로 prevPosition을 재설정
                SetVisible(true);  //다시 보이게 설정
            }
        }
        
        
    }
}