using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Tasks
{
    /// <summary>
    /// 自动设置左边任务按钮排版。挂在TaskListContent上。
    /// 每次RefreshList后调RefreshLayout()。
    /// </summary>
    public class TaskListAutoLayout : MonoBehaviour
    {
        [SerializeField] private float buttonHeight = 40f;
        [SerializeField] private float spacing = 10f;

        private void Awake()
        {
            StartCoroutine(DelayedAwake());
        }

        private IEnumerator DelayedAwake()
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
        }

        public void RefreshLayout()
        {
            StartCoroutine(DelayedRefresh());
        }

        private IEnumerator DelayedRefresh()
        {
            yield return new WaitForSecondsRealtime(0.02f);

            foreach (RectTransform child in transform)
            {
                var childFitter = child.GetComponent<ContentSizeFitter>();
                if (childFitter != null) Destroy(childFitter);

                var layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement == null) layoutElement = child.gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = buttonHeight;
                layoutElement.flexibleHeight = 0;

                var txt = child.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.enableWordWrapping = true;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
