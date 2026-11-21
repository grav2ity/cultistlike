using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class FragmentViz : MonoBehaviour, IPointerClickHandler, IScrollHandler
    {
        [Header("Aspect")]
        [SerializeField] private Fragment fragment;

        [Header("Layout")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private float width;

        private CardViz cardViz;
        private int _count;


        public int count
        {
            get => _count;
            set => SetCount(value);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (cardViz == null)
            {
                UIManager.Instance.aspectInfo?.LoadAspect(fragment);
            }
            else
            {
                UIManager.Instance.cardInfo?.LoadCard(cardViz);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
        #if UNITY_EDITOR
            float scroll = eventData.scrollDelta.y;
            var cardInfo = GetComponentInParent<CardInfo>();
            var actWindow = GetComponentInParent<ActWindow>();
            if (cardInfo != null)
            {
                //this needs to be before
                cardInfo.lastChange = fragment;

                if (scroll > 0)
                    cardInfo.cardViz.fragTree.Add(fragment);
                else if (scroll < 0)
                    cardInfo.cardViz.fragTree.Remove(fragment);

            }
            else if (actWindow != null)
            {
                if (scroll > 0)
                    actWindow.AddFragment(fragment);
                else if (scroll < 0)
                    actWindow.RemoveFragment(fragment);
            }
        #endif
        }

        public void Load<T>(T t) where T : IFrag
        {
            if (t != null)
            {
                cardViz = null;

                fragment = t.ToFragment();
                if (fragment.art != null)
                {
                    art.sprite = fragment.art;
                    art.color = Color.white;
                }
                else
                {
                    art.sprite = null;
                    art.color = fragment.color;
                }

                SetCount(t.Count());
            }
        }

        public void Load(CardViz cardViz)
        {
            if (cardViz != null)
            {
                Load(cardViz.card);
                this.cardViz = cardViz;

                SetCount(1);
            }
        }

        private void Awake()
        {
            SetCount(count);
        }

        private void AdjustSize()
        {
            //TODO
            if (count == 1)
            {
                var rt = gameObject.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(width / 2, rt.sizeDelta.y);

                rt = countText.gameObject.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);

                countText.gameObject.SetActive(false);
            }
            else
            {
                var rt = gameObject.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);

                rt = countText.gameObject.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(width / 2, rt.sizeDelta.y);

                countText.gameObject.SetActive(true);
            }
        }

        private void SetCount(int count)
        {
            _count = count;
            countText.text = count.ToString();
            AdjustSize();
        }
    }
}
