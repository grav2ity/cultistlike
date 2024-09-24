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

        [SerializeField, HideInInspector] private ActWindow actWindow;
        [SerializeField, HideInInspector] private CardViz _slottedCard;
        [SerializeField, HideInInspector] private bool open;

        public string Title { get => title.text; set => title.text = value; }
        public CardViz slottedCard { get => _slottedCard; private set => _slottedCard = value; }
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
                if (gameObject.activeInHierarchy)
                {
                    SlotCard(cardViz);
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

        public void SlotCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cardViz.transform.SetParent(transform);
                cardViz.transform.localPosition = Vector3.zero;

                slottedCard = cardViz;
                actWindow.Check();

                cardViz.UnhighlightTargets();
            }
        }

        public void UnslotCard()
        {
            if (slottedCard != null)
            {
                slottedCard.transform.SetParent(null);
                slottedCard = null;
            }
            if (autoClose == true)
            {
                CloseSlot();
            }
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
                Destroy(slottedCard.gameObject, 1f);
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
