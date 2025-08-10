using UnityEngine;

namespace TankCode.Feedbacks
{
    public class MuzzleFeedback : Feedback
    {
        [SerializeField] private GameObject muzzleSprite;
        [SerializeField] private float turnOnTime = 0.08f;
        
        public override async void CreateFeedback()
        {
            muzzleSprite.SetActive(true);
            await Awaitable.WaitForSecondsAsync(turnOnTime);
            muzzleSprite.SetActive(false);
        }

        public override void FinishFeedback()
        {
            muzzleSprite.SetActive(false);
        }
        
        
    }
}