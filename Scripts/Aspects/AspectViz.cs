using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace CultistLike
{
    public class AspectViz : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Aspect aspect;

        [SerializeField] private Image art;

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
        }
    }
}
