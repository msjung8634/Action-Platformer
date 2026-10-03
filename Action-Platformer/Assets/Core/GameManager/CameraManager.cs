using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] CinemachineImpulseSource _impulseSource;

    public void ApplyFeedback(FeedbackData feedbackData)
    {
        CameraFeedbackAsync(feedbackData).Forget();
    }
    async UniTaskVoid CameraFeedbackAsync(FeedbackData feedbackData)
    {
        var count = feedbackData.ShakeCount;
        var interval = feedbackData.ShakeInterval;

        for (int i = 0; i < count; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            Vector3 velocity = direction * feedbackData.ShakeStrength;

            _impulseSource.GenerateImpulseWithVelocity(velocity);

            if (i < count - 1)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(interval),
                    DelayType.DeltaTime,
                    cancellationToken: destroyCancellationToken
                );
            }
        }
    }
}
