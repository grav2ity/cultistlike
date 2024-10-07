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
            }
        }

        public void OnCardUndock(GameObject go)
        {
            CardViz cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                cards.Remove(cardViz);
                cardViz.ShowFace();
                actWindow.Check();
            }
        }

        public void PlaceCards(List<CardViz> cards)
        {
            this.cards = cards;

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
                cardViz.transform.SetParent(transform);
                cardViz.ShowBack();
                cardViz.transform.localPosition = o;
                o += spacing;
            }
        }

        private void Awake()
        {
            cards = new List<CardViz>();

            actWindow = GetComponentInParent<ActWindow>();
        }
    }
}
