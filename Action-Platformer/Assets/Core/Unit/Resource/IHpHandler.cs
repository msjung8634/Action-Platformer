using UnityEngine;

public interface IHpHandler
{
    void IncreaseCurrentHP(int amount);
    void DecreaseCurrentHP(int amount);
    void IncreaseMaxHP(int amount);
    void DecreaseMaxHP(int amount);
}
