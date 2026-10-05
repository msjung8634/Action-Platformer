using UnityEngine;

public class SpBarUI : ResourceBarUI
{
    ISpHandler _spHandler;

    protected override Resource TargetResource => _spHandler?.SP;

    protected override bool TryResolveHandler()
    {
        return _resourceManager.TryGetComponent(out _spHandler);
    }

    protected override void Subscribe()
    {
        if (_spHandler == null)
            return;

        _spHandler.OnSpChanged += OnResourceChanged;
    }

    protected override void Unsubscribe()
    {
        if (_spHandler == null)
            return;

        _spHandler.OnSpChanged -= OnResourceChanged;
    }
}
