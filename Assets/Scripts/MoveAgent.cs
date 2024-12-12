using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
#if UNITY_EDITOR
using UnityEditor; // Unity Editor 전용 네임스페이스
#endif

public class MoveAgent : Agent
{
    public float gymLimitX = 3.8f;
    public float gymLimitY = 3.8f;
    public float speed = 2f;
    public float gameDuration = 120f; // 게임 지속 시간 (2분)
    private float elapsedTime = 0f;
    private int score = 0;
    private bool gameEnded = false;

    [SerializeField] private Transform targetItem;
    private List<GameObject> activeBullets = new List<GameObject>();

    public override void OnEpisodeBegin()
    {
        Debug.Log("Game Start");
        elapsedTime = 0f;
        score = 0;
        gameEnded = false;

        transform.position = new Vector3(
            Random.Range(-gymLimitX, gymLimitX),
            Random.Range(-gymLimitY, gymLimitY),
            -0.01f
        );

        GameObject itemObject = GameObject.FindGameObjectWithTag("Item");
        if (itemObject != null)
        {
            targetItem = itemObject.transform;
        }

        // 게임 타이머 시작
        StartCoroutine(GameTimer());
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
        GameObject itemObject = GameObject.FindGameObjectWithTag("Item");
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

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (gameEnded) return;

        int action = actions.DiscreteActions[0];
        Vector3 targetPos = transform.position;

        switch (action)
        {
            case 0: break; // No action
            case 1: targetPos += new Vector3(0, 0.1f, 0); break; // UP
            case 2: targetPos += new Vector3(0, -0.1f, 0); break; // Down
            case 3: targetPos += new Vector3(-0.1f, 0, 0); break; // Left
            case 4: targetPos += new Vector3(0.1f, 0, 0); break; // Right
        }

        targetPos.x = Mathf.Clamp(targetPos.x, -gymLimitX, gymLimitX);
        targetPos.y = Mathf.Clamp(targetPos.y, -gymLimitY, gymLimitY);
        transform.position = targetPos;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.A)) discreteActions[0] = 3;
        if (Input.GetKey(KeyCode.D)) discreteActions[0] = 4;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return;

        if (other.CompareTag("Item"))
        {
            AddReward(3f);
            score += 3;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Bullet"))
        {
            AddReward(-2f);
            score -= 2;
            Destroy(other.gameObject);
        }
    }

    private List<GameObject> GetActiveBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        return new List<GameObject>(bullets);
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
