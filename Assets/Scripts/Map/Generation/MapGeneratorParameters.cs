using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MapGeneratorParameters
{
    [SerializeField]
    public int GenerationRadius;

    [SerializeField]
    public int MaxCellCount;

    [SerializeField]
    public int MinCellCount;

    [SerializeField]
    public int MaxCellSize;

    [SerializeField]
    public int MinCellSize;

    [SerializeField]
    public float RoomThresholdMultiplier;

    [SerializeField]
    public float MazeFactor;

    [SerializeField]
    public int MinCorridorWidth;

    [SerializeField]
    public int MaxCorridorWidth;

    [SerializeField]
    [Range(1, 20)]
    public int MinRoomDistance;

    [SerializeField]
    public float LockFactor;

    [SerializeField]
    public float PitFrequency;
}
