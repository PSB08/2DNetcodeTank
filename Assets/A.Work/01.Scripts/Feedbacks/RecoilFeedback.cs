using DG.Tweening;
using UnityEngine;

namespace TankCode.Feedbacks
{
    public class RecoilFeedback : Feedback
    {
        [SerializeField] private Transform targetTrm;
        [SerializeField] private float recoilPower = 0.2f;
        
        public override void CreateFeedback()
        {
            float current = targetTrm.localPosition.x;
            
            targetTrm.DOLocalMoveY(current - recoilPower, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        public override void FinishFeedback()
        {
            targetTrm.DOComplete();
        }
        
    }
}