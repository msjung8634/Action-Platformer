using UnityEngine;

[CreateAssetMenu(fileName = "DodgeData_", menuName = "Scriptable Objects/DodgeData")]
public class DodgeData : FeedbackData
{
    // Resource
    [field: Header("Resource")]
    [field: SerializeField] public int SpConsumption { get; private set; } = 100;
}
