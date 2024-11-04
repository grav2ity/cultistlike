using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;
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

        public string Label { get => label.text; private set => label.text = value; }
        public CardViz slottedCard { get => _slottedCard; private set => _slottedCard = value; }


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

                if (cardViz.isDragging == true)
                {
                    cardViz.UnhighlightTargets();
                }

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
                actWindow.UnholdCard(slottedCard, slot);
            }
            slottedCard = cardViz;
            if (slottedCard != null)
            {
                actWindow.HoldCard(slottedCard, slot);
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

        public void CloseSlot()
        {
            gameObject.SetActive(false);
            if (grab == true)
            {
                GameManager.Instance.onCardInPlay.RemoveListener(GrabAction);
            }
            // if (slottedCard != null)
            // {
            //     var cardViz = slottedCard;
            //     UnslotCard();
            //     GameManager.Instance.table.ReturnToTable(cardViz);
            // }
        }

        public void OpenSlot()
        {
            gameObject.SetActive(true);
            if (grab == true && slottedCard == null)
            {
                foreach (var cardViz in GameManager.Instance.cards)
                {
                    if (Grab(cardViz) == true)
                    {
                        return;
                    }
                }
                GameManager.Instance.onCardInPlay.AddListener(GrabAction);
            }
        }

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
            SlotCardLogical(null);
        }

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void GrabAction(CardViz cardViz)
        {
            if (Grab(cardViz) == true)
            {
                GameManager.Instance.onCardInPlay.RemoveListener(GrabAction);
            }
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

        public bool Grab(CardViz cardViz, bool bringUp = false)
        {
            if (cardViz.gameObject.activeSelf == true && AcceptsCard(cardViz) == true)
            {
                var cardVizY = cardViz.Yield();

                if (cardVizY.free == true)
                {
                    cardVizY.free = false;
                    cardVizY.transform.DOComplete(true);
                    bool prevInteractive = cardVizY.interactive;
                    cardVizY.interactive = false;
                    cardVizY.transform.parent?.GetComponentInParent<ICardDock>(true)?.
                        OnCardUndock(cardVizY.gameObject);
                    cardVizY.gameObject.SetActive(true);
                    cardVizY.transform.SetParent(null);
                    SlotCardLogical(cardVizY);

                    Vector3 target;
                    if (gameObject.activeInHierarchy == true)
                    {
                        target = transform.position;
                    }
                    else
                    {
                        target =  actWindow.tokenViz.transform.position;
                    }

                    var pos = cardVizY.transform.position;
                    cardVizY.transform.position = new Vector3(pos.x, pos.y, GameManager.Instance.cardDragPlane.position.z);

                    cardVizY.transform.DOMove(target, GameManager.Instance.normalSpeed).
                        OnComplete(() =>
                        {
                            cardVizY.interactive = prevInteractive;
                            SlotCardPhysical(cardVizY);
                            if (bringUp == true)
                            {
                                actWindow.BringUp();
                            }
                        });

                    return true;
                }
            }
            return false;
        }

        private void Awake()
        {
            actWindow = GetComponentInParent<ActWindow>();
        }
    }
}
