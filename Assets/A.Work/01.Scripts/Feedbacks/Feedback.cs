using UnityEngine;

namespace Scripts.Feedbacks
{
    public abstract class Feedback : MonoBehaviour
    {
        public abstract void CreateFeedback();
        public abstract void FinishFeedback();

        private void OnDestroy()
        {
            FinishFeedback();
        }

        private void OnDisable()
        {
            FinishFeedback();
        }
        
        
    }
}