using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartSpawner : MonoBehaviour
{
    [SerializeField]
    private Part[] prefabsToSpawn;

    [SerializeField]
    private float minSpawnDistance = default;

    [SerializeField]
    private float maxSpawnDistance = default;

    private Vector3 spawnPos;

    // Start is called before the first frame update
    void Start()
    {
        spawnPos = GetComponent<Transform>().position;
        prefabsToSpawn = FindObjectsOfType<Part>();
        foreach(Part each in prefabsToSpawn)
        {
            Spawn(each, 2);
        }
    }

    private void Spawn(Part part, int count)
    {
        for(int i=0; i<count; i++)
        {
            Part tempPart = Instantiate(part, new Vector3(spawnPos.x + UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance), spawnPos.y, spawnPos.z + UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance)), new Quaternion());
            tempPart.SetToPhysical();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
