using System.Collections.Generic;
using TankCode.Players;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Combat
{
    public class HealingZone : NetworkBehaviour
    {
         [Header("Reference")]
        [SerializeField] private Transform _healPowerBarTrm;

        [Header("Settings")]
        [SerializeField] private int _maxHealPower = 30; //회복시킬수 있는 틱수 
        [SerializeField] private float _cooldown = 60f; //1분 쿨다운
        [SerializeField] private float _healTickRate = 1f;
        [SerializeField] private int _coinPerTick = 10; //1틱당 10코인씩 빠져나간다.
        [SerializeField] private int _healPerTick = 10; //1틱당 10씩 체력이 찬다.
        [SerializeField][ColorUsage(true, true)] private Color _normalColor, _chargeColor;
        
        private List<PlayerController> _playersInZone = new List<PlayerController>();

        private NetworkVariable<bool> _isInCharge;
        private NetworkVariable<int> _healPower = new NetworkVariable<int>();

        private float _remainingCooldown;
        private float _tickTimer;
        private Material _barMaterial;
        private readonly int _emissionColorHash = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _isInCharge = new NetworkVariable<bool>(); //차징모드에 들어갔는가?
            _barMaterial = _healPowerBarTrm.GetComponentInChildren<SpriteRenderer>().material;
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                _healPower.OnValueChanged += HandleHealPowerChanged;
                //최초 한번은 실행해줘야 해.
                HandleHealPowerChanged(0, _healPower.Value);
                _isInCharge.OnValueChanged += HandleChargeModeChanged;
            }

            if (IsServer)
            {
                _healPower.Value = _maxHealPower; //게임 시작시 셋팅
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                _healPower.OnValueChanged -= HandleHealPowerChanged;
                _isInCharge.OnValueChanged -= HandleChargeModeChanged;
             }
        }

        private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
        {
            _healPowerBarTrm.localScale = new Vector3((float)newHealPower / _maxHealPower, 1, 1);
        }

        private void HandleChargeModeChanged(bool oldValue, bool newValue)
        {
            Color color = newValue ? _chargeColor : _normalColor;
            _barMaterial.SetColor(_emissionColorHash, color);
        }


        #region Only server section

        private void Update()
        {
            if (!IsServer) return;

            if (_remainingCooldown > 0)
            {
                _remainingCooldown -= Time.deltaTime;

                float percent = 1f - _remainingCooldown / _cooldown;
                int value = Mathf.FloorToInt(_maxHealPower * percent);

                _healPower.Value = value;

                if (_remainingCooldown < 0)
                {
                    _healPower.Value = _maxHealPower;
                    _isInCharge.Value = false;
                }
                else
                {
                    return;
                }
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _healTickRate)
            {
                foreach (PlayerController player in _playersInZone)
                {
                    if (_healPower.Value <= 0) break;
                    //풀피이거나
                    if (player.HealthCompo.currentHealth.Value == player.HealthCompo.maxHealth) continue;
                //돈이 있거나.
                    if (player.CoinCompo.totalCoins.Value < _coinPerTick) { continue; }
                    player.CoinCompo.SpendCoin(_coinPerTick);
                    player.HealthCompo.RestoreHealth(_healPerTick);

                    _healPower.Value -= 1;
                    if (_healPower.Value <= 0)
                    {
                        _isInCharge.Value = true;
                        _remainingCooldown = _cooldown; //다 쓴 패드는 1분뒤에 복귀된다.
                    }
                } //end of foreach

                _tickTimer = _tickTimer % _healTickRate;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;
    
            if (other.attachedRigidbody.TryGetComponent<PlayerController>(out PlayerController player))
            {
                _playersInZone.Add(player);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsServer) return;

            if (other.attachedRigidbody.TryGetComponent<PlayerController>(out PlayerController player))
            {
                _playersInZone.Remove(player);
            }
        }
        #endregion


        
        
    }
}