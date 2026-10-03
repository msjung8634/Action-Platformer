using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStat", menuName = "Scriptable Objects/Player Stat")]
public class PlayerStat : ScriptableObject
{
    // 1_00 당 1로 처리
    [field: SerializeField] public int MaxHP { get; private set; } = 3_00;  // 체력 3칸
    [field: SerializeField] public int MaxSP { get; private set; } = 5_00;  // 스태미너 5칸
}