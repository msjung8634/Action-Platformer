using UnityEngine;

public class FpBarUI : ResourceBarUI
{
    IFpHandler _fpHandler;

    protected override Resource TargetResource => _fpHandler?.FP;

    protected override bool TryResolveHandler()
    {
        return _resourceManager.TryGetComponent(out _fpHandler);
    }

    protected override void Subscribe()
    {
        if (_fpHandler == null)
            return;

        _fpHandler.OnFpChanged += OnResourceChanged;
        _fpHandler.OnNotEnoughFp += OnNotEnough;
    }

    protected override void Unsubscribe()
    {
        if (_fpHandler == null)
            return;

        _fpHandler.OnFpChanged -= OnResourceChanged;
        _fpHandler.OnNotEnoughFp -= OnNotEnough;
    }
}
