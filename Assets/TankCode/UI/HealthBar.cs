using UnityEngine;

namespace TankCode.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Transform barTrm;

        public void HandleHealthChange(int currentHealth, int maxHealth)
        {
            float ratio = currentHealth / (float)maxHealth;
            barTrm.localScale = new Vector3(ratio, 1, 1);
        }
        
        
    }
}