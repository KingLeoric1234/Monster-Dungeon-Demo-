using UnityEngine;
using Game.Core;
using Game.Items;
using Game.Player;
using Game.UI;
using TMPro;

namespace Game.Weapons
{
    /// <summary>
    /// 狙击枪武器：高伤害、穿透、无距离限制、开镜指哪打哪。
    /// 挂在 SwitchWeapon 的子物体上（如 SwitchWeapon/BurstSniper），由 SwitchWeapon 设为当前手持武器。
    /// </summary>
    public class SniperWeapon : IWeapon
    {
        [Header("引用")]
        [SerializeField] private GameObject bulletPrefab;         // 子弹预制体
        [SerializeField] private Transform bulletSpawnPoint;      // 子弹生成位置（不填则用玩家位置）
        [SerializeField] private TextMeshProUGUI noAmmoText;      // 没子弹提示文本
        [SerializeField] private PlayerFacing playerFacing;       // 玩家朝向
        [SerializeField] private PlayerMovement playerMovement;   // 玩家移动（用于检测奔跑）

        [Header("设置")]
        [SerializeField] private float bulletSpeed = 25f;         // 子弹飞行速度（比手枪快）
        [SerializeField] private float noAmmoShowDuration = 1.5f; // 没子弹提示显示时长

        private Coroutine noAmmoCoroutine;

        public override bool IsRanged => true;

        /// <summary>执行狙击射击</summary>
        public override void Attack(Vector2 playerPosition, Vector2 direction, Vector2 mousePosition)
        {
            WeaponData currentWeapon = SwitchWeapon.Instance?.GetCurrentWeaponData();
            if (currentWeapon == null || bulletPrefab == null) return;

            // 检测子弹：优先Pocket，然后Pouch
            var (bullet, slot) = FindBulletInSlots();
            if (bullet == null)
            {
                ShowNoAmmoText();
                return;
            }

            // 根据子弹ID设置颜色
            Color bulletColor = GetBulletColor(bullet.id);

            // 总伤害 = 枪伤害 + 子弹额外伤害
            int totalDamage = Mathf.CeilToInt(currentWeapon.damage + bullet.bonusDamage);

            // 发射点
            Vector2 spawnPos = bulletSpawnPoint != null ? (Vector2)bulletSpawnPoint.position : playerPosition;

            // 射击方向：朝向鼠标
            Vector2 shootDir = (mousePosition - spawnPos).normalized;

            // 射击角度限制
            if (playerFacing != null && currentWeapon.attackAngle < 360f)
            {
                Vector2 facingDir = playerFacing.GetFacingVector();
                float halfAngle = currentWeapon.attackAngle / 2f;
                float angleToTarget = Vector2.Angle(facingDir, shootDir);

                if (angleToTarget > halfAngle)
                {
                    float signedAngle = Vector2.SignedAngle(facingDir, shootDir);
                    float clampedAngle = Mathf.Clamp(signedAngle, -halfAngle, halfAngle);
                    shootDir = Quaternion.Euler(0, 0, clampedAngle) * facingDir;
                }
            }

            // 子弹散布：开镜时散布=0（指哪打哪），不开镜时正常散布，奔跑时扩大
            float actualSpread = 0f;
            if (!SniperScopeUI.IsScopeOpenStatic)
            {
                actualSpread = currentWeapon.bulletSpread;
                if (playerMovement != null && playerMovement.IsRunning())
                {
                    actualSpread *= currentWeapon.runSpreadMultiplier;
                }
                float randomOffset = Random.Range(-actualSpread, actualSpread);
                shootDir = Quaternion.Euler(0, 0, randomOffset * Mathf.Rad2Deg) * shootDir;
            }

            // 生成子弹：狙击枪无距离限制（maxRange=999），彗尾粗壮有力
            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(shootDir, totalDamage, 999f, bulletSpeed,
                    null, bulletColor, trailTime: 0.3f, trailStartWidth: 0.15f);
            }

            // 攻击音效
            MessageBus.Publish(new PlaySoundMessage { Type = SoundType.PlayerAttack });
        }

        /// <summary>从Pocket和Pouch槽中找子弹（优先Pocket）</summary>
        private (BulletData data, FixedSlotType slot) FindBulletInSlots()
        {
            if (ItemDirector.Instance == null) return (null, FixedSlotType.Pocket);

            var pocketItem = ItemDirector.Instance.GetFixedSlot(FixedSlotType.Pocket);
            if (pocketItem.type == FixedItemType.Bullet)
            {
                return (BulletConfig.GetBullet(pocketItem.id), FixedSlotType.Pocket);
            }

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
                    return Color.white;
                case "steel_bullet":
                    return new Color(0.3f, 0.6f, 1f);
                case "titanium_bullet":
                    return new Color(1f, 0.3f, 0.3f);
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
