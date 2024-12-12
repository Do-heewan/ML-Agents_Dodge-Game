using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Noh_MoveAgent : Agent
{
    public float gymLimitX = 3.8f;
    public float gymLimitY = 3.8f;
    public float speed = 2f;
    private int score;
    public float gameDuration = 120f; // 게임 지속 시간 (2분)
    private float elapsedTime = 0f;
    private bool gameEnded = false;

    private int itemCount;
    private int bulletHit;

    [SerializeField] private Transform targetItem;
    private List<GameObject> activeBullets = new List<GameObject>();

    public override void OnEpisodeBegin()
    {
        elapsedTime = 0f;
        score = 0;
        gameEnded = false;
        
        itemCount = 0;
        bulletHit = 0;
        // DestroyAllGameObjects();

        Debug.Log("Game Start");
        transform.position = new Vector3(
            Random.Range(-gymLimitX, gymLimitX),
            Random.Range(-gymLimitY, gymLimitY),
            -0.01f
        );
        
        GameObject itemObject = GameObject.FindGameObjectWithTag("Item");
        if (itemObject != null){
            targetItem = itemObject.transform;
        }

        StartCoroutine(GameTimer());
    }

    const int k_NoAction = 0;
    const int k_Up = 1;
    const int k_Down = 2;
    const int k_Left = 3;
    const int k_Right = 4;
    public override void OnActionReceived(ActionBuffers actions) {
        var action = actions.DiscreteActions[0];
        var targetPos = transform.position;

        // Debug.Log($"Action Received: {action}");

        switch(action) {
            case k_NoAction:
                break;
            case k_Up:
                targetPos = targetPos + new Vector3(0, 0.1f, 0);
                break;
            case k_Down:
                targetPos = targetPos + new Vector3(0, -0.1f, 0);
                break;
            case k_Left:
                targetPos = targetPos + new Vector3(-0.1f, 0, 0);
                break;
            case k_Right:
                targetPos = targetPos + new Vector3(0.1f, 0, 0);
                break;
            default:
                Debug.Log("error");
                break;
        }

        // 범위 제한 적용
        targetPos.x = Mathf.Clamp(targetPos.x, -3.5f, 3.5f);
        targetPos.y = Mathf.Clamp(targetPos.y, -3.5f, 3.5f);

        transform.position = targetPos;

        // episode_end();
        // early_stop();
        // avoid_Bullet();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);

        if (targetItem != null)
        {
            sensor.AddObservation(targetItem.position);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        // Item 위치를 인식하여 추가
        // GameObject itemObject = GameObject.FindGameObjectWithTag("Item");
        GameObject itemObject = GameObject.Find("Item(Clone)");
        if (itemObject != null) {
            sensor.AddObservation(itemObject.transform.position);
        }
        else {
            sensor.AddObservation(Vector3.zero);
        }

        activeBullets = GetActiveBullets();
        foreach (var bullet in activeBullets)
        {
            sensor.AddObservation(bullet.transform.position);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var DiscreteActionsOut = actionsOut.DiscreteActions;
        DiscreteActionsOut[0] = k_NoAction;

        if (Input.GetKey(KeyCode.W)){
            DiscreteActionsOut[0] = k_Up;
        }
        else if (Input.GetKey(KeyCode.S)){
            DiscreteActionsOut[0] = k_Down;
        }
        else if (Input.GetKey(KeyCode.A)){
            DiscreteActionsOut[0] = k_Left;
        }
        else if (Input.GetKey(KeyCode.D)){
            DiscreteActionsOut[0] = k_Right;

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            AddReward(3f);
            score += 3;
            itemCount++;
            // Debug.Log($"[Item Collected] {gameObject.tag} {GetCumulativeReward()} {bulletHit}");
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Bullet"))
        {
            AddReward(-2f);
            score -= 2;
            bulletHit++;
            // Debug.Log($"[Hit by Bullet] {gameObject.tag} {GetCumulativeReward()} {bulletHit}");
            Destroy(other.gameObject);
        }
    }
    
    private List<GameObject> GetActiveBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        return new List<GameObject>(bullets);
    }

    // 120s 후 에피소드 종료
    private float serverTime;
    void episode_end() {
        serverTime += Time.deltaTime;
        if (serverTime >= 120f) {
            Debug.Log($"[Final Score] Agent {name}: {score}, {itemCount}, {bulletHit}");
            // Debug.Log($"Get Item = {itemCount}, Bullet Hit = {bulletHit}");
            EndEpisode();
            serverTime = 0f;
        }
    }

    // 리워드가 일정 이상일 경우 에피소드 종료
    void early_stop() {
        if (GetCumulativeReward() > 40) {
            EndEpisode();
        }
    }

    // 총알을 일정 횟수 이상 맞으면 에피소드 종료
    void avoid_Bullet() {
        if (10 - bulletHit < 0)
            EndEpisode();
    }

    private IEnumerator GameTimer()
    {
        yield return new WaitForSeconds(gameDuration);

        if (!gameEnded)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        Debug.Log($"[Final Score] Agent {name}: {score}");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 종료
#else
        Application.Quit(); // 빌드된 환경에서 실행 종료
#endif
    }

    private void DestroyAllGameObjects()
    {
        // Item 제거
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }

        // Bullet 제거
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        // Spawner 제거 (Spawner 태그가 있다고 가정)
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (GameObject spawner in spawners)
        {
            Destroy(spawner);
        }
    }

    public float bounceForce = 0.5f; // 튕기는 힘의 크기
    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject otherGameObject = other.gameObject;
        if (otherGameObject.CompareTag("Player2") || otherGameObject.CompareTag("Player1"))
        {
            Vector3 collisionDirection = otherGameObject.transform.position - transform.position;
            collisionDirection = collisionDirection.normalized;
            transform.position -= collisionDirection * bounceForce;
        }
    }
}
