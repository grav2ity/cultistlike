using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


namespace CultistLike
{
    public class Slot : MonoBehaviour, IDropHandler, ICardDock, IPointerClickHandler
    {
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

        [SerializeField, HideInInspector] private ActWindow actWindow;
        [SerializeField, HideInInspector] private CardViz _slottedCard;
        [SerializeField, HideInInspector] private bool open;

        public string Title { get => title.text; set => title.text = value; }
        public CardViz slottedCard
        {
            get => _slottedCard;
            set
            {
                _slottedCard = value;
                if (_slottedCard != null)
                {
                    actWindow.Check();
                }
            }
        }

        public bool empty { get => open == true && slottedCard == null; }
        public int index { get; set; }


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
            if (cardViz != null)
            {
                if (gameObject.activeInHierarchy &&
                    (!onlyMatching || actWindow.MatchesAnyOpenSlot(cardViz.card) == true))
                {
                    if (slottedCard == null)
                    {
                        SlotCard(cardViz);
                    }
                    else if (cardLock == false)
                    {
                        var sc = UnslotCard();
                        GameManager.Instance.table.ReturnToTable(sc);
                        SlotCard(cardViz);
                    }
                }
                else
                {
                    GameManager.Instance.table.ReturnToTable(cardViz);
                }
            }
        }

        public void OnCardUndock(GameObject go)
        {
            UnslotCard();
            actWindow.Check();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            actWindow.HighlightCards(index);
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
                }
            }
        }

        public CardViz UnslotCard()
        {
            var sc = slottedCard;
            if (slottedCard != null)
            {
                slottedCard.transform.SetParent(null);
                slottedCard = null;
            }
            if (autoClose == true)
            {
                CloseSlot();
            }
            return sc;
        }

        public void CloseSlot()
        {
            if (slottedCard != null)
            {
                var cardViz = slottedCard;
                UnslotCard();
                GameManager.Instance.table.ReturnToTable(cardViz);
            }
            gameObject.SetActive(false);
            open = false;
        }

        public void OpenSlot(string name = "")
        {
            gameObject.SetActive(true);
            open = true;
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

        private void Awake()
        {
            actWindow = GetComponentInParent<ActWindow>();
        }
    }
}
