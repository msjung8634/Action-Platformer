using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WaveManager : Singleton<WaveManager>
{
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
        _gameCts = new CancellationTokenSource();
        StartGameAsync(_gameCts.Token).Forget();
    }

    void OnDisable()
    {
        StopGame();
    }

    #region Start/Stop Game

    async UniTask StartGameAsync(CancellationToken token)
    {
        try
        {
            Debug.Log("Game Start!");

            // Wave 시작까지 대기
            await WaveStartCountdownAsync(_startDelaySeconds, token);

            for (int i = 0; i < _waveDatas.Length; i++)
            {
                CurrentWave = i + 1;
                Debug.Log($"Wave{CurrentWave} Start!");

                WaveData waveData = _waveDatas[i];
                await StartWaveAsync(waveData, token);
                await UniTask.WaitUntil(IsAllEnemiesRemoved, cancellationToken: token);
                Debug.Log($"Wave{CurrentWave} Clear!");

                if (i == _waveDatas.Length - 1)
                    break;

                // 다음 Wave 시작까지 대기
                await WaveStartCountdownAsync(waveData.NextWaveDelaySeconds, token);
            }

            IsCleared = true;
            Debug.Log("Game Clear!");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Game Canceled");
        }
    }

    public void StopGame()
    {
        if (_gameCts == null)
            return;

        _gameCts.Cancel();
        _gameCts.Dispose();
        _gameCts = null;
    }

    bool IsAllEnemiesRemoved()
    {
        _spawnedEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
        return _spawnedEnemies.Count == 0;
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

    async UniTask WaveStartCountdownAsync(float durationSeconds, CancellationToken token)
    {
        float endTime = Time.time + durationSeconds;

        while (true)
        {
            float remainingTime = endTime - Time.time;
            if (remainingTime <= 0f)
                break;

            int remainingSeconds = Mathf.CeilToInt(remainingTime);
            Debug.Log($"Wave{CurrentWave + 1} Start in {remainingSeconds}...");

            float waitSeconds = remainingTime - (remainingSeconds - 1);
            await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: token);
        }
    }
}
