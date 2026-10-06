using UnityEngine;
using Game.Core;
using Game.Items;
using Game.Player;
using TMPro;

namespace Game.Weapons
{
    /// <summary>
    /// 手枪武器：检测子弹，生成子弹，子弹飞行碰撞扣血。
    /// 挂在玩家身上，由 SwitchWeapon 设为当前手持武器。
    /// </summary>
    public class PistolWeapon : IWeapon
    {
        [Header("引用")]
        [SerializeField] private GameObject bulletPrefab;         // 子弹预制体
        [SerializeField] private Transform bulletSpawnPoint;      // 子弹生成位置（不填则用玩家位置）
        [SerializeField] private TextMeshProUGUI noAmmoText;      // 没子弹提示文本
        [SerializeField] private PlayerFacing playerFacing;       // 玩家朝向（用于射击角度限制）
        [SerializeField] private PlayerMovement playerMovement;   // 玩家移动（用于检测奔跑）

        [Header("设置")]
        [SerializeField] private float bulletSpeed = 15f;         // 子弹飞行速度
        [SerializeField] private float noAmmoShowDuration = 1.5f; // 没子弹提示显示时长

        private Coroutine noAmmoCoroutine;

        public override bool IsRanged => true;

        /// <summary>执行射击</summary>
        public override void Attack(Vector2 playerPosition, Vector2 direction, Vector2 mousePosition)
        {
            WeaponData currentWeapon = SwitchWeapon.Instance?.GetCurrentWeaponData();
            if (currentWeapon == null || bulletPrefab == null) return;

            var (bullet, slot) = FindBulletInSlots();
            if (bullet == null)
            {
                ShowNoAmmoText();
                return;
            }

            // 根据子弹ID设置颜色（图片用预制体默认的）
            Color bulletColor = GetBulletColor(bullet.id);

            // 总伤害 = 枪伤害 + 子弹额外伤害
            int totalDamage = Mathf.CeilToInt(currentWeapon.damage + bullet.bonusDamage);

            // 发射点：直接用玩家位置
            Vector2 spawnPos = playerPosition;

            // 射击方向：以发射点为原点，朝向鼠标位置
            Vector2 shootDir = (mousePosition - spawnPos).normalized;

            // 射击角度限制：钳制到玩家朝向的±attackAngle/2范围内
            if (playerFacing != null && currentWeapon.attackAngle < 360f)
            {
                Vector2 facingDir = playerFacing.GetFacingVector();
                float halfAngle = currentWeapon.attackAngle / 2f;
                float angleToTarget = Vector2.Angle(facingDir, shootDir);

                if (angleToTarget > halfAngle)
                {
                    // 计算钳制后的方向：旋转到扇形边界
                    float signedAngle = Vector2.SignedAngle(facingDir, shootDir);
                    float clampedAngle = Mathf.Clamp(signedAngle, -halfAngle, halfAngle);
                    shootDir = Quaternion.Euler(0, 0, clampedAngle) * facingDir;
                }
            }

            // 子弹散布：奔跑时散布扩大，否则正常散布
            float actualSpread = currentWeapon.bulletSpread;
            if (playerMovement != null && playerMovement.IsRunning())
            {
                actualSpread *= currentWeapon.runSpreadMultiplier;
            }
            // 随机偏移：在±actualSpread弧度范围内随机旋转
            float randomOffset = Random.Range(-actualSpread, actualSpread);
            shootDir = Quaternion.Euler(0, 0, randomOffset * Mathf.Rad2Deg) * shootDir;

            // 生成子弹
            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(shootDir, totalDamage, currentWeapon.range, bulletSpeed,
                    null, bulletColor, trailTime: 0.15f, trailStartWidth: 0.08f);
            }

            // 攻击音效
            MessageBus.Publish(new PlaySoundMessage { Type = SoundType.PlayerAttack });
        }

        /// <summary>从Pocket和Pouch槽中找子弹（优先Pocket）</summary>
        private (BulletData data, FixedSlotType slot) FindBulletInSlots()
        {
            if (ItemDirector.Instance == null) return (null, FixedSlotType.Pocket);

            // 先查Pocket
            var pocketItem = ItemDirector.Instance.GetFixedSlot(FixedSlotType.Pocket);
            if (pocketItem.type == FixedItemType.Bullet)
            {
                return (BulletConfig.GetBullet(pocketItem.id), FixedSlotType.Pocket);
            }

            // 再查Pouch
            var pouchItem = ItemDirector.Instance.GetFixedSlot(FixedSlotType.Pouch);
            if (pouchItem.type == FixedItemType.Bullet)
            {
                return (BulletConfig.GetBullet(pouchItem.id), FixedSlotType.Pouch);
            }

            return (null, FixedSlotType.Pocket);
        }

        /// <summary>根据子弹ID返回颜色</summary>
        private Color GetBulletColor(string bulletID)
        {
            switch (bulletID)
            {
                case "normal_bullet":
                    return Color.white;      // 普通子弹：白色（不改变）
                case "steel_bullet":
                    return new Color(0.3f, 0.6f, 1f);  // 钢制子弹：蓝色
                case "titanium_bullet":
                    return new Color(1f, 0.3f, 0.3f);  // 钛钢子弹：红色
                default:
                    return Color.white;
            }
        }

        /// <summary>显示没子弹提示</summary>
        private void ShowNoAmmoText()
        {
            if (noAmmoText == null) return;

            noAmmoText.gameObject.SetActive(true);
            noAmmoText.text = "No Ammos";

            if (noAmmoCoroutine != null) StopCoroutine(noAmmoCoroutine);
            noAmmoCoroutine = StartCoroutine(HideNoAmmoText());
        }

        /// <summary>隐藏没子弹提示</summary>
        private System.Collections.IEnumerator HideNoAmmoText()
        {
            yield return new WaitForSeconds(noAmmoShowDuration);
            if (noAmmoText != null)
            {
                noAmmoText.gameObject.SetActive(false);
            }
        }
    }
}
