using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject bulletPrefab; 
    public float bulletSpawnIntervalMin = 2f; 
    public float bulletSpawnIntervalMax = 5f; 

    void Start()
    {
        StartCoroutine(SpawnBullets());
    }

    IEnumerator SpawnBullets()
    {
        while (true)
        {
            float randomInterval = Random.Range(bulletSpawnIntervalMin, bulletSpawnIntervalMax);
            yield return new WaitForSeconds(randomInterval);

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, -0.1f);

            Destroy(bullet, 3f);
        }
    }
}
