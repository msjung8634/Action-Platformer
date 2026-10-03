using UnityEngine;

public interface ISpHandler
{
    void IncreaseCurrentSP(int amount);
    void DecreaseCurrentSP(int amount);
    void IncreaseMaxSP(int amount);
    void DecreaseMaxSP(int amount);
}
