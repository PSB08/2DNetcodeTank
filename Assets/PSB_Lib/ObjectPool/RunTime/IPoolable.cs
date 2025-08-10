using UnityEngine;

namespace PSB_Lib.ObjectPool.RunTime
{
    public interface IPoolable
    {
        public PoolItemSO PoolItem { get; }
        public GameObject gameObject { get; }
        public void SetUpPool(Pool pool);
        public void ResetItem();

    }
}