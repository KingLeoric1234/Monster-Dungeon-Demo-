using UnityEngine;
using System.Collections;
using TMPro;

namespace Game.Enemy
{
    /// <summary>
    /// 伤害数字。挂在伤害数字预制体上。
    /// 从脚下蹦出，上抛运动，落地后颠簸两下，然后淡出。
    /// 普通伤害白色，暴击伤害红色且更大更久。
    /// 可调参数不再写死在本组件里，由 DamageNumberArguments 传入。
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        private TextMeshPro textMesh;
        private float startY;
        private float velocityY;
        private float velocityX;
        private int bouncesLeft;
        private float totalDuration;
        private bool isCrit;
        private DamageNumberArguments args;   // 本次动画用的参数，由 Initialize 传入

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        /// <summary>初始化伤害数字</summary>
        public void Initialize(int damage, bool crit, DamageNumberArguments arguments)
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            args = arguments;

            isCrit = crit;

            // 设置文字
            textMesh.text = damage.ToString();

            // 设置颜色和大小
            if (isCrit)
            {
                textMesh.color = args.critColor;
                textMesh.fontSize = args.critSize;
                textMesh.fontStyle = FontStyles.Bold;
            }
            else
            {
                textMesh.color = args.normalColor;
                textMesh.fontSize = args.normalSize;
                textMesh.fontStyle = FontStyles.Normal;
            }

            // 记录起始位置（脚下）
            startY = transform.position.y;

            // 计算上抛初速度 v = sqrt(2 * g * h)
            float height = isCrit ? args.jumpHeight * args.critHeightMultiplier : args.jumpHeight;
            velocityY = Mathf.Sqrt(2 * args.gravity * height);

            // 随机水平方向
            velocityX = Random.Range(-args.horizontalSpeed, args.horizontalSpeed);

            // 颠簸次数
            bouncesLeft = args.bounceCount;

            // 计算总持续时间（上抛+下落+颠簸）
            float timeToTop = velocityY / args.gravity;
            float timeToFall = timeToTop;  // 对称下落
            totalDuration = (timeToTop + timeToFall) * (isCrit ? args.critDurationMultiplier : 1f);

            // 开始动画
            StartCoroutine(PlayAnimation());
        }

        /// <summary>上抛运动+颠簸+淡出</summary>
        private IEnumerator PlayAnimation()
        {
            float timer = 0f;
            Vector3 pos = transform.position;
            bool isFading = false;

            while (timer < totalDuration || bouncesLeft > 0)
            {
                timer += Time.deltaTime;

                // 应用重力
                velocityY -= args.gravity * Time.deltaTime;

                // 更新位置
                pos.y += velocityY * Time.deltaTime;
                pos.x += velocityX * Time.deltaTime;

                // 检测落地
                if (pos.y <= startY && velocityY < 0)
                {
                    pos.y = startY;

                    if (bouncesLeft > 0)
                    {
                        // 颠簸：速度反向，乘以衰减
                        velocityY = -velocityY * args.bounceDamping;
                        bouncesLeft--;
                    }
                    else
                    {
                        // 颠簸完了，停在地面
                        velocityY = 0;
                        velocityX = 0;
                    }
                }

                transform.position = pos;

                // 最后30%时间淡出
                if (timer > totalDuration * 0.7f && !isFading)
                {
                    isFading = true;
                }

                if (isFading)
                {
                    float alpha = Mathf.Lerp(textMesh.color.a, 0f, Time.deltaTime * 5f);
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
                }

                yield return null;
            }

            // 确保完全透明后销毁
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }
    }
}
