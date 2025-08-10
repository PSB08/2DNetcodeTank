using System;
using System.Collections;
using System.Collections.Generic;
using TankCode.Core;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TankCode.Combat
{
    public class CoinSpawner : NetworkBehaviour
    {
        [SerializeField] private RespawnCoin coinPrefab;

        [Header("Setting Values")] 
        [SerializeField] private int maxCoinCount = 30;
        [SerializeField] private int coinValue = 10;
        [SerializeField] private LayerMask whatIsObstacle;
        [SerializeField] private float spawnTerm = 30f;
        [SerializeField] private float spawnRadius = 8f;

        private bool _isSpawning = false;
        private float _spawnTime = 0;
        private int _spawnCountTime = 10;

        public List<Transform> spawnPoints;
        private float coinRadius;

        private Stack<RespawnCoin> _coinPool = new Stack<RespawnCoin>();
        private List<RespawnCoin> _activeCoinList = new List<RespawnCoin>();

        //이 메서드는 서버만 실행
        private RespawnCoin SpawnCoin()
        {
            RespawnCoin coinInstance = Instantiate(coinPrefab);
            coinInstance.SetCoinValue(coinValue);
            coinInstance.GetComponent<NetworkObject>().Spawn();
            //서버가 클라에게 스폰 되었음을 알림 (네트워크 오브젝트는 이렇게 스폰 해줘야 함)
            
            coinInstance.OnCollected.AddListener(HandleCoinCollected);
            return coinInstance;
        }

        private void HandleCoinCollected(RespawnCoin targetCoin)
        {
            _activeCoinList.Add(targetCoin);
            targetCoin.SetVisible(false);
            _coinPool.Push(targetCoin);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer == false) return;
            
            coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;  //코인의 반지름 알아내기

            for (int i = 0; i < maxCoinCount; i++)
            {
                RespawnCoin coin = SpawnCoin();
                coin.SetVisible(false);  //처음 생성한 것들은 꺼주기
                _coinPool.Push(coin);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
                StopAllCoroutines();
        }

        private void Update()
        {
            if (IsServer == false) return;

            if (GameManager.Instance.GameStarted == false) return;
            
            //현재 생성에 들어가지 않았고, 생성된 코인이 하나도 없다면 생성을 시작
            if (_isSpawning == false && _activeCoinList.Count == 0)
            {
                _spawnTime += Time.deltaTime;
                if (_spawnTime >= spawnTerm)
                {
                    _spawnTime = 0;
                    StartCoroutine(SpawnCoroutine());
                }
            }
        }

        private IEnumerator SpawnCoroutine()
        {
            _isSpawning = true;
            int pointIdx = Random.Range(0, spawnPoints.Count);
            int coinCount = Random.Range(maxCoinCount / 2, maxCoinCount + 1);

            for (int i = _spawnCountTime; i > 0; i--)
            {
                CountDownClientRpc(i, pointIdx, coinCount);
                yield return new WaitForSeconds(1);  //서버가 1초마다 한 번씩 클라에게 RPC를 날린다
            }

            Vector2 center = spawnPoints[pointIdx].position;
            for (int i = 0; i < coinCount; i++)
            {
                Vector2 pos = Random.insideUnitCircle * spawnRadius + center;
                RespawnCoin coin = _coinPool.Pop();
                coin.transform.position = pos;
                coin.ResetCoin();
                
                _activeCoinList.Add(coin);
                yield return new WaitForSeconds(4f);  //4초마다 1개씩 생성
            }
            _isSpawning = false;  //작업 완료 후 플래그 끄기
        }

        [ClientRpc]
        private void CountDownClientRpc(int sec, int pointIdx, int coinCount)
        {
            Debug.Log($"{pointIdx} 지점에서 {sec}초 후에 {coinCount}개의 코인이 생성됩니다.");
        }
        
        
    }
}