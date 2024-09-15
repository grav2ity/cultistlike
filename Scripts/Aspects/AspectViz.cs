using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace CultistLike
{
    public class AspectViz : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public Aspect aspect;

        [SerializeField] private Image art;

        //TODO
        public void OnPointerClick(PointerEventData eventData) {}

        //TODO
        public void OnPointerEnter(PointerEventData eventData) {}

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
