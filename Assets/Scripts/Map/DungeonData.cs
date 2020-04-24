using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class DungeonData :   MonoBehaviour
{
    [SerializeField]
    public MapGeneratorParameters Parameters;

    [SerializeField]
    public TileContainer TileSet;

    [SerializeField]
    public TileContainer PitSet;

    //[SerializeField]
    //public SpawnableContainer spawnables;

    [SerializeField]
    public InteractiveDungeonObjectContainer InteractiveObjects;

    [SerializeField]
    public TrapContainer TrapSet;
}
