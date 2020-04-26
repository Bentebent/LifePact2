using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummoningSpell : MonoBehaviour
{
    [SerializeField]
    private float _manaCost = 0.0f;

    [SerializeField]
    private GameObject _minionPrefab = null;

    [SerializeField]
    private int _minionCount = 0;

    public float ManaCost => _manaCost;

    public List<GameObject> PerformSummon(Vector3 origin, GameObject owner)
    {
        List<GameObject> minions = new List<GameObject>();
        if (_minionCount == 1)
        {
            minions.Add(Instantiate(_minionPrefab, origin, Quaternion.identity));
        }
        else
        {
            for(int i = 0; i < _minionCount; i++)
            {
                minions.Add(Instantiate(_minionPrefab, origin + RandomHelper.RandomPointInCircle(1.0f).ToVector3(), 
                    Quaternion.identity));
            }
        }

        minions.ForEach(x =>
        {
            x.GetComponent<Summon>().SetOwner(owner);
        });

        return minions;
    }
}
