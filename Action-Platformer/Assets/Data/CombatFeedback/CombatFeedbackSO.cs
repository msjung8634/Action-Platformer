using UnityEngine;

[CreateAssetMenu(fileName = "CombatFeedbackSO_", menuName = "Scriptable Objects/CombatFeedbackSO")]
public class CombatFeedbackSO : ScriptableObject
{
    [field: Header("Stop Time")]
    [field: SerializeField] public float StopTimeDuration { get; private set; } = 0.01f;

    [field: Header("Slow Time")]
    [field: SerializeField] public float SlowTimeDuration { get; private set; } = 0.05f;
    [field: SerializeField] public float SlowTimeScale { get; private set; } = 0.1f;

    [field: Header("Camera Shake")]
    [field: SerializeField] public float ShakeStrength { get; private set; } = 0.1f;
    [field: SerializeField] public int ShakeCount { get; private set; } = 1;
    [field: SerializeField] public float ShakeInterval { get; private set; } = 0.05f;

    //[field: Header("VFX")]
    //[field: SerializeField]
    //public GameObject HitVFX { get; private set; }

    //[field: Header("SFX")]
    //[field: SerializeField]
    //public AudioClip HitSFX { get; private set; }
}
