// Assets/Scripts/Enemy/EnemyHealth.cs（重写：只扣血+死亡，击退由CombatDirector处理）
using UnityEngine;
using System.Collections;
using Game.Core;

namespace Game.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        private MonsterTemplate template;
        private int currentHealth;
        private int maxHealth;
        [SerializeField] private HealthBar healthBar;  // 怪物血条
        [SerializeField] private bool isInvincible = false;  // 无敌（假人用，不会死）
        [SerializeField] private GameObject hitParticlePrefab; // 受击粒子特效


        public void Initialize(MonsterTemplate template)
        {
            this.template = template;
            maxHealth = template.maxHealth;
            currentHealth = template.maxHealth;
            transform.localScale = Vector3.one * template.size;

            if (healthBar != null)
            {
                healthBar.Initialize(maxHealth);
            }
        }


        /// <summary>只扣血+死亡判定，击退由 CombatDirector 处理</summary>
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;
        private Coroutine flashRoutine;

        private void Awake()
        {
            // 找所有SpriteRenderer（包括子物体）
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers.Length > 0)
            {
                originalColors = new Color[spriteRenderers.Length];
                for (int i = 0; i < spriteRenderers.Length; i++)
                    originalColors[i] = spriteRenderers[i].color;
            }
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;

            if (isInvincible && currentHealth <= 0)
            {
                currentHealth = 1;
            }

            // 闪红反馈
            FlashWhite();

            // 顿帧反馈：打中瞬间游戏慢动作
            HitStop();

            // 受击粒子：数量随伤害正比例，上限100
            if (hitParticlePrefab != null)
            {
                GameObject particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
                ParticleSystem ps = particle.GetComponent<ParticleSystem>();
                int count = Mathf.Min(100, Mathf.RoundToInt(0.5f * damage));
                var emission = ps.emission;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });
            }

            if (template != null)
            {
                if (template.difficulty == "Elite")
                    MessageBus.Publish(new PlaySoundMessage { Type = SoundType.EliteHurt });
                else
                    MessageBus.Publish(new PlaySoundMessage { Type = SoundType.NormalHurt });
            }

            // 只有非无敌单位才会死
            if (currentHealth <= 0 && !isInvincible)
            {
                Die(); // 死亡函数
            }

            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth);
            }
        }

        private void Die()
        {
            // 细胞怪分裂检查：能分裂就分裂，不真死
            CellSplit cellSplit = GetComponent<CellSplit>();
            if (cellSplit != null && cellSplit.OnDeathSplit())
            {
                FindObjectOfType<EnemyDirector>()?.OnEnemyDied(gameObject);
                Destroy(gameObject);
                return;
            }

            // 怪物死亡音效（有template才播）
            if (template != null)
            {
                if (template.difficulty == "Elite")
                    MessageBus.Publish(new PlaySoundMessage { Type = SoundType.EliteDie });
                else
                    MessageBus.Publish(new PlaySoundMessage { Type = SoundType.NormalDie });
            }

            // 死亡溶解粒子：淡黄色往上飘
            SpawnDeathParticles();

            // 通知EnemyDirector移除自己
            FindObjectOfType<EnemyDirector>()?.OnEnemyDied(gameObject);
            Destroy(gameObject);
        }

        private void SpawnDeathParticles()
        {
            GameObject obj = new GameObject("DeathParticles");
            obj.transform.position = transform.position;
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 0.5f;
            main.startSize = 0.1f;
            main.startColor = Color.white;
            main.maxParticles = 10;
            main.loop = false;
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(1f, 2f); // 慢慢往上飘
            vel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); // 左右随机摆动
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = new ParticleSystem.MinMaxGradient(g);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            var ad = obj.AddComponent<ParticleAutoDestroy>();
            ad.lifetime = 0.9f;
            ps.Play();
        }

        public void OnEnemyDied(GameObject enemy) //确认EnemyDirector.OnEnemyDied正确移除
        {
            // 通知EnemyDirector移除自己
            FindObjectOfType<EnemyDirector>()?.OnEnemyDied(gameObject);

        }

        public MonsterTemplate Template => template;
        public int CurrentHealth => currentHealth;
        public string TemplateName => template.monsterName;

        /// <summary>设置血量（假人回血红）</summary>
        public void SetHealth(int health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            if (healthBar != null) healthBar.UpdateHealth(currentHealth);
        }

        /// <summary>获取最大血量</summary>
        public int GetMaxHealth() => maxHealth;

        private void FlashWhite()
        {
            // Debug.Log("[FlashWhite] 调用闪白, 找到" + (spriteRenderers != null ? spriteRenderers.Length : 0) + "个SpriteRenderer");
            if (spriteRenderers == null || spriteRenderers.Length == 0) return;
            for (int i = 0; i < spriteRenderers.Length; i++)
                Debug.Log("[FlashWhite] 物体=" + spriteRenderers[i].gameObject.name + " 路径=" + GetPath(spriteRenderers[i].transform));
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }

        private void OnDestroy()
        {
            // 兜底：物体销毁时恢复timeScale
            if (Time.timeScale < 1f) Time.timeScale = 1f;
        }

        private IEnumerator FlashRoutine()
        {
            float duration = template != null ? template.flashDuration : 0.1f;
            float inTime = duration * 0.2f;  // 0.02秒快速变红
            float outTime = duration * 0.8f; // 0.08秒淡出

            // 快速变红
            float t = 0;
            while (t < inTime)
            {
                t += Time.deltaTime;
                float lerp = t / inTime;
                for (int i = 0; i < spriteRenderers.Length; i++)
                    spriteRenderers[i].color = Color.Lerp(originalColors[i], Color.red, lerp);
                yield return null;
            }

            // 快速淡出恢复
            t = 0;
            while (t < outTime)
            {
                t += Time.deltaTime;
                float lerp = t / outTime;
                for (int i = 0; i < spriteRenderers.Length; i++)
                    spriteRenderers[i].color = Color.Lerp(Color.red, originalColors[i], lerp);
                yield return null;
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = originalColors[i];
        }

        private bool isHitStopping = false;

        // 顿帧：打中时时间变慢0.05秒
        private void HitStop()
        {
            if (isHitStopping) return;
            //Debug.Log("顿帧触发了");
            StartCoroutine(HitStopRoutine());
        }

        private IEnumerator HitStopRoutine()
        {
            isHitStopping = true;
            Time.timeScale = 0.03f;
            float duration = template != null ? template.hitStopDuration : 0.01f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            isHitStopping = false;
        }

        /// <summary>假人专用初始化（不需要MonsterTemplate）</summary>
        public void InitializeDummy(int health)
        {
            maxHealth = health;
            currentHealth = health;
            isInvincible = true;  // 假人默认无敌
            if (healthBar != null) healthBar.Initialize(maxHealth);
        }

        /// <summary>设置无敌状态</summary>
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
        }
    }
}
