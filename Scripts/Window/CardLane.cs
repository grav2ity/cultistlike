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
                if (gameObject.activeInHierarchy == true)
                {
                    cards.Add(cardViz);
                    cardViz.transform.SetParent(transform);

                    actWindow.UpdateBars();
                    actWindow.Check();
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

                //TODO ??
                if (actWindow.gameObject.activeInHierarchy == false)
                {
                    cardViz.transform.SetParent(actWindow.tokenViz.transform);
                    cardViz.transform.localPosition = Vector3.zero;
                }
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
                cardViz.transform.DOComplete(true);
                cardViz.Show();
                cardViz.free = true;
                cardViz.interactive = true;
                cardViz.transform.SetParent(transform);
                cardViz.transform.localPosition = o;
                o += spacing;
            }

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                GameManager.Instance.CardInPlay(cards[i]);
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
            PlaceCards(cards);
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
