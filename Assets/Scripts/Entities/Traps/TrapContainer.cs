using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapContainer : MonoBehaviour
{
    [SerializeField]
    private GameObject _slowTrap;

    [SerializeField]
    private GameObject _spikeTrap;

    public (GameObject, Trap) GetRandomTrap()
    {
        List<GameObject> traps = GetTrapList();

        int index = Random.Range(0, traps.Count);
        return (traps[index], traps[index].GetComponent<Trap>());
    }

    private List<GameObject> GetTrapList()
    {
        return new List<GameObject>()
        {
           _slowTrap,
           _spikeTrap
        };
    }
}
