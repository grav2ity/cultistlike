using System;
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
        [SerializeField] private GameObject visualsGO;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Renderer art;
        [SerializeField] private Renderer highlight;

        [Header("Card Options")]
        [Tooltip("Grabs Cards for itself.")]
        public bool grab;
        [Tooltip("Cannot remove Card from the Slot.")]
        public bool cardLock;

        [Header("Options")]
        [Tooltip("Removing Card will cause all other Slots to close.")]
        public bool firstSlot;


        [SerializeField, HideInInspector] private ActWindow actWindow;
        [SerializeField, HideInInspector] private CardViz _slottedCard;

        public bool open { get => gameObject.activeSelf; }
        public CardViz slottedCard { get => _slottedCard; private set => _slottedCard = value; }


        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Drag drag = eventData.pointerDrag?.GetComponent<Drag>();
                if (drag != null && drag.isDragging == true)
                {
                    OnCardDock(eventData.pointerDrag);
                }
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
                if (AcceptsCard(cardViz) == true)
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
            //handles dropping whole stack on a slot
            var cardVizY = cardViz.Yield();

            SlotCardLogical(cardVizY);
            SlotCardPhysical(cardVizY);

            if (cardViz != cardVizY)
            {
                GameManager.Instance.table.ReturnToTable(cardViz);
            }
        }

        public void SlotCardPhysical(CardViz cardViz)
        {
            if (cardViz != null && open == true)
            {
                cardViz.transform.SetParent(transform);
                cardViz.transform.localPosition = Vector3.zero;

                if (visualsGO.activeInHierarchy == false)
                {
                    cardViz.Hide();
                }

                if (cardViz.isDragging == true)
                {
                    cardViz.UnhighlightTargets();
                }

                cardViz.CastShadow(false);
            }
        }

        public void SlotCardLogical(CardViz cardViz)
        {
            if (cardViz != null && slot != null)
            {
                slottedCard = cardViz;

                foreach (var frag in slot.fragments)
                {
                    actWindow.AddFragment(frag);
                }

                slottedCard.Parent(transform);

                if (cardLock == true)
                {
                    cardViz.interactive = false;
                    cardViz.free = false;
                }

                // if (firstSlot == true)
                // {
                //     actWindow.SetFragMemory(cardViz);
                // }
            }
        }

        public CardViz UnslotCard()
        {
            Debug.Assert(slot != null);

            if (slottedCard != null)
            {
                slottedCard.free = true;
                slottedCard.interactive = true;
                slottedCard.Parent(null);

                slottedCard.CastShadow(true);

                foreach (var frag in slot.fragments)
                {
                    actWindow.RemoveFragment(frag);
                }

                var sc = slottedCard;
                slottedCard = null;

                if (firstSlot == true)
                {
                    actWindow.FirstSlotEmpty();
                }


                return sc;
            }
            else
            {
                return null;
            }
        }

        public void ParentCardToWindow()
        {
            if (slottedCard != null)
            {
                var cardViz = slottedCard;
                slottedCard = null;
                cardViz.ParentTo(actWindow.transform, true);
            }
        }

        public void CloseSlot()
        {
            gameObject.SetActive(false);
            slot = null;
            SetHighlight(false);
            if (grab == true)
            {
                GameManager.Instance.onCardInPlay.RemoveListener(GrabAction);
            }
        }

        public void OpenSlot()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Refresh()
        {
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
            else if (slot.actGrab == true && slottedCard == null)
            {
                ActGrab();
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
                label.text = slot.label;
                grab = slot.grab;
                cardLock = slot.cardLock;
            }
        }

        public void ActGrab()
        {
            foreach (var cardViz in actWindow.cards)
            {
                if (AcceptsCard(cardViz) == true)
                {
                    cardViz.free = true;
                    cardViz.interactive = true;
                    cardViz.Show();
                    SlotCard(cardViz);
                    return;
                }
            }
        }

        public bool Grab(CardViz cardViz, bool bringUp = false)
        {
            if (cardViz != null && cardViz.free == true && AcceptsCard(cardViz) == true)
            {
                Vector3 target;
                if (actWindow.open == true)
                {
                    target = transform.position;
                }
                else
                {
                    target =  actWindow.tokenViz.targetPosition;
                }

                Action<CardViz> onStart = x => SlotCardLogical(x);
                Action<CardViz> onComplete = x =>
                {
                    SlotCardPhysical(x);
                    if (bringUp == true)
                    {
                        actWindow.BringUp();
                    }
                };

                return cardViz.Grab(target, onStart, onComplete);
            }
            else
            {
                return false;
            }
        }

        public void Hide()
        {
            visualsGO.SetActive(false);
            slottedCard?.Hide();
        }

        public void Show()
        {
            visualsGO.SetActive(true);
            slottedCard?.Show();
        }

        public SlotVizSave Save()
        {
            var save = new SlotVizSave();
            save.slot = slot;
            save.cardID = slottedCard != null ? slottedCard.GetInstanceID() : 0;
            return save;
        }

        public void Load(SlotVizSave save)
        {
            LoadSlot(save.slot);

            var cardViz = SaveManager.Instance.CardFromID(save.cardID);
            if (cardViz != null)
            {
                slottedCard = cardViz;
                SlotCardPhysical(cardViz);
            }
        }

        private void Awake()
        {
            actWindow = GetComponentInParent<ActWindow>();
        }
    }

    [Serializable]
    public class SlotVizSave
    {
        public Slot slot;
        public int cardID;
    }

}
