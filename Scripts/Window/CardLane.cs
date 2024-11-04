using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;


namespace CultistLike
{
    [RequireComponent(typeof(RectTransform))]
    public class CardLane : MonoBehaviour, ICardDock
    {
        public float maxSpacingX;
        public float spacingZ;
        [HideInInspector] public List<CardViz> cards;

        [SerializeField, HideInInspector] private ActWindow actWindow;


        private Rect rect { get => GetComponent<RectTransform>().rect; }


        public void OnCardDock(GameObject go)
        {
            CardViz cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                if (gameObject.activeInHierarchy)
                {
                    cards.Add(cardViz);
                    cardViz.transform.SetParent(transform);
                }
                else
                {
                    GameManager.Instance.table.ReturnToTable(cardViz);
                }
                actWindow.HoldCard(cardViz);
                actWindow.UpdateBars();
            }
        }

        public void OnCardUndock(GameObject go)
        {
            CardViz cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                cards.Remove(cardViz);
                cardViz.ShowFace();

                if (actWindow.gameObject.activeInHierarchy == false)
                {
                    cardViz.transform.SetParent(actWindow.tokenViz.transform);
                    cardViz.transform.localPosition = Vector3.zero;
                }
                actWindow.UnholdCard(cardViz);
                actWindow.UpdateBars();
                actWindow.Check();
            }
        }

        public void PlaceCards(List<CardViz> cards)
        {
            this.cards = cards.GetRange(0, cards.Count);

            Vector3 spacing = new Vector3(
                Math.Min(rect.width / cards.Count, maxSpacingX),
                0f,
                -spacingZ
            );

            Vector3 o = new Vector3(
                -0.5f * spacing.x * (cards.Count - 1),
                0,
                0
            );

            foreach (var cardViz in cards)
            {
                cardViz.gameObject.SetActive(true);
                cardViz.transform.SetParent(transform);
                cardViz.ShowBack();
                cardViz.transform.localPosition = o;
                o += spacing;
            }

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                GameManager.Instance.CardInPlay(cards[i]);
            }
        }

        private void Awake()
        {
            actWindow = GetComponentInParent<ActWindow>();
        }
    }
}
