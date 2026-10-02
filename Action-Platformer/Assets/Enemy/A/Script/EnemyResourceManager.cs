using UnityEngine;

public class EnemyResourceManager : MonoBehaviour
{
    [SerializeField] EnemyStatSO _statData;

    public Resource HP { get; private set; }

    void Awake()
    {
        HP = new Resource(_statData.MaxHP);
    }

    // TODO : HP 0되면 사망

}
