using UnityEngine;

public class HpBarUI : ResourceBarUI
{
    IHpHandler _hpHandler;

    protected override Resource TargetResource => _hpHandler?.HP;

    protected override bool TryResolveHandler()
    {
        return _resourceManager.TryGetComponent(out _hpHandler);
    }

    protected override void Subscribe()
    {
        if (_hpHandler == null)
            return;

        _hpHandler.OnHpChanged += OnResourceChanged;
        _hpHandler.OnDead += OnNotEnough;
    }

    protected override void Unsubscribe()
    {
        if (_hpHandler == null)
            return;

        _hpHandler.OnHpChanged -= OnResourceChanged;
        _hpHandler.OnDead -= OnNotEnough;
    }
}
