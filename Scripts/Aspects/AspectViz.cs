using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

namespace CultistLike
{
    public class AspectViz : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Aspect")]
        public Aspect aspect;

        [Header("Layout")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI countText;

        [SerializeField, HideInInspector] private int _count;

        public int count
        {
            get => _count;
            set => SetCount(value);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            UIManager.Instance?.aspectInfo?.LoadAspect(aspect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance?.aspectInfo?.Unload();
        }

        public void LoadAspect(Aspect aspect)
        {
            if (aspect == null)
                return;

            this.aspect = aspect;
            if (aspect.art != null)
            {
                art.sprite = aspect.art;
            }
            else
            {
                art.color = aspect.color;
            }

            SetCount(1);
        }

        public void LoadAspect(AspectViz aspectViz)
        {
            if (aspectViz == null)
                return;

            aspect = aspectViz.aspect;
            if (aspectViz.art != null)
            {
                art.sprite = aspectViz.art.sprite;
            }
            else
            {
                art.color = aspectViz.art.color;
            }

            SetCount(aspectViz.count);
        }

        private void SetCount(int count)
        {
            _count = count;
            countText.text = count.ToString();
        }
    }
}
