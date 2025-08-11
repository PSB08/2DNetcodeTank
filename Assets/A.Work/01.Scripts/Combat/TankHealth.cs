using System;
using Scripts.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Combat
{
    public class TankHealth : NetworkBehaviour
    {
        //초기화 반드시 Awake나 위에 있어야 함 얘는 중간에 생성을 허락하지 않음
        public NetworkVariable<int> currentHealth = new  NetworkVariable<int>(); 
        public int maxHealth = 100;
        
        public event Action OnDieEvent;
        public event Action OnHealthChangedEvent;
        public UnityEvent<int, int> OnHealthChange;
        
        [field: SerializeField] public bool IsDead { get; private set; }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                currentHealth.OnValueChanged += HandleHealthChange;
                currentHealth.OnValueChanged += HandleChangeHealthValue;
                HandleHealthChange(0, maxHealth);  //최초에는 한 번만 실행
            }

            if (!IsServer) return;  //아래의 코드는 서버만 실행한다
            currentHealth.Value = maxHealth;  //네트워크 변수는 서버만 그 값을 변경하는 것이 가능
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                currentHealth.OnValueChanged -= HandleHealthChange;
                currentHealth.OnValueChanged -= HandleChangeHealthValue;
            }
        }
        
        private void HandleHealthChange(int previousValue, int newValue)
            =>  OnHealthChange?.Invoke(newValue, maxHealth);

        public void TakeDamage(int amount) => ModifyHealth(-amount);
        public void RestoreHealth(int amount) => ModifyHealth(amount);
        
        //이건 오직 서버만 실행 가능해야 함
        private void ModifyHealth(int value)
        {
            if (IsDead) return;
            
            currentHealth.Value = Mathf.Clamp(currentHealth.Value + value, 0, maxHealth);
            if (currentHealth.Value == 0)
            {
                OnDieEvent?.Invoke();
                IsDead = true;
            }
        }
        
        public void TakeDamage(int amount, ulong attackerId)
        {
            if (!IsServer || IsDead) return;

            currentHealth.Value = Mathf.Clamp(currentHealth.Value - amount, 0, maxHealth);
    
            if (currentHealth.Value == 0)
            {
                IsDead = true;
                OnDieEvent?.Invoke();
                
                KillFeedManager.Instance.LogKill(attackerId, OwnerClientId);
            }
        }
        
        private void HandleChangeHealthValue(int previousValue, int newValue)
        {
            OnHealthChangedEvent?.Invoke();

            int delta = newValue - previousValue;
            int value = Mathf.Abs(delta);
            if (value == maxHealth) return;
            if (delta < 0)
            {
                TextManager.Instance.PopupText(value.ToString(), transform.position, Color.red);
            }
            else 
            {
                TextManager.Instance.PopupText(value.ToString(), transform.position, Color.green);
            }
        }
        
    }
}