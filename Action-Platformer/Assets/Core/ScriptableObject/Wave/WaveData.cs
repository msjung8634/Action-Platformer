using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnSide
{
    Left,
    Right
}

[Serializable]
public class WaveSpawnData
{
    [Min(0f)] public float SpawnTimeSeconds;
    public GameObject Prefab;
    public SpawnSide Side;
    public Vector2 Offset;
}


[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/Wave Data")]
public class WaveData : ScriptableObject
{
    public List<WaveSpawnData> Spawns = new();
    [Min(0f)] public float NextWaveDelaySeconds = 10f;
}
