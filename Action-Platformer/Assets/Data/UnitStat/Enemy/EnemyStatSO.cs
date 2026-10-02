using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatSO_", menuName = "Unit Stat Data/Enemy")]
public class EnemyStatSO : ScriptableObject
{
    // 1_00 당 1로 처리
    [field: SerializeField] public int MaxHP { get; private set; } = 3_00;  // 체력 3칸
}
