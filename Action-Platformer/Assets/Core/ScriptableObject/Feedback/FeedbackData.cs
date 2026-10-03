using UnityEngine;

public abstract class FeedbackData : ScriptableObject
{
    [field: Header("Sound")]
    [field: SerializeField] public AudioClip SfxClip { get; private set; }
    [field: SerializeField] public float SfxVolume { get; private set; } = 1f;


    [field: Header("Camera")]
    [field: SerializeField] public float ShakeStrength { get; private set; } = 0.1f;
    [field: SerializeField] public int ShakeCount { get; private set; } = 1;
    [field: SerializeField] public float ShakeInterval { get; private set; } = 0.05f;


    [field: Header("Time")]
    [field: SerializeField] public float StopTimeDuration { get; private set; } = 0.01f;
    [field: Space(10)]
    [field: SerializeField] public float SlowTimeDuration { get; private set; } = 0.05f;
    [field: SerializeField] public float SlowTimeScale { get; private set; } = 0.1f;
}