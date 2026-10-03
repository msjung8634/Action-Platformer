using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] AudioSource _bgmSource;
    [SerializeField] AudioSource _sfxSource;

    public void StartBGM(AudioClip bgmClip)
    {
        _bgmSource.Stop();
        _bgmSource.clip = bgmClip;
        _bgmSource.Play();
    }

    public void ApplyFeedback(FeedbackData feedbackData)
    {
        if (feedbackData == null || feedbackData.SfxClip == null)
            return;

        _sfxSource.PlayOneShot(feedbackData.SfxClip, feedbackData.SfxVolume);
    }
}
