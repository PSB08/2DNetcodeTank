using System.Collections.Generic;
using UnityEngine;

namespace TankCode.Feedbacks
{
    public class FeedbackPlayer : MonoBehaviour
    {
        [SerializeField] private List<Feedback> feedbackToPlay;

        public void PlayAllFeedbacks()
        {
            FinishFeedbacks();
            feedbackToPlay.ForEach(feedback => feedback.CreateFeedback());
        }

        private void FinishFeedbacks()
        {
            feedbackToPlay.ForEach(feedback => feedback.FinishFeedback());
        }
        
        
    }
}