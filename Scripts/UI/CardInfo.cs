using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class CardInfo : MonoBehaviour, IScrollHandler
    {
        [Header("Layout")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private FragmentBar fragmentBar;

        [HideInInspector] public CardViz cardViz;

        [HideInInspector] public Fragment lastChange;


        public string Description {
            get => description.text;
            set => description.text = value;
        }

        public string CardName {
            get => cardName.text;
            set => cardName.text = value;
        }

        public void OnChange()
        {
            fragmentBar.Load(cardViz.fragTree);
        }

        public void LoadCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                this.cardViz = cardViz;
                this.cardViz.fragTree.ChangeEvent += OnChange;
                LoadCard(cardViz.card);

                fragmentBar.Load(cardViz.fragTree);
            }
        }

        public void LoadCard(Card card)
        {
            UIManager.Instance.aspectInfo?.Unload();

            if (card!= null)
            {
                gameObject.SetActive(true);

                CardName = card.label;
                Description = card.description;

                if (card.art != null)
                {
                    art.sprite = card.art;
                    art.color = Color.white;
                }
                else
                {
                    art.sprite = null;
                    art.color = card.color;
                }
                fragmentBar.Unload();
            }
        }

        public void Load(SlotViz slotViz)
        {
            UIManager.Instance.aspectInfo?.Unload();

            if (slotViz != null)
            {
                gameObject.SetActive(true);

                CardName = slotViz.slot.label;
                Description = slotViz.slot.description;

                art.sprite = null;
                art.color = Color.white;

                fragmentBar.Load(slotViz.slot);
            }
        }

        public void Unload()
        {
            CardName = "";
            Description = "";
            art.sprite = null;
            art.color = Color.white;

            if (cardViz != null) cardViz.fragTree.ChangeEvent -= OnChange;
            cardViz = null;

            fragmentBar.Unload();

            gameObject.SetActive(false);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (cardViz != null) cardViz.fragTree.ChangeEvent -= OnChange;
        }

        public void OnScroll(PointerEventData eventData)
        {
        #if UNITY_EDITOR
            float scroll = eventData.scrollDelta.y;
            if (lastChange != null)
            {
                if (scroll > 0)
                    cardViz.fragTree.Add(lastChange);
                else if (scroll < 0)
                    cardViz.fragTree.Remove(lastChange);
            }
        #endif
        }
    }
}
