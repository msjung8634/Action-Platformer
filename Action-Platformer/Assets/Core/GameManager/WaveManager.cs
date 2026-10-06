using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class WaveManager : Singleton<WaveManager>
{
    [Header("Restart")]
    [SerializeField] PlayerResourceManager _playerResourceManager;
    [SerializeField] GameObject _restartPanel;
    [SerializeField, Min(0f)] float _restartInputDelay = 0.5f;
    bool _isGameOver;
    bool _isRestarting;
    float _restartInputEnableTime;
    public event Action OnRestartGame;

    [Header("Spawn Points")]
    [SerializeField] Transform _leftSpawnPoint;
    [SerializeField] Transform _rightSpawnPoint;

    [Header("Wave")]
    [SerializeField, Min(0f)] float _startDelaySeconds = 3f;
    [SerializeField] WaveData[] _waveDatas;

    readonly List<GameObject> _spawnedEnemies = new();
    CancellationTokenSource _gameCts;

    public int CurrentWave { get; private set; }
    public int TotalWaves => _waveDatas.Length;
    public bool IsCleared { get; private set; }

    void Start()
    {
        StartGame();
    }

    void OnEnable()
    {
        _playerResourceManager.OnDead += OnPlayerDead;
    }

    void OnDisable()
    {
        _playerResourceManager.OnDead -= OnPlayerDead;
        StopGame();
    }

    void Update()
    {
        if ((!_isGameOver && !IsCleared) || _isRestarting)
            return;

        if (Time.unscaledTime < _restartInputEnableTime)
            return;

        if (CheckAnyButtonPressed())
            RestartGame();
    }
    bool CheckAnyButtonPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void OnPlayerDead()
    {
        _isGameOver = true;
        StopGame();
        NoticeUI.Instance.Clear();

        foreach (GameObject enemy in _spawnedEnemies)
        {
            if (enemy == null)
                continue;

            if (enemy.TryGetComponent<EnemyBrain>(out var brain))
                brain.enabled = false;
        }

        ShowRestartPanel();
    }
    void ShowRestartPanel()
    {
        _restartInputEnableTime = Time.unscaledTime + _restartInputDelay;
        _restartPanel.SetActive(true);
    }

    #region Start Game
    void StartGame(bool isRestart = false)
    {
        if (isRestart)
        {
            NoticeUI.Instance.ShowMsg($"어디서 본듯 한 놈이군...");
        }

        CurrentWave = 0;
        IsCleared = false;
        _isGameOver = false;
        _isRestarting = false;
        _restartInputEnableTime = 0f;

        _restartPanel.SetActive(false);

        _gameCts = new CancellationTokenSource();
        StartGameAsync(_gameCts.Token).Forget();
    }
    async UniTask StartGameAsync(CancellationToken token)
    {
        try
        {
            await WaveStartCountdownAsync(_startDelaySeconds, token);

            for (int i = 0; i < _waveDatas.Length; i++)
            {
                CurrentWave = i + 1;
                NoticeUI.Instance.ShowMsg($"{CurrentWave}번째 전투");
                NoticeUI.Instance.ShowMsg($"{_waveDatas[i].StartMsg}");

                WaveData waveData = _waveDatas[i];
                await StartWaveAsync(waveData, token);
                await UniTask.WaitUntil(IsAllEnemiesRemoved, cancellationToken: token);
                NoticeUI.Instance.ShowMsg($"모든 적 처치");

                if (i == _waveDatas.Length - 1)
                    break;

                // 다음 Wave 시작까지 대기
                await WaveStartCountdownAsync(waveData.NextWaveDelaySeconds, token);
            }

            IsCleared = true;
            NoticeUI.Instance.ShowMsg($"간만에 실력이 좋은놈이 왔군 . . .");
            NoticeUI.Instance.ShowMsg($"또 보자고 친구 . . ");

            await UniTask.WaitUntil(() => !NoticeUI.Instance.IsBusy, cancellationToken: token);
            // 사망시 재시작 활용
            OnPlayerDead();
        }
        catch (OperationCanceledException)
        {
            
        }
    }
    async UniTask WaveStartCountdownAsync(float durationSeconds, CancellationToken token)
    {
        NoticeUI.Instance.ShowMsg($"다음 웨이브 시작까지...");
        await UniTask.WaitUntil(() => !NoticeUI.Instance.IsBusy, cancellationToken: token);

        float endTime = Time.time + durationSeconds;

        while (true)
        {
            float remainingTime = endTime - Time.time;
            if (remainingTime <= 0f)
                break;

            int remainingSeconds = Mathf.CeilToInt(remainingTime);
            NoticeUI.Instance.ShowCountdown(remainingSeconds);

            float waitSeconds = remainingTime - (remainingSeconds - 1);
            await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: token);
        }
    }
    bool IsAllEnemiesRemoved()
    {
        _spawnedEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
        return _spawnedEnemies.Count == 0;
    }

    #endregion
    #region Stop Game
    public void StopGame()
    {
        if (_gameCts == null)
            return;

        _gameCts.Cancel();
        _gameCts.Dispose();
        _gameCts = null;
    }

    #endregion
    #region StartWave

    async UniTask StartWaveAsync(WaveData waveData, CancellationToken token)
    {
        var timeline = new List<WaveSpawnData>(waveData.Spawns);
        timeline.Sort((a, b) => a.SpawnTimeSeconds.CompareTo(b.SpawnTimeSeconds));

        float waveStartTime = Time.time;

        foreach (WaveSpawnData spawnData in timeline)
        {
            float spawnTime = waveStartTime + spawnData.SpawnTimeSeconds;
            float remainSeconds = spawnTime - Time.time;

            if (remainSeconds > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainSeconds), cancellationToken: token);
            }

            SpawnEnemy(spawnData);
        }
    }

    void SpawnEnemy(WaveSpawnData spawnData)
    {
        Transform spawnPoint = spawnData.Side == SpawnSide.Left
            ? _leftSpawnPoint
            : _rightSpawnPoint;

        Vector3 position = spawnPoint.position;
        position.x += spawnData.Offset.x;
        position.y += spawnData.Offset.y;

        GameObject enemy = Instantiate(
            spawnData.Prefab,
            position,
            Quaternion.identity);

        _spawnedEnemies.Add(enemy);
    }

    #endregion
    #region Restart Game

    public void RestartGame()
    {
        if (_isRestarting)
            return;

        _isRestarting = true;
        StopGame();
        NoticeUI.Instance.Clear();

        // 적 제거
        foreach (GameObject enemy in _spawnedEnemies)
        {
            if (enemy == null)
                continue;

            enemy.SetActive(false);
            Destroy(enemy);
        }
        _spawnedEnemies.Clear();

        // callback
        OnRestartGame?.Invoke();
        
        _restartPanel.SetActive(false);
        StartGame(true);
    }

    #endregion
}
