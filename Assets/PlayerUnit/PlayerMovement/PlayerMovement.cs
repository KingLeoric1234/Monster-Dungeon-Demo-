// Assets/Scripts/Player/PlayerMovement.cs
using UnityEngine;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// 玩家移动状态
    /// </summary>
    public enum PlayerMoveState
    {
        Walk,   // 走路
        Run     // 奔跑（Shift加速）
    }

    /// <summary>
    /// 玩家移动、边界限制、走路/奔跑状态切换、对数型加速、急刹触发。
    /// 动画播放由 PlayerFacing 负责，本脚本只负责移动逻辑和状态。
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        private bool canControl; // 判定玩家能不能移动
        private KnockBackHandler knockBack;
        private Rigidbody2D rb;
        private LayerMask obstacleLayer;
        private PlayerStamina stamina;  // 体力管理

        // 移动状态
        private PlayerMoveState currentState = PlayerMoveState.Walk;
        private Vector2 currentVelocity;     // 当前实际速度向量（用于滑行衰减）
        private bool isMoving;
        private bool wasRunning;
        private float dustTimer; // 跑步灰尘计时器

        // 急刹
        private bool isBraking;              // 是否在急刹硬直中
        private float brakeTimer;             // 急刹计时器

        // 外部强制速度（攻击减速用，-1表示不强制）
        private float forcedSpeed = -1f;

        // 最终速度（由PlayerStatsDirector设置，默认用PlayerConfig的值）
        private float finalMoveSpeed = -1f;
        private float finalRunSpeed = -1f;

        /// <summary>设置最终移动速度和奔跑速度（由PlayerStatsDirector调用）</summary>
        public void SetFinalSpeeds(float moveSpeed, float runSpeed)
        {
            finalMoveSpeed = moveSpeed;
            finalRunSpeed = runSpeed;
        }

        /// <summary>获取最终走路速度</summary>
        private float GetFinalMoveSpeed()
        {
            return finalMoveSpeed > 0f ? finalMoveSpeed : PlayerConfig.MoveSpeed;
        }

        /// <summary>获取最终奔跑速度</summary>
        private float GetFinalRunSpeed()
        {
            return finalRunSpeed > 0f ? finalRunSpeed : PlayerConfig.RunSpeed;
        }

        private void Awake()
        {
            knockBack = GetComponent<KnockBackHandler>();
            rb = GetComponent<Rigidbody2D>();
            stamina = GetComponent<PlayerStamina>();
            float playerCheckRadius = PlayerConfig.PlayerSize * PlayerConfig.PlayerSizeParameter;
            if (knockBack != null) knockBack.SetCheckRadius(playerCheckRadius);
            obstacleLayer = CombatConfig.ObstacleLayer;
        }

        private void Update()
        {
            if (!canControl) return;
            if (knockBack != null && knockBack.IsKnockedBack) return;

            // ── 急刹硬直：期间不能移动，但可以攻击 ──
            if (isBraking)
            {
                brakeTimer -= Time.deltaTime;
                if (brakeTimer <= 0f)
                {
                    isBraking = false;
                }
                return; // 急刹期间跳过移动逻辑
            }

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

            isMoving = moveDirection.sqrMagnitude > 0.01f;

            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // ── 急刹触发：松开Shift且之前在真正奔跑 ──
            if (!shiftPressed && wasRunning && currentVelocity.magnitude > GetFinalMoveSpeed() * 1.5f)
            {
                isBraking = true;
                brakeTimer = PlayerConfig.BrakeDuration;
                currentVelocity = Vector2.zero;
                wasRunning = false;
                return;
            }

            // ── 状态切换：Shift按下+移动=奔跑，松开=走路 ──
            // 只有真正在移动且按Shift才算奔跑（光按Shift不消耗PP）
            bool wantsToRun = shiftPressed && isMoving;
            bool canRun = stamina != null ? stamina.CanSprint() : true;

            if (wantsToRun && canRun)
            {
                if (currentState != PlayerMoveState.Run)
                {
                    currentState = PlayerMoveState.Run;
                }
                if (stamina != null) stamina.StartSprinting();
            }
            else
            {
                if (currentState != PlayerMoveState.Walk)
                {
                    currentState = PlayerMoveState.Walk;
                }
                if (stamina != null) stamina.StopSprinting();
            }

            wasRunning = shiftPressed && isMoving;

            // ── 指数趋近移动：起跑/转向加速快，停下滑行慢 ──
            float targetSpeed = (currentState == PlayerMoveState.Run && isMoving)
                ? GetFinalRunSpeed()
                : GetFinalMoveSpeed();
            Vector2 targetVelocity = isMoving ? moveDirection * targetSpeed : Vector2.zero;

            // 输入时用加速系数，没输入时用减速系数（滑行更远）
            float k = isMoving ? PlayerConfig.MoveAccelK : PlayerConfig.MoveDecelK;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.deltaTime * k);

            // 实际移动用currentVelocity
            Vector2 moveVec = currentVelocity;
            if (forcedSpeed > 0f)
            {
                moveVec = moveDirection * forcedSpeed;
            }

            // 分轴检测碰撞：X和Y分开，被挡的轴单独停，另一个轴继续走（沿墙滑动）
            float collisionRadius = PlayerConfig.PlayerSize * PlayerConfig.PlayerSizeParameter;

            // 先试X方向
            Vector2 deltaX = new Vector2(moveVec.x * Time.deltaTime, 0);
            Vector2 posX = (Vector2)transform.position + deltaX;
            Collider2D hitX = Physics2D.OverlapCircle(posX, collisionRadius, obstacleLayer);
            if (hitX == null)
                transform.position = posX;

            // 再试Y方向
            Vector2 deltaY = new Vector2(0, moveVec.y * Time.deltaTime);
            Vector2 posY = (Vector2)transform.position + deltaY;
            Collider2D hitY = Physics2D.OverlapCircle(posY, collisionRadius, obstacleLayer);
            if (hitY == null)
                transform.position = posY;

            // 跑步时生成灰尘粒子
            dustTimer -= Time.deltaTime;
            if (IsRunning() && dustTimer <= 0f)
            {
                SpawnDust();
                dustTimer = 0.15f;
            }
        }

        private void SpawnDust()
        {
            GameObject dust = new GameObject("Dust");
            dust.transform.position = transform.position + new Vector3(0, -0.3f, 0);
            ParticleSystem ps = dust.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.15f;
            main.startSpeed = 0f;
            main.startSize = 0.1f;
            main.startColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            main.maxParticles = 5;
            main.loop = false;
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 2) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            var autoDestroy = dust.AddComponent<ParticleAutoDestroy>();
            autoDestroy.lifetime = 0.15f;
            ps.Play();
        }

        // ── 公共状态读取（供 PlayerFacing 等其他脚本读取） ──

        /// <summary>获取当前移动状态（走路/奔跑）</summary>
        public PlayerMoveState GetCurrentState() => currentState;

        /// <summary>获取当前速度向量（相机预判用）</summary>
        public Vector2 GetCurrentVelocity() => currentVelocity;

        /// <summary>是否在移动（输入或滑行中）</summary>
        public bool IsMoving() => isMoving || currentVelocity.magnitude > PlayerConfig.MoveStopThreshold;

        /// <summary>是否在真正奔跑（速度超过走路速度的一半）</summary>
        public bool IsRunning() => currentState == PlayerMoveState.Run && currentVelocity.magnitude > GetFinalMoveSpeed() * 1.5f;

        /// <summary>是否在急刹硬直中</summary>
        public bool IsBraking() => isBraking;

        public void EnableControl() => canControl = true;
        public void DisableControl() => canControl = false;
        public void ResetPosition()
        {
            GameObject spawn = GameObject.Find("SpawnPoint");
            if (spawn != null)
                transform.position = spawn.transform.position;
            else
                transform.position = Vector3.zero;
        }

        /// <summary>设置强制速度（攻击减速用）</summary>
        public void SetForcedSpeed(float speed) => forcedSpeed = speed;

        /// <summary>清除强制速度，恢复正常移动</summary>
        public void ClearForcedSpeed() => forcedSpeed = -1f;

        /// <summary>获取当前实际速度</summary>
        public float GetCurrentSpeed()
        {
            if (forcedSpeed > 0f) return forcedSpeed;
            return currentVelocity.magnitude;
        }
    }
}
