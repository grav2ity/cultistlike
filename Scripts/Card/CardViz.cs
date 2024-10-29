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

        [Header("Fragments")]
        public FragContainer fragments;

        [HideInInspector] public bool free;

        [Header("Layout")]
        [SerializeField] private TextMeshPro title;
        [SerializeField] private Renderer artBack;
        [SerializeField] private SpriteRenderer art;
        [SerializeField] private Renderer highlight;
        [SerializeField] private CardStack cardStack;
        [SerializeField] private CardDecay cardDecay;

        [Header("Table")]
        [Tooltip("Size on the table for an Array based table; final size is (1,1) + 2*(x,y)")]
        [SerializeField] private Vector2Int CellCount;

        [SerializeField, HideInInspector] private bool faceDown;


        public override Vector2Int GetCellSize() => CellCount;


        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (interactive == true && eventData.button == PointerEventData.InputButton.Left)
            {
                if (cardStack.Count > 1)
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
                    if (tokenViz.actWindow.MatchesAnyOpenSlot(this) == true)
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
                        if (card == droppedCard.card)
                        {
                            if (cardStack.Push(droppedCard) == true)
                            {
                                droppedCard.OnEndDrag(eventData);
                            }
                        }
                        return;
                    }

                    //handles dropping card on a slotted card
                    var slot = GetComponentInParent<SlotViz>();
                    if (slot != null && slot.autoClose == false)
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

        public void OnDecayComplete(Card card)
        {
            interactive = false;

            if (card != null)
            {
                var yScale = transform.localScale.y;
                var targetScale = new Vector3(transform.localScale.x, 0f, transform.localScale.z);
                transform.DOScale(targetScale, GameManager.Instance.scaleSpeed).
                    OnComplete(() =>
                    {
                        Transform(card);
                        var targetScale2 = new Vector3(transform.localScale.x, yScale, transform.localScale.z);
                        transform.DOScale(targetScale2, GameManager.Instance.scaleSpeed).
                            OnComplete(() => { interactive = true; });
                    });
            }
            else
            {
                var targetScale = new Vector3(transform.localScale.x, 0f, transform.localScale.z);
                transform.DOScale(targetScale, GameManager.Instance.scaleSpeed).
                    OnComplete(() => { GameManager.Instance.DestroyCard(this); });
            }
        }

        public void Decay(Card card, float time)
        {
            cardDecay.StartTimer(time, () => OnDecayComplete(card));
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
        }

        public void ShowFace()
        {
            transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            faceDown = false;
        }

        public void ShowBack()
        {
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            faceDown = true;
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

            if (card.lifetime > 0f)
            {
                Decay(card.decayTo, card.lifetime);
            }
        }

        private void LoadFragments()
        {
            foreach (var frag in card.fragments)
            {
                if (frag != null)
                {
                    fragments.Add(frag);
                }
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

            if (card.lifetime > 0f)
            {
                Decay(card.decayTo, card.lifetime);
            }

            draggingPlane = GameManager.Instance.cardDragPlane;
            GameManager.Instance.AddCard(this);
        }
    }
}
