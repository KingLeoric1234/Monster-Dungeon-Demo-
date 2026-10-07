using UnityEngine;
using Game.World;
using Game.Core;
using Game.Player;

namespace Game.Core
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform target;
        private Vector3 offset;
        private Vector2 freeOffset;
        private float recenterTimer;
        private Vector2 lastInputDir = Vector2.zero;
        private float turnDelayTimer;

        private void OnEnable() => MessageBus.Subscribe<DungeonSpawnPointMessage>(OnSpawnPoint);
        private void OnDisable() => MessageBus.Unsubscribe<DungeonSpawnPointMessage>(OnSpawnPoint);

        private void Start()
        {
            TryFindPlayer();
            if (target != null)
                offset = transform.position - target.position;
        }

        private void OnSpawnPoint(DungeonSpawnPointMessage msg)
        {
            TryFindPlayer();
            transform.position = new Vector3(msg.Position.x, msg.Position.y, transform.position.z);
            offset = new Vector3(0, 0, offset.z);  // 清零xy偏移，完美居中
            freeOffset = Vector2.zero;
            recenterTimer = 0f;
            turnDelayTimer = 0f;
            // Debug.Log("[CameraFollow] 相机已对齐出生点");
        }

        private void TryFindPlayer()
        {
            if (PlayerDirector.Instance != null)
            {
                target = PlayerDirector.Instance.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                TryFindPlayer();
                return;
            }

            float dt = Time.deltaTime;

            // 1. 死区内自由偏移：输入方向驱动，指数平滑，有上限
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 inputDir = new Vector2(h, v).normalized;

            // 转向检测：方向变化超过阈值就启动延迟
            if (Vector2.Dot(inputDir, lastInputDir) < 0.5f && inputDir != Vector2.zero)
                turnDelayTimer = PlayerConfig.CameraTurnDelay;
            if (turnDelayTimer > 0)
            {
                turnDelayTimer -= dt;
                lastInputDir = inputDir;
                return; // 延迟期间相机不动
            }
            lastInputDir = inputDir;

            // 松开按键就回中
            Vector2 targetFree = inputDir * PlayerConfig.CameraFreeRadius;
            float freeK = PlayerConfig.CameraInputSmooth;
            float freeT = 1f - Mathf.Exp(-freeK * dt);
            freeOffset = Vector2.Lerp(freeOffset, targetFree, freeT);

            // 2. 计算目标位置 = 玩家位置 + 偏移 + 自由偏移
            Vector3 desiredPosition = new Vector3(
                target.position.x + offset.x + freeOffset.x,
                target.position.y + offset.y + freeOffset.y,
                transform.position.z);

            // 3. 走出死区：计时0.2s后回中
            float dx = desiredPosition.x - transform.position.x;
            float dy = desiredPosition.y - transform.position.y;
            float distFromCenter = new Vector2(dx, dy).magnitude;

            if (distFromCenter > PlayerConfig.CameraDeadzoneX)
            {
                recenterTimer += dt;
                if (recenterTimer >= 0.2f)
                {
                    // 回中：freeOffset归零
                    freeOffset = Vector2.Lerp(freeOffset, Vector2.zero, 1f - Mathf.Exp(-8f * dt));
                }
            }
            else
            {
                recenterTimer = 0;
            }

            // 重新算回中后的目标
            desiredPosition = new Vector3(
                target.position.x + offset.x + freeOffset.x,
                target.position.y + offset.y + freeOffset.y,
                transform.position.z);

            // 4. 指数平滑跟随
            float k = PlayerConfig.CameraSmoothSpeed;
            float t = 1f - Mathf.Exp(-k * dt);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
        }
    }
}
