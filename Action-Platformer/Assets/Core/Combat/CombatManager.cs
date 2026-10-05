using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    public void ProcessHit(HitData hitData)
    {
        if (hitData.Target == null)
            return;

        hitData.Target.TakeDamage(hitData);
    }
}
