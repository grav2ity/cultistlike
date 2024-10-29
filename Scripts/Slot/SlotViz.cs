using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


namespace CultistLike
{
    public class SlotViz : MonoBehaviour, IDropHandler, ICardDock, IPointerClickHandler
    {
        public Slot slot;

        [Header("Layout")]
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Renderer art;
        [SerializeField] private Renderer highlight;

        [Header("Card Options")]
        [Tooltip("Accept only matching Cards.")]
        public bool onlyMatching;
        [Tooltip("Cannot remove Card from the Slot.")]
        public bool cardLock;
        [Tooltip("Grabs Cards for itself.")]
        public bool grab;

        [Header("Options")]
        [Tooltip("Close when Card is removed.")]
        public bool autoClose;
        [Tooltip("Removing Card will cause all other Slots to close.")]
        public bool firstSlot;

        [SerializeField, HideInInspector] private ActWindow actWindow;
        [SerializeField, HideInInspector] private CardViz _slottedCard;

        public string Label { get => label.text; set => label.text = value; }
        public CardViz slottedCard { get => _slottedCard; set => _slottedCard = value; }


        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Drag drag = eventData.pointerDrag?.GetComponent<Drag>();
                if (drag == null || drag.isDragging == false)
                {
                    return;
                }

                OnCardDock(eventData.pointerDrag);
            }
        }

        public void OnCardDock(GameObject go)
        {
            CardViz cardViz = go.GetComponent<CardViz>();
            if (gameObject.activeInHierarchy == false || TrySlotCard(cardViz) == false)
            {
                GameManager.Instance.table.ReturnToTable(cardViz);
            }
        }

        public void OnCardUndock(GameObject go)
        {
            UnslotCard();
            actWindow.Check();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            List<CardViz> cardsToH = new List<CardViz>();
            foreach (var cardViz in GameManager.Instance.table.GetCards())
            {
                if (AcceptsCard(cardViz) == true)
                {
                    cardsToH.Add(cardViz);
                }
            }
            GameManager.Instance.table.HighlightCards(cardsToH);

            UIManager.Instance?.cardInfo?.Load(this);
        }

        public bool TrySlotCard(CardViz cardViz)
        {
            if (gameObject.activeSelf == true && cardViz != null)
            {
                if (onlyMatching != true || AcceptsCard(cardViz) != false)
                {
                    if (slottedCard == null)
                    {
                        SlotCard(cardViz);
                        return true;
                    }
                    else if (cardLock == false)
                    {
                        var sc = UnslotCard();
                        GameManager.Instance.table.ReturnToTable(sc);
                        SlotCard(cardViz);
                        return true;
                    }
                }
            }
            return false;
        }

        public void SlotCard(CardViz cardViz)
        {
            SlotCardLogical(cardViz);
            SlotCardPhysical(cardViz);
        }

        public void SlotCardPhysical(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cardViz.transform.SetParent(transform);
                cardViz.transform.localPosition = Vector3.zero;

                cardViz.UnhighlightTargets();

                if (cardLock == true)
                {
                    cardViz.interactive = false;
                    cardViz.free = false;
                }
            }
        }

        public void SlotCardLogical(CardViz cardViz)
        {
            if (slottedCard != null)
            {
                actWindow.UnholdCard(slottedCard);
            }
            else
            {
                actWindow.UnholdCard(slottedCard);
            }
            slottedCard = cardViz;
            if (slottedCard != null)
            {
                actWindow.HoldCard(slottedCard);
                actWindow.Check();
            }
        }

        public CardViz UnslotCard()
        {
            var sc = slottedCard;
            if (slottedCard != null)
            {
                slottedCard.free = true;
                slottedCard.interactive = true;
                slottedCard.transform.SetParent(null);
                SlotCardLogical(null);
            }
            if (firstSlot == true)
            {
                actWindow.FirstSlotEmpty();
            }

            // if (autoClose == true)
            // {
            //     CloseSlot();
            // }

            return sc;
        }

        public void ParentCardToWindow()
        {
            if (slottedCard != null)
            {
                slottedCard.gameObject.SetActive(false);
                slottedCard.transform.SetParent(actWindow.transform);
                slottedCard = null;
            }
            // if (autoClose == true)
            // {
            //     CloseSlot();
            // }
        }

        // public void CloseSlot()
        // {
        //     if (slottedCard != null)
        //     {
        //         var cardViz = slottedCard;
        //         UnslotCard();
        //         GameManager.Instance.table.ReturnToTable(cardViz);
        //     }
        //     gameObject.SetActive(false);
        //     // open = false;
        // }

        // public void OpenSlot(string name = "")
        // {
        //     gameObject.SetActive(true);
        //     open = true;
        // }

        public bool AcceptsCard(CardViz cardViz)
        {
            if (slot == null || cardViz == null)
            {
                return false;
            }
            else
            {
                return slot.AcceptsCard(cardViz);
            }
        }

        public void DestroyCard()
        {
            if (slottedCard != null)
            {
                GameManager.Instance.DestroyCard(slottedCard);
            }
            // slottedCard = null;
            SlotCardLogical(null);
        }

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void LoadSlot(Slot slot)
        {
            if (slot != null)
            {
                this.slot = slot;
                Label = slot.label;
                grab = slot.grab;
                cardLock = slot.cardLock;
                onlyMatching = slot.onlyMatching;
            }
        }

        private void Awake()
        {
            actWindow = GetComponentInParent<ActWindow>();
        }
    }
}
