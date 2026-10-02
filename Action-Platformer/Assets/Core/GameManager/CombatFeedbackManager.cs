using UnityEngine;

public class CombatFeedbackManager : Singleton<CombatFeedbackManager>
{
    public void ApplyFeedback(CombatFeedbackSO data)
    {
        TimeManager.Instance.ApplyCombatFeedback(data);
        CameraManager.Instance.ApplyCombatFeedbackShake(data);
        // VFX
        // SFX
    }
}
