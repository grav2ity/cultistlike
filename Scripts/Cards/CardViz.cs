using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


namespace CultistLike
{
    public class CardViz : Viz, IDropHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("Card")]
        public Card card;

        [Header("Layout")]
        [SerializeField] private TextMeshPro title;
        [SerializeField] private Renderer artBack;
        [SerializeField] private SpriteRenderer art;
        [SerializeField] private Renderer highlight;

        [Header("Table")]
        [Tooltip("Size on the table for an Array based table; final size is (1,1) + 2*(x,y)")]
        [SerializeField] private Vector2Int CellCount;


        public override Vector2Int GetCellSize() => CellCount;


        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            Drag drag = eventData.pointerDrag.GetComponent<Drag>();
            if (drag == null || drag.draggable == false)
            {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                foreach(var actViz in GameManager.Instance.acts)
                {
                    if (actViz.actWindow.HighlightSlots(card, false) == true)
                    {
                        actViz.SetHighlight(true);
                    }
                }
                GameManager.Instance.openWindow?.HighlightSlots(card);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            UnhighlightTargets();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Drag drag = eventData.pointerDrag?.GetComponent<Drag>();
                if (drag == null || drag.isDragging == false)
                {
                    return;
                }

                //handles dropping card on a slotted card
                var droppedCard = eventData.pointerDrag.GetComponent<CardViz>();
                if (droppedCard != null)
                {
                    var slot = GetComponentInParent<Slot>();
                    if (slot != null)
                    {
                        slot.UnslotCard();
                        GameManager.Instance.table.ReturnToTable(this);
                        slot.SlotCard(droppedCard);
                        return;
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UIManager.Instance?.cardInfo?.LoadCard(card);
        }

        public void OnPointerEnter(PointerEventData eventData) {}

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void UnhighlightTargets()
        {
            foreach(var act in GameManager.Instance.acts)
            {
                act.SetHighlight(false);
            }

            GameManager.Instance.openWindow?.HighlightSlots(null);
        }

        public void LoadCard(Card card)
        {
            if (card == null) return;

            this.card = card;
            title.text = card.cardName;
            if (card.art != null)
            {
                art.sprite = card.art;
            }
            else
            {
                artBack.material.SetColor("_Color", card.color);
            }
        }

        public void SetCard(Card card)
        {
            this.card = card;
        }

        private void Start()
        {
            GetComponent<Drag>().draggingPlane = GameManager.Instance.cardDragPlane;
            LoadCard(card);

            GameManager.Instance.AddCard(this);
        }
    }
}
