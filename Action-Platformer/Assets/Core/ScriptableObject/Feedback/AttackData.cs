using UnityEngine;

[CreateAssetMenu(fileName = "AttackData_", menuName = "Scriptable Objects/AttackData")]
public class AttackData : FeedbackData
{
    // Resource
    [field: Header("Resource")]
    [field: SerializeField] public int Damage { get; private set; } = 100;
    [field: SerializeField] public int SpConsumption { get; private set; } = 100;
}
