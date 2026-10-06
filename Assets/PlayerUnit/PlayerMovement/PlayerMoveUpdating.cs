using UnityEngine;
using Game.Player;

public class PlayerBobbing : MonoBehaviour
{
    [Header("摆动设置")]
    [SerializeField] private float bobHeight = 0.05f;   // 上下摆动幅度
    [SerializeField] private float bobSpeed = 0.5f;     // 一次完整上下来回的时间（秒）

    private Vector3 basePos;
    private float t = 0f;

    private void Start()
    {
        basePos = transform.localPosition;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isMoving = (h != 0 || v != 0);

        if (isMoving)
        {
            t += Time.deltaTime;

            // PingPong：在 0..1..0 之间线性来回（三角波）
            float p = Mathf.PingPong(t, bobSpeed) / bobSpeed;   // 结果 0→1→0
            float offset = (p - 0.5f) * 2f * bobHeight;          // 映射到 -height..+height

            transform.localPosition = new Vector3(basePos.x, basePos.y + offset, basePos.z);
        }
        else
        {
            t = 0f;
            transform.localPosition = basePos;
        }
    }
}
