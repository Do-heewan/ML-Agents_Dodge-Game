using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public GameObject spawnerPrefab; 
    public float gymSize = 8f; 
    public float wallThickness = 0.5f; 
    public float spawnerSpawnIntervalMin = 2f; 
    public float spawnerSpawnIntervalMax = 5f; 
    private List<GameObject> spawners = new List<GameObject>(); 

    void Start()
    {
        StartCoroutine(SpawnerRoutine());
    }

    void Update()
    {  

    }

    // 스포너 생성 루틴
    IEnumerator SpawnerRoutine()
    {
        while (true)
        {
            float randomInterval = Random.Range(spawnerSpawnIntervalMin, spawnerSpawnIntervalMax);
            yield return new WaitForSeconds(randomInterval);

            Vector3 spawnPosition = GetRandomEdgePosition();
            GameObject newSpawner = Instantiate(spawnerPrefab, spawnPosition, Quaternion.identity); 
            spawners.Add(newSpawner); 
        }
    }

    // 스포너가 생기는 위치를 랜덤으로 결정
    Vector3 GetRandomEdgePosition()
    {
        float adjustedEdge = gymSize / 2 + wallThickness;

        float x = 0f, y = 0f;

        if (Random.value > 0.5f)
        {
            x = Random.value > 0.5f ? adjustedEdge : -adjustedEdge;
            y = Random.Range(-adjustedEdge, adjustedEdge);
        }
        else
        {
            y = Random.value > 0.5f ? adjustedEdge : -adjustedEdge;
            x = Random.Range(-adjustedEdge, adjustedEdge);
        }

        return new Vector3(x, y, 0f); 
    }
}