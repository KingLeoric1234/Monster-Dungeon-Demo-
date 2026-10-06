using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    /// <summary>初始化血量：模板建档 + 体型缩放。由 EnemyHealthCalculator.Initialize 转发调用</summary>
    public class EnemyInit : MonoBehaviour
    {
        private EnemyHealthData data;   // 同物体组件，GetComponent 直取

        private void Awake()
        {
            data = GetComponent<EnemyHealthData>();
        }

        public void Initialize(MonsterTemplate template)
        {
            if (template == null) return;

            transform.localScale = Vector3.one * template.size;
            if (data != null) data.Initialize(template);
        }
    }
}
