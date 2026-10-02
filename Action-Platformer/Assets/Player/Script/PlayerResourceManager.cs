using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    [SerializeField] PlayerStatSO _statData;

    public Resource HP { get; private set; }
    public Resource SP { get; private set; }

    void Awake()
    {
        HP = new Resource(_statData.MaxHP);
        SP = new Resource(_statData.MaxSP);
    }

    // TODO : HP 0되면 사망

    // TODO : SP 0되면 탈진 (2초간 SP재생 중단)

}
