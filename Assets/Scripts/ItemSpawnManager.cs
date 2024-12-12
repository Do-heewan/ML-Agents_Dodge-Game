using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab; 
    public float spawnIntervalMin = 3f; 
    public float spawnIntervalMax = 5f; 
    public float gymLimitX = 3.5f; 
    public float gymLimitY = 3.5f; 

    private GameObject currentItem; 

    void Start()
    {
        StartCoroutine(SpawnItemRoutine());
    }

    IEnumerator SpawnItemRoutine()
    {
        while (true)
        {
            float randomInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(randomInterval);

            Vector3 spawnPosition = GetRandomSpawnPosition();
            Debug.Log("Item spawned at: " + spawnPosition);

            if (currentItem != null)
            {
                Destroy(currentItem);
            }

            currentItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        float X = Random.Range(-gymLimitX, gymLimitX);
        float Y = Random.Range(-gymLimitY, gymLimitY);

        return new Vector3(X, Y, -0.01f); 
    }
}
