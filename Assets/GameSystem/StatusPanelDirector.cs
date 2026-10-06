using UnityEngine;
using System.Collections;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// ESC状态面板管理器（Director）。
    /// 点击ESC弹出/隐藏状态面板（黑色背景淡入淡出 + 面板从左滑入/从右滑出）。
    /// 面板分为左右两部分（1:2），左边放玩家Sprite动画。
    /// 面板大小、切割比例、玩家Sprite大小在Inspector可调（调整好后可改为private）。
    /// </summary>
    public class StatusPanelDirector : MonoBehaviour
    {
        [Header("动画参数")]
        [SerializeField] private float slideDuration = 0.3f;        // 面板滑入/滑出时长（秒）

        [Header("引用（可拖可不拖：不拖则自动查找）")]
        [SerializeField] private GameObject panel;                   // 面板物体（默认=本物体）
        [SerializeField] private GameObject blackOverlay;            // 黑色背景（自动查找子物体）
        [SerializeField] private RectTransform panelRect;            // 面板RectTransform（默认=本物体）

        [Header("装备槽（运行时在子物体中按SlotType自动查找）")]
        private EquipmentSlotUI headSlot;           // 头部装备槽（SlotType=Head）
        private EquipmentSlotUI handSlot;           // 手部装备槽（SlotType=Hand）
        private EquipmentSlotUI bodySlot;           // 身体装备槽（SlotType=Body）
        private EquipmentSlotUI backSlot;           // 背部装备槽（SlotType=Back）

        [Header("HUD")]
        [SerializeField] private HUDPanel hudPanel;                  // HUD面板（打开状态栏时显示HP/Gold/PP）

        private bool isPanelOpen = false;
        private Vector2 panelOriginalPos;  // 面板原始位置（中心）
        private Coroutine slideCoroutine;
        private CanvasGroup blackCanvasGroup;  // 黑色背景的CanvasGroup（用于淡入淡出）

        /// <summary>面板是否打开（供外部读取）</summary>
        public bool IsOpen => isPanelOpen;

        private void OnEnable()
        {
            AutoFindPanelRefs();
            FindEquipmentSlots();
        }

        /// <summary>自动查找面板引用（拖了就用拖的，没拖自动找）</summary>
        private void AutoFindPanelRefs()
        {
            if (panel == null) panel = gameObject;
            if (panelRect == null) panelRect = GetComponent<RectTransform>();
            if (blackOverlay == null)
            {
                Transform overlay = transform.Find("BlackOverlay");
                if (overlay == null) overlay = transform.Find("BlackBackground");
                if (overlay == null) overlay = transform.Find("Overlay");
                if (overlay != null) blackOverlay = overlay.gameObject;
            }
        }

        /// <summary>在子物体中按SlotType自动查找四个装备槽</summary>
        private void FindEquipmentSlots()
        {
            EquipmentSlotUI[] slots = GetComponentsInChildren<EquipmentSlotUI>(true);
            foreach (EquipmentSlotUI slot in slots)
            {
                switch (slot.GetSlotType())
                {
                    case EquipmentSlotType.Head: headSlot = slot; break;
                    case EquipmentSlotType.Hand: handSlot = slot; break;
                    case EquipmentSlotType.Body: bodySlot = slot; break;
                    case EquipmentSlotType.Back: backSlot = slot; break;
                }
            }
        }

        private void Start()
        {
            // 记录面板原始位置
            if (panelRect != null)
            {
                panelOriginalPos = panelRect.anchoredPosition;
            }

            // 给黑色背景加CanvasGroup（用于淡入淡出）
            if (blackOverlay != null)
            {
                blackCanvasGroup = blackOverlay.GetComponent<CanvasGroup>();
                if (blackCanvasGroup == null)
                {
                    blackCanvasGroup = blackOverlay.AddComponent<CanvasGroup>();
                }
            }

            // 初始化时隐藏面板
            HidePanelImmediate();
        }

        private void Update()
        {
            // ESC完全由UIPanelStack统一管理，这里不监听ESC
            // UIPanelStack在没有其他面板打开时，会调用TogglePanel()
        }

        /// <summary>切换面板显示/隐藏</summary>
        public void TogglePanel()
        {
            if (isPanelOpen)
                HidePanel();
            else
                ShowPanel();
        }

        /// <summary>显示面板（黑色背景淡入 + 面板从左滑入）</summary>
        public void ShowPanel()
        {
            isPanelOpen = true;
            if (panel != null) panel.SetActive(true);
            if (blackOverlay != null) blackOverlay.SetActive(true);

            // 刷新装备显示
            RefreshEquipmentDisplay();

            // 刷新固定槽显示（因为面板用SetActive隐藏，重新打开时需要手动刷新）
            if (panel != null)
            {
                FixedSlotUI[] fixedSlots = panel.GetComponentsInChildren<FixedSlotUI>(true);
                foreach (var slot in fixedSlots)
                {
                    slot.RefreshDisplay();
                }
            }

            // 刷新背包显示（根据装备的背包显示对应数量的格子）
            RefreshInventoryDisplay();

            // 通知HUD显示HP/Gold/PP
            if (hudPanel != null) hudPanel.OnStatusPanelOpen();

            // 停止之前的动画
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);

            // 播放滑入动画
            slideCoroutine = StartCoroutine(SlideInAnimation());
        }

        /// <summary>从ItemDirector读取已穿戴装备，刷新到面板的装备槽上</summary>
        private void RefreshEquipmentDisplay()
        {
            if (ItemDirector.Instance == null) return;

            // 刷新每个槽
            RefreshSlot(headSlot, EquipmentSlotType.Head);
            RefreshSlot(handSlot, EquipmentSlotType.Hand);
            RefreshSlot(bodySlot, EquipmentSlotType.Body);
            RefreshSlot(backSlot, EquipmentSlotType.Back);
        }

        /// <summary>刷新单个装备槽</summary>
        private void RefreshSlot(EquipmentSlotUI slot, EquipmentSlotType slotType)
        {
            // Debug.Log($"[StatusPanel刷新]功能正常 slot={(slot == null ? "NULL" : slot.name)} id={ItemDirector.Instance.GetEquipped(slotType) ?? "空"}");
            if (slot == null) return;

            string equippedID = ItemDirector.Instance.GetEquipped(slotType);
            // Debug.Log($"[UI通路] 环4 type={slotType} 读到ID={equippedID ?? "空"}");
            if (!string.IsNullOrEmpty(equippedID))
            {
                EquipmentData data = EquipmentConfig.GetEquipment(equippedID);
                if (data != null)
                {
                    slot.EquipItem(data);
                    // Get icon from cache (set by shop items)
                    // Debug.Log($"[UI通路] 环7 进入Cache: id={equippedID}, HasIcon={EquipmentIconCache.HasIcon(equippedID)}");
                    Sprite icon = EquipmentIconCache.GetIcon(equippedID);
                    // Debug.Log(icon != null ? "[Cache读取]功能正常" : "[Cache读取]失败：Cache无此装备图标");
                    // Debug.Log($"[UI通路] 环7 取出icon={(icon == null ? "NULL" : "有")}");
                    if (icon != null)
                    {
                        slot.SetIcon(icon, EquipmentIconCache.GetIconColor(equippedID));
                    }
                }
            }
            else
            {
                slot.UnequipItem();
            }
        }

        /// <summary>刷新背包显示：根据装备的背包显示对应数量的格子</summary>
        private void RefreshInventoryDisplay()
        {
            // 旧的格子淡入淡出已废弃
        }

        /// <summary>隐藏面板（面板从右滑出 + 黑色背景淡出）</summary>
        public void HidePanel()
        {
            isPanelOpen = false;

            // 通知HUD隐藏HP/Gold/PP
            if (hudPanel != null) hudPanel.OnStatusPanelClose();

            // 停止之前的动画
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);

            // 播放滑出动画
            slideCoroutine = StartCoroutine(SlideOutAnimation());
        }

        /// <summary>瞬间隐藏面板（初始化用，无动画）</summary>
        private void HidePanelImmediate()
        {
            isPanelOpen = false;
            if (panel != null) panel.SetActive(false);
            if (blackOverlay != null) blackOverlay.SetActive(false);

            // 黑色背景瞬间透明
            if (blackCanvasGroup != null)
            {
                blackCanvasGroup.alpha = 0f;
            }
        }

        /// <summary>滑入动画：面板从屏幕左边滑到中心 + 黑色背景淡入（easeOutExpo，先快后慢）</summary>
        private IEnumerator SlideInAnimation()
        {
            if (panelRect == null) yield break;

            // 起始位置：屏幕左边外面
            float screenWidth = Screen.width;
            float panelWidth = panelRect.rect.width;
            Vector2 startPos = panelOriginalPos + new Vector2(-screenWidth - panelWidth, 0f);
            Vector2 endPos = panelOriginalPos;

            // 面板起始位置
            panelRect.anchoredPosition = startPos;

            // 黑色背景起始透明
            if (blackCanvasGroup != null) blackCanvasGroup.alpha = 0f;

            // 滑入 + 淡入
            float t = 0f;
            while (t < slideDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / slideDuration);
                float eased = 1f - Mathf.Pow(2f, -10f * p);  // easeOutExpo（先快后慢）

                // 面板位置
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);

                // 黑色背景淡入
                if (blackCanvasGroup != null)
                {
                    blackCanvasGroup.alpha = Mathf.Lerp(0f, 1f, p);
                }

                yield return null;
            }

            // 确保最终状态
            panelRect.anchoredPosition = endPos;
            if (blackCanvasGroup != null) blackCanvasGroup.alpha = 1f;
        }

        /// <summary>滑出动画：面板从中心滑到屏幕右边 + 黑色背景淡出（easeInExpo，先慢后快）</summary>
        private IEnumerator SlideOutAnimation()
        {
            if (panelRect == null) yield break;

            // 起始位置：中心
            Vector2 startPos = panelOriginalPos;
            // 结束位置：屏幕右边外面
            float screenWidth = Screen.width;
            float panelWidth = panelRect.rect.width;
            Vector2 endPos = panelOriginalPos + new Vector2(screenWidth + panelWidth, 0f);

            // 黑色背景起始不透明
            if (blackCanvasGroup != null) blackCanvasGroup.alpha = 1f;

            // 滑出 + 淡出
            float t = 0f;
            while (t < slideDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / slideDuration);
                float eased = Mathf.Pow(2f, 10f * (p - 1f));  // easeInExpo（先慢后快）

                // 面板位置
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);

                // 黑色背景淡出
                if (blackCanvasGroup != null)
                {
                    blackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, p);
                }

                yield return null;
            }

            // 确保最终状态
            panelRect.anchoredPosition = endPos;
            if (blackCanvasGroup != null) blackCanvasGroup.alpha = 0f;

            // 动画结束后隐藏面板
            if (panel != null) panel.SetActive(false);
            if (blackOverlay != null) blackOverlay.SetActive(false);
        }
    }
}


