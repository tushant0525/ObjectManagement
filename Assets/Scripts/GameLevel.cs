using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    [SerializeField]
    SpawnZone spawnZone;

    void Start()
    {
        GameManager.Instance.SpawnZoneOfLevel = spawnZone;
    }
}
