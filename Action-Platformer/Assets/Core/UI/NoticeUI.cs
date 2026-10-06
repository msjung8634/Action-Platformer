using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoticeUI : Singleton<NoticeUI>
{
    [Header("References")]
    [SerializeField] RectTransform _noticePrefab;

    [Header("Position")]
    [SerializeField] Vector2 _displayPosition = Vector2.zero;
    [SerializeField, Min(0f)] float _enterDistance = 40f;
    [SerializeField, Min(0f)] float _exitDistance = 60f;

    [Header("Duration")]
    [SerializeField, Min(0.01f)] float _enterDuration = 0.2f;
    [SerializeField, Min(0f)] float _holdDuration = 0.6f;
    [SerializeField, Min(0.01f)] float _exitDuration = 0.2f;

    public bool IsBusy => _isPlaying || _messages.Count > 0;
    readonly Queue<string> _messages = new();

    RectTransform _noticeRectObj;
    TMP_Text _text;
    CanvasGroup _canvasGroup;
    bool _isPlaying;

    protected override void Awake()
    {
        base.Awake();

        _noticeRectObj = Instantiate(_noticePrefab, transform);
        _text = _noticeRectObj.GetComponentInChildren<TMP_Text>(true);
        _canvasGroup = _noticeRectObj.GetComponentInChildren<CanvasGroup>();
        _noticeRectObj.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        PlayNext();
    }

    void OnDisable()
    {
        Clear();
    }

    public void ShowMsg(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _messages.Enqueue(message);
        PlayNext();
    }

    public void ShowCountdown(int seconds)
    {
        Clear();

        _messages.Enqueue($"{seconds}");
        PlayNext(.99f);
    }

    Sequence _noticeSequence;
    void PlayNext(float maxDuration = 0f)
    {
        if (_noticeRectObj == null ||_isPlaying || _messages.Count == 0)
            return;

        _isPlaying = true;
        _text.text = _messages.Dequeue();

        _noticeRectObj.anchoredPosition = _displayPosition + (Vector2.down * _enterDistance);
        _canvasGroup.alpha = 0f;
        _noticeRectObj.gameObject.SetActive(true);

        _noticeSequence = DOTween.Sequence();
        // Timescale Feedback 중에도 UI 정상속도로 연출
        _noticeSequence.SetUpdate(true);

        // maxDuration에 맞게 각 duration 조정
        float enterDuration = _enterDuration;
        float holdDuration =  _holdDuration;
        float exitDuration = _exitDuration;
        float totalDuration = enterDuration + holdDuration + exitDuration;
        if (maxDuration > 0f && totalDuration > maxDuration)
        {
            float scale = maxDuration / totalDuration;

            enterDuration *= scale;
            holdDuration *= scale;
            exitDuration *= scale;
        }

        // Enter
        _noticeSequence.Append(_noticeRectObj.DOAnchorPos(_displayPosition, enterDuration).SetEase(Ease.OutCubic));
        _noticeSequence.Join(_canvasGroup.DOFade(1f, enterDuration).SetEase(Ease.Linear));

        // Hold
        _noticeSequence.AppendInterval(holdDuration);

        // Exit
        _noticeSequence.Append(_noticeRectObj.DOAnchorPos(_displayPosition + Vector2.up * _exitDistance, exitDuration).SetEase(Ease.InCubic));
        _noticeSequence.Join(_canvasGroup.DOFade(0f, exitDuration).SetEase(Ease.Linear));
        _noticeSequence.OnComplete(() =>
        {
            _noticeSequence = null;
            _isPlaying = false;
            _noticeRectObj.gameObject.SetActive(false);
            PlayNext();
        });
    }

    public void Clear()
    {
        _messages.Clear();

        _noticeSequence?.Kill();
        _noticeSequence = null;
        _isPlaying = false;

        _noticeRectObj.anchoredPosition = _displayPosition;
        _canvasGroup.alpha = 0f;
        _noticeRectObj.gameObject.SetActive(false);
    }
}
