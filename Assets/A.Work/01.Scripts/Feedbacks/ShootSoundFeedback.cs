using System;
using Ami.BroAudio;
using UnityEngine;

namespace TankCode.Feedbacks
{
    public class ShootSoundFeedback : Feedback
    {
        [SerializeField] private SoundID shootSound;
        [SerializeField] private float turnOnTime = 0.08f;
        
        public override async void CreateFeedback()
        {
            BroAudio.Play(shootSound);
            await Awaitable.WaitForSecondsAsync(turnOnTime);
            BroAudio.Stop(shootSound);
        }

        public override void FinishFeedback()
        {
            BroAudio.Stop(shootSound);
        }
        
    }
}