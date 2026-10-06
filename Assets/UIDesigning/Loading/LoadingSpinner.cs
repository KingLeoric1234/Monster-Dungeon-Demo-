using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>简单加载转圈：挂在Loading Panel上，自动转Image</summary>
    public class LoadingSpinner : MonoBehaviour
    {
        [SerializeField] private RectTransform spinner;
        [SerializeField] private float speed = 200f;

        private void Update()
        {
            if (spinner != null) spinner.Rotate(0, 0, -speed * Time.deltaTime);
        }
    }
}
