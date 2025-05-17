using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;
using TMPro;


namespace CultistLike
{
    public class CardViz : Viz, ICardDock, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card")]
        public Card card;

        [HideInInspector] public CardViz stack;

        [Header("Layout")]
        public FragTree fragTree;
        [SerializeField] private GameObject visualsGO;
        [SerializeField] private TextMeshPro title;
        [SerializeField] private Renderer artBack;
        [SerializeField] private SpriteRenderer art;
        [SerializeField] private Renderer highlight;
        [SerializeField] private CardStack cardStack;
        [SerializeField] private CardDecay cardDecay;

        [Header("Table")]
        [Tooltip("Size on the table for an Array based table; final size is (1,1) + 2*(x,y)")]
        [SerializeField] private Vector2Int CellCount;

        [SerializeField, HideInInspector] private bool _faceDown;


        public override Vector2Int GetCellSize() => CellCount;

        public bool free { get => fragTree.free; set => fragTree.free = value; }
        public bool visible { get => visualsGO.activeInHierarchy; }
        public bool faceDown { get => _faceDown; private set => _faceDown = value; }

        public CardDecay decay { get => cardDecay; }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (interactive == true && eventData.button == PointerEventData.InputButton.Left)
            {
                //if not dragging whole stack yield one card
                if (cardStack.stackDrag == false && cardStack.Count > 1)
                {
                    var cardViz = cardStack.Pop();
                    if (cardViz != null)
                    {
                        eventData.pointerDrag = cardViz.gameObject;
                        cardViz.OnBeginDrag(eventData);
                        return;
                    }
                }

                base.OnBeginDrag(eventData);

                foreach(var tokenViz in GameManager.Instance.tokens)
                {
                    var slotViz = tokenViz.actWindow.AcceptsCard(this);
                    if (slotViz != null && (slotViz.slottedCard == null || slotViz.cardLock == false))
                    {
                        tokenViz.SetHighlight(true);
                    }
                }
                GameManager.Instance.openWindow?.HighlightSlots(this);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            UnhighlightTargets();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (interactive == true && eventData.button == PointerEventData.InputButton.Left)
            {
                var droppedCard = eventData.pointerDrag.GetComponent<CardViz>();
                if (droppedCard != null && droppedCard.isDragging == true)
                {
                    //handles stacking cards
                    //TODO
                    if (GetComponentInParent<ArrayTable>() != null)
                    {
                        if (CanStack(droppedCard))
                        {
                            bool stacked = false;
                            if (droppedCard.cardStack.Count > 1)
                            {
                                stacked = cardStack.Merge(droppedCard.cardStack);
                            }
                            else
                            {
                                stacked = cardStack.Push(droppedCard);
                            }

                            if (stacked == true)
                            {
                                droppedCard.OnEndDrag(eventData);
                            }
                        }
                        return;
                    }

                    //handles dropping card on a slotted card
                    var slot = GetComponentInParent<SlotViz>();
                    if (slot != null)
                    {
                        slot.OnCardDock(droppedCard.gameObject);
                        return;
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactive == true && faceDown == true)
            {
                Reverse();
            }
            else
            {
                UIManager.Instance?.cardInfo?.LoadCard(this);
            }

            if (eventData.clickCount == 2)
            {
                var slot = GetComponentInParent<SlotViz>();
                if (slot != null && slot.cardLock == false)
                {
                    slot.UnslotCard();
                    GameManager.Instance.table.ReturnToTable(this);
                }
                else
                {
                    //TODO does not exactly work with stacks where cards might have different fragments
                    SlotViz readySlot = null;
                    if (GameManager.Instance.openWindow != null)
                    {
                        readySlot = GameManager.Instance.openWindow.AcceptsCard(this, true);
                    }
                    if (readySlot == null)
                    {
                        foreach(var tokenViz in GameManager.Instance.tokens)
                        {
                            readySlot = tokenViz.actWindow.AcceptsCard(this, true);
                            if (readySlot != null)
                            {
                                break;
                            }
                        }
                    }
                    if (readySlot != null)
                    {
                        var cardVizY = Yield();

                        cardVizY.transform.parent?.
                            GetComponentInParent<ICardDock>(true)?.OnCardUndock(cardVizY.gameObject);
                        readySlot.Grab(cardVizY, true);
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (cardDecay.enabled == true)
            {
                cardDecay.ShowTimer();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (cardDecay.enabled == true)
            {
                cardDecay.HideTimer();
            }
        }

        public virtual void OnCardDock(GameObject go)
        {
            var cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                if (cardStack.Push(cardViz) != true)
                {
                    GameManager.Instance.table.ReturnToTable(cardViz);
                }
            }
        }

        public virtual void OnCardUndock(GameObject go) {}

        public void Decay(Card card, float time)
        {
        #if UNITY_EDITOR
            cardDecay.StartTimer(GameManager.Instance.DevTime(time), card);
        #else
            cardDecay.StartTimer(time, card);
        #endif
            if (visible == false && cardDecay.pauseOnHide == true)
            {
                cardDecay.Pause();
            }
        }

        public void Destroy()
        {
            transform.parent?.GetComponentInParent<ICardDock>(true)?.OnCardUndock(gameObject);
            GameManager.Instance.DestroyCard(this);
        }

        public void Reverse(bool instant = false)
        {
            if (instant == true)
            {
                transform.localEulerAngles += new Vector3(0f, 180f, 0f);
            }
            else
            {
                interactive = false;
                transform.DOLocalRotate(transform.localEulerAngles +
                                        new Vector3(0f, 180f, 0f),
                                        GameManager.Instance.rotateSpeed);
                transform.DOPunchPosition(new Vector3(0f, 0f, -1f),
                                          GameManager.Instance.rotateSpeed, 2, 0f).
                    OnComplete(() => { interactive = true; });

            }
            faceDown = !faceDown;
            fragTree.OnChange();
        }

        public void Hide()
        {
            visualsGO.SetActive(false);
            if (cardDecay.pauseOnHide == true)
            {
                cardDecay.Pause();
            }
        }

        public void Show()
        {
            visualsGO.SetActive(true);
            //TODO
            if (cardDecay.pauseOnSlot == false || GetComponentInParent<SlotViz>() == null)
            {
                cardDecay.Unpause();
            }
        }

        public void ShowFace()
        {
            transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            faceDown = false;
            fragTree.OnChange();
        }

        public void ShowBack()
        {
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            faceDown = true;
            fragTree.OnChange();
        }

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void UnhighlightTargets()
        {
            foreach(var token in GameManager.Instance.tokens)
            {
                token.SetHighlight(false);
            }

            GameManager.Instance.openWindow?.UnhighlightSlots();
        }

        public CardViz Yield() => cardStack.Count > 1 ? cardStack.Pop() : this;

        public void LoadCard(Card card, bool loadFragments = true)
        {
            if (card == null) return;

            this.card = card;

            title.text = card.label;
            if (card.art != null)
            {
                art.sprite = card.art;
            }
            else
            {
                artBack.material.SetColor("_Color", card.color);
            }

            if (loadFragments == true)
            {
                LoadFragments();
            }
        }

        public void Transform(Card card)
        {
            LoadCard(card, false);

            fragTree.OnChange();

            cardDecay.StopTimer();
            if (card.lifetime > 0f)
            {
                Decay(card.decayTo, card.lifetime);
            }
        }

        public CardViz Duplicate()
        {
            //TODO clone whole object?
            var newCardViz = GameManager.Instance.CreateCard(card);

            if (newCardViz != null)
            {
                foreach (var frag in fragTree.localFragments)
                {
                    newCardViz.fragTree.Add(frag);
                }
            }

            return newCardViz;
        }

        public void ParentToWindow(Transform trans, bool hide = false)
        {
            if (hide == true)
            {
                Hide();
            }
            free = false;
            Parent(trans);
        }

        public bool Grab(Vector3 target, Action<CardViz> onStart, Action<CardViz> onComplete)
        {
            if (gameObject.activeSelf == true && free == true)
            {
                var cardVizY = this.Yield();

                if (cardVizY.free == true)
                {
                    cardVizY.transform.DOComplete(true);
                    cardVizY.isDragging = false;

                    cardVizY.transform.parent?.GetComponentInParent<ICardDock>(true)?.
                        OnCardUndock(cardVizY.gameObject);
                    cardVizY.gameObject.SetActive(true);

                    cardVizY.transform.position = cardVizY.Position();
                    cardVizY.Parent(null);

                    if (onStart != null)
                    {
                        onStart(cardVizY);
                    }

                    Action<Drag> onMoveEnd = x =>
                    {
                        if (onComplete != null)
                        {
                            onComplete(cardVizY);
                        }
                    };

                    cardVizY.DOMove(target, GameManager.Instance.normalSpeed, null, onMoveEnd);

                    return true;
                }
            }
            return false;
        }

        public CardVizSave Save()
        {
            var save = new CardVizSave();
            save.ID = GetInstanceID();
            save.card = card;
            save.fragSave = fragTree.Save();
            save.free = free;
            save.faceDown = faceDown;
            save.position = transform.position;
            if (cardDecay.timeLeft != 0f)
            {
                save.decaySave = cardDecay.Save();
            }
            if (cardStack.Count > 1)
            {
                List<CardViz> cards = new List<CardViz>();
                cardStack.GetComponentsInChildren(true, cards);
                save.stackedCards = new List<int>();
                foreach (var cardViz in cards)
                {
                    save.stackedCards.Add(cardViz.GetInstanceID());
                }
            }
            return save;
        }

        public void Load(CardVizSave save)
        {
            LoadCard(save.card, false);
            fragTree.Load(save.fragSave);
            free = save.free;
            faceDown = save.faceDown;
            if (faceDown == true)
            {
                ShowBack();
            }
            transform.position = save.position;
            if (save.decaySave != null)
            {
                cardDecay.Load(save.decaySave);
            }
            if (save.stackedCards != null)
            {
                foreach (var cardID in save.stackedCards)
                {
                    cardStack.Push(SaveManager.Instance.CardFromID(cardID));
                }
            }
            draggingPlane = GameManager.Instance.cardDragPlane;
        }

        public bool CanStack(CardViz cardViz)
        {
            if (card == cardViz.card &&
                faceDown == false &&
                cardViz.faceDown == false &&
                cardDecay.enabled == false &&
                cardViz.cardDecay.enabled == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Stack(CardViz cardViz)
        {
            cardStack.Push(cardViz);
        }

        public void OnDecayComplete(Card targetCard)
        {
            var transform = visualsGO.transform;

            if (targetCard != null)
            {
                var yScale = transform.localScale.y;
                var targetScale = new Vector3(transform.localScale.x, 0f, transform.localScale.z);
                transform.DOScale(targetScale, GameManager.Instance.scaleSpeed).
                    OnComplete(() =>
                    {
                        if (card.onDecayComplete != null)
                        {
                            using (var context = new Context(this))
                            {
                                card.onDecayComplete.Run(context);
                            }
                        }

                        Transform(targetCard);

                        if (card.onDecayInto != null)
                        {
                            using (var context = new Context(this))
                            {
                                card.onDecayInto.Run(context);
                            }
                        }

                        var targetScale2 = new Vector3(transform.localScale.x, yScale, transform.localScale.z);
                        transform.DOScale(targetScale2, GameManager.Instance.scaleSpeed);
                    });
            }
            else
            {
                var targetScale = new Vector3(transform.localScale.x, 0f, transform.localScale.z);
                transform.DOScale(targetScale, GameManager.Instance.scaleSpeed).
                    OnComplete(() =>
                    {
                        if (card.onDecayComplete != null)
                        {
                            using (var context = new Context(this))
                            {
                                card.onDecayComplete.Run(context);
                            }
                        }

                        Destroy();
                    });
            }
        }

        private void LoadFragments()
        {
            foreach (var frag in card.fragments)
            {
                if (frag != null)
                {
                    fragTree.Add(frag);
                }
            }

            // if (fragTree.localFragments.Count > 0)
            // {
            //     fragTree.memoryFragment = fragTree.localFragments[0].fragment;
            // }
            if (fragTree.memoryFragment == null)
            {
                fragTree.memoryFragment = card;
            }
        }

        private Vector3 Position()
        {
            var actWindow = GetComponentInParent<ActWindow>();
            if (actWindow && actWindow.open == false)
            {
                return actWindow.tokenViz.transform.position;
            }
            else
            {
                return transform.position;
            }
        }

        private void Awake()
        {
            if (card != null)
            {
                LoadCard(card);
            }
        }

        private void Start()
        {
            if (card == null)
            {
                Debug.LogError("Please set Card for " + this.name);
            }

            if (cardDecay.timeLeft == 0f && card.lifetime > 0f)
            {
                Decay(card.decayTo, card.lifetime);
            }

            draggingPlane = GameManager.Instance.cardDragPlane;
        }
    }

    [Serializable]
    public class CardVizSave
    {
        public int ID;
        public Card card;
        public FragTreeSave fragSave;
        public bool free;
        public bool faceDown;
        public CardDecaySave decaySave;
        public List<int> stackedCards;
        public Vector3 position;
    }
}
