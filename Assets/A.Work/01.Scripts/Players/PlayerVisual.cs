using UnityEngine;

namespace Scripts.Players
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] sprites;

        public void SetTankColor(Color color)
        {
            foreach (SpriteRenderer sp in sprites)
            {
                sp.color = color;
            }
        }
        
    }
}