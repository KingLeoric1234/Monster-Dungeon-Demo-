using UnityEngine;

public class BeeSwarm : MonoBehaviour
{
    [Header("Setup")]
    public GameObject beePrefab;
    public int beeCount = 3;
    public float wanderRadius = 1.5f;
    public float moveSpeed = 2f;

    private GameObject[] bees;
    private Vector3[] targets;
    private Vector3 hivePos;

    void Start()
    {
        hivePos = transform.position;
        bees = new GameObject[beeCount];
        targets = new Vector3[beeCount];

        for (int i = 0; i < beeCount; i++)
        {
            bees[i] = Instantiate(beePrefab, hivePos, Quaternion.identity, transform);
            bees[i].transform.localPosition = Random.insideUnitSphere * wanderRadius;
            targets[i] = GetNewTarget();
        }
    }

    void Update()
    {
        for (int i = 0; i < beeCount; i++)
        {
            if (bees[i] == null) continue;

            bees[i].transform.position = Vector3.MoveTowards(
                bees[i].transform.position, targets[i], moveSpeed * Time.deltaTime);

            if (Vector3.Distance(bees[i].transform.position, targets[i]) < 0.1f)
            {
                targets[i] = GetNewTarget();
            }

            // 小幅度上下浮动
            float bob = Mathf.Sin(Time.time * 10f + i * 2f) * 0.05f;
            bees[i].transform.localPosition += new Vector3(0, bob * Time.deltaTime, 0);
        }
    }

    Vector3 GetNewTarget()
    {
        return hivePos + (Vector3)Random.insideUnitCircle * wanderRadius;
    }
}
