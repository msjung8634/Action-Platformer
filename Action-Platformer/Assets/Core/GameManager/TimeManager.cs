using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    CancellationTokenSource _hitStopCts;

    public void ApplyFeedback(FeedbackData feedbackData)
    {
        _hitStopCts?.Cancel();
        _hitStopCts?.Dispose();
        _hitStopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        TimeFeedbackAsync(feedbackData, _hitStopCts.Token).Forget();
    }
    private async UniTaskVoid TimeFeedbackAsync(FeedbackData feedbackData, CancellationToken token)
    {
        try
        {
            // Stop
            Time.timeScale = 0f;
            await UniTask.Delay(TimeSpan.FromSeconds(feedbackData.StopTimeDuration), DelayType.Realtime, cancellationToken: token);

            // Slow
            Time.timeScale = feedbackData.SlowTimeScale;
            await UniTask.Delay(TimeSpan.FromSeconds(feedbackData.SlowTimeDuration), DelayType.Realtime, cancellationToken: token);

            // 정상 종료시 원상복구
            Time.timeScale = 1f;
        }
        catch (OperationCanceledException)
        {
            // Cancel(새로운 Feedback으로 교체)된 경우 끝까지 처리하고 원상복구
        }
    }

    void OnDestroy()
    {
        _hitStopCts?.Cancel();
        _hitStopCts?.Dispose();

        Time.timeScale = 1f;
    }
}
