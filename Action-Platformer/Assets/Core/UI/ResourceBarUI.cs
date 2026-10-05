using UnityEngine;
using UnityEngine.UI;

public abstract class ResourceBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject _resourceManager;
    [SerializeField] Slider _front;
    [SerializeField] Slider _back;

    [Header("Feedback")]
    [SerializeField, Min(0f)] float _delaySeconds = 0.3f;
    [SerializeField, Min(0.01f)] float _decreaseDuration = 0.8f;
    [SerializeField] AnimationCurve _decreaseCurve = new AnimationCurve();
    float _targetValue;
    float _delayEndTime;
    bool _isAnimating;
    bool _started;
    float _backStartValue;

    protected abstract Resource TargetResource { get; }
    protected abstract bool TryResolveHandler();
    protected abstract void Subscribe();
    protected abstract void Unsubscribe();

    protected virtual void Awake()
    {
        InitializeSlider(_front);
        InitializeSlider(_back);
        TryResolveHandler();
    }

    protected virtual void OnEnable()
    {
        Subscribe();
        RefreshImmediate();
        WaveManager.Instance.OnRestartGame += RefreshImmediate;
    }

    protected virtual void Start()
    {
        RefreshImmediate();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
        _isAnimating = false;

        WaveManager.Instance.OnRestartGame -= RefreshImmediate;
    }

    void InitializeSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    void RefreshImmediate()
    {
        Resource resource = TargetResource;

        if (resource == null)
            return;

        _targetValue = Mathf.Clamp01((float)resource.Current / resource.Max);
        _front.SetValueWithoutNotify(_targetValue);
        _back.SetValueWithoutNotify(_targetValue);

        _isAnimating = false;
    }

    protected virtual void OnResourceChanged(int current, int max)
    {
        float nextValue = Mathf.Clamp01((float)current / max);

        // decrease
        if (nextValue < _targetValue)
        {
            // back은 delay후 반영
            _backStartValue = _back.value;
            _delayEndTime = Time.unscaledTime + _delaySeconds;
            _isAnimating = true;
        }
        // increase
        else if (nextValue > _targetValue)
        {
            // back도 즉시 반영
            _back.SetValueWithoutNotify(nextValue);
            _isAnimating = false;
        }

        // front는 즉시 반영
        _targetValue = nextValue;
        _front.SetValueWithoutNotify(nextValue);
    }

    protected virtual void Update()
    {
        if (!_isAnimating || Time.unscaledTime < _delayEndTime)
            return;

        float elapsed = Time.unscaledTime - _delayEndTime;
        float progress = Mathf.Clamp01(elapsed / _decreaseDuration);
        float curveValue = _decreaseCurve.Evaluate(progress);
        float value = Mathf.MoveTowards(
            _back.value,
            _targetValue,
            curveValue);

        _back.SetValueWithoutNotify(value);

        if (progress >= 1f)
        {
            _back.SetValueWithoutNotify(_targetValue);
            _isAnimating = false;
        }
    }
}
