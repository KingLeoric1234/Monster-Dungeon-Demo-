using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// 自动设置右边任务详情排版：Content加LayoutGroup+Fitter，每个TMP加Fitter+自动换行。
    /// 挂在RightPanel的DetailContent上，Start自动执行。
    /// </summary>
    public class QuestLogAutoLayout : MonoBehaviour
    {
        [SerializeField] private float spacing = 20f;

        private void Awake()
        {
            StartCoroutine(DelayedLayout());
        }

        private IEnumerator DelayedLayout()
        {
            yield return new WaitForSecondsRealtime(0.02f);

            var vlg = GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            foreach (TMP_Text tmp in GetComponentsInChildren<TMP_Text>())
            {
                var cf = tmp.GetComponent<ContentSizeFitter>();
                if (cf == null) cf = tmp.gameObject.AddComponent<ContentSizeFitter>();
                cf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                cf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Overflow;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        /// <summary>文字变化后重新排版（点击任务切换详情时调）</summary>
        public void RefreshLayout()
        {
            StopAllCoroutines();
            StartCoroutine(DelayedRefresh());
        }

        private IEnumerator DelayedRefresh()
        {
            yield return new WaitForSecondsRealtime(0.02f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
