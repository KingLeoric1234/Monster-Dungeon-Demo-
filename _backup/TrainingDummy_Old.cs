using UnityEngine;
using System.Collections;

namespace Game.Enemy
{
    /// <summary>
    /// 战斗假人。
    /// 无法移动，有血量，无法被打死，3秒后血量回满。
    /// 挂在主世界的假人物体上，需要EnemyHealth和BoxCollider2D。
    /// </summary>
    public class TrainingDummy : MonoBehaviour
    {
        [Header("假人设置")]
        [SerializeField] private float regenDelay = 3f;       // 多少秒后开始回血
        [SerializeField] private float regenSpeed = 50f;      // 每秒回血量

        [Header("血量显示")]
        [SerializeField] private TMPro.TMP_Text healthText;   // 血量数字显示（x/x格式）

        private EnemyHealth enemyHealth;
        private Coroutine regenCoroutine;
        private int lastHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void Start()
        {
            if (enemyHealth != null)
            {
                // 如果EnemyHealth没有被Initialize，用假人专用初始化
                if (enemyHealth.GetMaxHealth() <= 0)
                {
                    enemyHealth.InitializeDummy(100);  // 默认100血
                }
                lastHealth = enemyHealth.CurrentHealth;
            }
        }

        private void Update()
        {
            if (enemyHealth == null) return;

            // 更新血量数字显示
            UpdateHealthText();

            // 检测血量变化
            if (enemyHealth.CurrentHealth < lastHealth)
            {
                // 受到伤害了，触发回血
                OnTakeDamage();
            }
            lastHealth = enemyHealth.CurrentHealth;

            // 确保假人不会死（留1滴血）
            if (enemyHealth.CurrentHealth <= 0)
            {
                enemyHealth.SetHealth(1);
            }
        }

        /// <summary>更新血量数字显示</summary>
        private void UpdateHealthText()
        {
            if (healthText != null)
            {
                healthText.text = $"{enemyHealth.CurrentHealth}/{enemyHealth.GetMaxHealth()}";
            }
        }

        /// <summary>受到伤害时触发回血</summary>
        private void OnTakeDamage()
        {
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
            }
            regenCoroutine = StartCoroutine(RegenerateHealth());
        }

        /// <summary>回血协程</summary>
        private IEnumerator RegenerateHealth()
        {
            // 等待regenDelay秒
            yield return new WaitForSeconds(regenDelay);

            // 持续回血直到满
            int maxHealth = enemyHealth.GetMaxHealth();
            while (enemyHealth.CurrentHealth < maxHealth)
            {
                int newHealth = Mathf.CeilToInt(enemyHealth.CurrentHealth + regenSpeed * Time.deltaTime);
                enemyHealth.SetHealth(newHealth);
                yield return null;
            }
        }
    }
}
