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
        [SerializeField] private TextMeshPro title;
        [SerializeField] private Renderer art;
        [SerializeField] private Renderer highlight;

        [Header("Options")]
        [Tooltip("Close when card is removed")]
        public bool autoClose;
        [Tooltip("Accept only matching cards")]
        public bool onlyMatching;
        [Tooltip("Cannot remove card from the slot")]
        public bool cardLock;

        public bool grab;

        [SerializeField, HideInInspector] private FragContainer fragments;

        [SerializeField, HideInInspector] private ActWindow actWindow;
        [SerializeField, HideInInspector] private CardViz _slottedCard;

        public string Title { get => title.text; set => title.text = value; }
        //TODO 
        public CardViz slottedCard
        {
            get => _slottedCard;
            set
            {
                if (_slottedCard != null)
                {
                    actWindow.UnholdCard(_slottedCard);
                    fragments.Remove(_slottedCard);
                }
                _slottedCard = value;
                if (_slottedCard != null)
                {
                    actWindow.HoldCard(slottedCard);
                    fragments.Add(slottedCard);
                    actWindow.Check();
                }
            }
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

        public void SlotCard(CardViz cardViz, bool onlyFinalize = false)
        {
            if (cardViz != null)
            {
                if (onlyFinalize == false)
                {
                    slottedCard = cardViz;
                }

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

        public CardViz UnslotCard()
        {
            var sc = slottedCard;
            if (slottedCard != null)
            {
                slottedCard.free = true;
                slottedCard.transform.SetParent(null);
                slottedCard = null;
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
                //TODO
                _slottedCard = null;
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
        //     open = false;
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
            slottedCard = null;
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
                Title = slot.title;
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
