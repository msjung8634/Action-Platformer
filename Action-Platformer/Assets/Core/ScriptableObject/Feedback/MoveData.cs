using UnityEngine;

[CreateAssetMenu(fileName = "MoveData_", menuName = "Scriptable Objects/MoveData")]
public class MoveData : FeedbackData
{
    // Resource
    [field: Header("Resource")]
    [field: SerializeField] public int SpConsumption { get; private set; } = 100;
}
