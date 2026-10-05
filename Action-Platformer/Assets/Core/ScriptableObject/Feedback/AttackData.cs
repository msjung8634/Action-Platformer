using UnityEngine;

[CreateAssetMenu(fileName = "AttackData_", menuName = "Scriptable Objects/AttackData")]
public class AttackData : FeedbackData
{
    [field: Header("Hit")]
    [field: SerializeField] public bool AllowMultiHit { get; private set; } = false;
    [field: SerializeField] public int Damage { get; private set; } = 100;

    [field: Header("Consume Resource")]
    [field: Space(10)]
    [field: SerializeField] public int SpConsumption { get; private set; } = 0;
    [field: SerializeField] public int FpConsumption { get; private set; } = 0;

    [field: Header("Gain Resource")]
    [field: SerializeField] public int FpGain { get; private set; } = 0;
}
