using UnityEngine;

public class FeedbackManager : Singleton<FeedbackManager>
{
    public void Apply(AttackData data)
    {
        SoundManager.Instance.ApplyFeedback(data);
        CameraManager.Instance.ApplyFeedback(data);
        TimeManager.Instance.ApplyFeedback(data);
    }
}
