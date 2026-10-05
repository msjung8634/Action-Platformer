using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public abstract class ResourceBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject _resourceManager;
    [SerializeField] Slider _front;
    [SerializeField] Slider _back;

    [Header("Decrease")]
    [SerializeField, Min(0f)] float _decreaseDelay = 0.3f;
    [SerializeField, Min(0.01f)] float _decreaseDuration = 0.8f;
    [SerializeField] AnimationCurve _decreaseCurve = new AnimationCurve();
    float _targetValue;
    float _delayEndTime;
    bool _isAnimating;

    [Header("NotEnough Shake")]
    [SerializeField, Min(0.01f)] float _shakeDuration = 0.2f;
    // 기본 흔들림 범위: X는 좌우, Y는 상하
    [SerializeField] Vector2 _shakeStrength = new(4f, 2f);
    [SerializeField, Min(1)] int _shakeVibrato = 8;

    // 흔들리는 도중 추가 요청이 들어올 때마다 증가하는 배율
    [SerializeField, Min(0f)] float _shakeMultiplierStep = 0.3f;
    [SerializeField, Min(1f)] float _maxShakeMultiplier = 2.5f;

    Tween _shakeTween;
    Vector2 _shakeStartPosition;
    float _shakeMultiplier = 1f;

    protected abstract Resource TargetResource { get; }
    protected abstract bool TryResolveHandler();
    protected abstract void Subscribe();
    protected abstract void Unsubscribe();

    protected virtual void OnEnable()
    {
        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame += RefreshImmediate;
    }

    protected virtual void Start()
    {
        Initialize(_front);
        Initialize(_back);
        TryResolveHandler();

        Subscribe();
        RefreshImmediate();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
        _isAnimating = false;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame -= RefreshImmediate;
    }

    void Initialize(Slider slider)
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
            _delayEndTime = Time.unscaledTime + _decreaseDelay;
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

    protected void OnNotEnough()
    {
        if (transform is RectTransform shakeRect)
        {
            bool isShaking = _shakeTween != null && _shakeTween.IsActive();

            if (isShaking)
            {
                _shakeMultiplier = Mathf.Min(_shakeMultiplier + _shakeMultiplierStep, _maxShakeMultiplier);
                _shakeTween.Kill();
                shakeRect.anchoredPosition = _shakeStartPosition;
            }
            else
            {
                _shakeStartPosition = shakeRect.anchoredPosition;
                _shakeMultiplier = 1f;
            }

            Vector3 strength = new Vector3(Mathf.Max(0f, _shakeStrength.x), Mathf.Max(0f, _shakeStrength.y), 0f) * _shakeMultiplier;
            _shakeTween = shakeRect.DOShakeAnchorPos(
                duration: _shakeDuration,
                strength: strength,
                vibrato: _shakeVibrato,
                randomness: 0f,
                snapping: false,
                fadeOut: true
            ).SetUpdate(true)
            .OnComplete(() =>
            {
                shakeRect.anchoredPosition = _shakeStartPosition;
                _shakeTween = null;
                _shakeMultiplier = 1f;
            });
        }
    }
}
