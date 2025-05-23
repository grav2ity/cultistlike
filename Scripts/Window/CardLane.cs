using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;


namespace CultistLike
{
    [RequireComponent(typeof(RectTransform))]
    public class CardLane : MonoBehaviour, ICardDock
    {
        public bool stackMatching;
        public float maxSpacingX;
        public float spacingZ;
        [HideInInspector] public List<CardViz> cards;

        [SerializeField, HideInInspector] private ActWindow actWindow;


        public int Count
        {
            get
            {
                int count = 0;
                foreach (var cardViz in cards)
                {
                    count += cardViz.stackCount;
                }
                return count;
            }
        }

        private Rect rect { get => GetComponent<RectTransform>().rect; }


        public void OnCardDock(GameObject go)
        {
            CardViz cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                GameManager.Instance.table.ReturnToTable(cardViz);
            }
        }

        //TODO won't get called when getting card from stack
        //TODO parent ??
        public void OnCardUndock(GameObject go)
        {
            CardViz cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                cards.Remove(cardViz);
                cardViz.ShowFace();
            }
        }

        public void ParentCards(List<CardViz> newCards)
        {
            foreach (var cardViz in newCards)
            {
                cardViz.transform.DOComplete(true);
                cardViz.Show();
                cardViz.free = true;
                cardViz.interactive = true;
                cardViz.Parent(transform);

                cardViz.DecayDefault();
            }
            cards = newCards;

            PlaceCards();
        }

        public void PlaceCards()
        {
            var newCards = cards;

            if (stackMatching == true)
            {
                List<CardViz> cardsStacked = new List<CardViz>();
                foreach (var cardViz in cards)
                {
                    bool stacked = false;
                    foreach (var cardS in cardsStacked)
                    {
                        if (cardViz.CanStack(cardS))
                        {
                            cardS.Stack(cardViz);
                            stacked = true;
                            break;
                        }
                    }
                    if (stacked == false)
                    {
                        cardsStacked.Add(cardViz);
                    }
                }
                cards = cardsStacked;
            }

            if (cards.Count > 0)
            {
                Vector3 spacing = new Vector3(
                    Math.Min(rect.width / cards.Count, maxSpacingX),
                    0f,
                    spacingZ
                );

                Vector3 o = new Vector3(
                    -0.5f * spacing.x * (cards.Count - 1),
                    0,
                    -spacingZ * (cards.Count)
                );

                foreach (var cardViz in cards)
                {
                    if (actWindow.open == false)
                    {
                        cardViz.Hide();
                    }
                    cardViz.transform.localPosition = o;
                    o += spacing;
                }
            }

            for (int i = newCards.Count - 1; i >= 0; i--)
            {
                GameManager.Instance.CardInPlay(newCards[i]);
            }
        }

        public void Hide()
        {
            foreach (var cardViz in cards)
            {
                cardViz.Hide();
            }
        }

        public void Show()
        {
            foreach (var cardViz in cards)
            {
                cardViz.Show();
            }
        }

        public CardLaneSave Save()
        {
            var save = new CardLaneSave();
            save.maxSpacingX = maxSpacingX;
            save.spacingZ = spacingZ;

            save.cards = new List<int>();
            foreach (var cardViz in cards)
            {
                save.cards.Add(cardViz.GetInstanceID());
            }
            return save;
        }

        public void Load(CardLaneSave save)
        {
            maxSpacingX = save.maxSpacingX;
            spacingZ = save.spacingZ;

            var cards = new List<CardViz>();
            foreach (var cardID in save.cards)
            {
                cards.Add(SaveManager.Instance.CardFromID(cardID));
            }
            ParentCards(cards);
        }

        private void Awake()
        {
            actWindow = GetComponentInParent<ActWindow>();
        }
    }

    [Serializable]
    public class CardLaneSave
    {
        public float maxSpacingX;
        public float spacingZ;
        public List<int> cards;
    }
}
