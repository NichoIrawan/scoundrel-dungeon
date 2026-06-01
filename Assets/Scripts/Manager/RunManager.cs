using Assets.Scripts;
using Assets.Scripts.ScriptableObjects;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    [SerializeField] private string seed = "default_seed";
    [SerializeField] private EffigiesDictionary _dictionary;

    public Dictionary<int, string[]> EffigiesInRoom = new();
    public Dictionary<string, int> EffigiesInRun = new();

    private void Awake()
    {
        _dictionary.Initialize();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateMap(7, seed);
        foreach (var kvp in EffigiesInRun)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
        foreach (var kvp in EffigiesInRoom)
        {
            Debug.Log($"Room {kvp.Key}:");
            foreach (var effigy in kvp.Value)
            {
                Debug.Log($"  - {effigy}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMap(int roomUnit, string seed)
    {
        var random = new System.Random(seed.GetHashCode());

        for (int i = 0; i < roomUnit; i++)
        {
            var effigiesInRoom = new string[4];

            for (int j = 0; j < 4; j++)
            {
                var rng = random.Next(1, 52);
                string effigy;

                if (rng <= 26)
                {
                    var divider = 26 / _dictionary._enemies.Count;
                    var index = Math.Clamp(rng / divider, 0, _dictionary._enemies.Count - 1);
                    effigy = _dictionary._enemies[index];
                }
                else if (rng <= 39)
                {
                    var divider = 13 / _dictionary._equipments.Count;
                    var index = Math.Clamp((rng - 26) / divider, 0, _dictionary._equipments.Count - 1);
                    effigy = _dictionary._equipments[index];
                }
                else
                {
                    var divider = 13 / _dictionary._consumables.Count;
                    var index = Math.Clamp((rng - 39) / divider, 0, _dictionary._consumables.Count - 1);
                    effigy = _dictionary._consumables[index];
                }
                effigiesInRoom[j] = effigy;

                if (EffigiesInRun.TryGetValue(effigy, out int count))
                {
                    EffigiesInRun[effigy] = count + 1;
                }
                else
                {
                    EffigiesInRun[effigy] = 1;
                }
            }
            EffigiesInRoom[i] = effigiesInRoom;
        }
    }
}
