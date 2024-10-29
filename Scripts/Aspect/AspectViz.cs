using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class AspectViz : MonoBehaviour, IPointerClickHandler
    {
        [Header("Aspect")]
        public Fragment aspect;

        [Header("Layout")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI countText;

        [SerializeField, HideInInspector] private int _count;


        public int count
        {
            get => _count;
            set => SetCount(value);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            UIManager.Instance?.aspectInfo?.LoadAspect(aspect);
        }

        public void LoadAspect(CardViz cardViz)
        {
            if (cardViz == null)
                return;

            this.aspect = cardViz.card;
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

        public void LoadAspect(HeldFragment frag)
        {
            if (frag != null)
            {
                if (frag.cardViz != null)
                {
                    LoadAspect(frag.cardViz);
                }
                else if (frag.fragment != null && frag.fragment is Aspect)
                {
                    LoadAspect((Aspect)frag.fragment);
                    SetCount(frag.count);
                }
            }
        }

        private void SetCount(int count)
        {
            _count = count;
            countText.text = count.ToString();
        }
    }
}
