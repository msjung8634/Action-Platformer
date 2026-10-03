using UnityEngine;

public struct HitData
{
    public GameObject Attacker;
    public IDamageable Target;
    public Vector2 HitPoint;
    public Vector2 HitDirection;
    public AttackData AttackData;

    public HitData(GameObject attacker, IDamageable target, Vector2 hitPoint, Vector2 hitDirection, AttackData attackData)
    {
        Attacker = attacker;
        Target = target;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
        AttackData = attackData;
    }

    public override string ToString()
    {
        var targetMono = Target as MonoBehaviour;
        var targetName = targetMono.transform.root.name;

        return $"Hit : {Attacker.name} ━▶ {targetName} " +
           $"[dmg:{AttackData.Damage:F1} / spConsume:{AttackData.SpConsumption:F1}]";
    }
}