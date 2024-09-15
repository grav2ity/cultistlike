using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Prefabs")]
        public CardViz cardPrefab;
        public ActViz actPrefab;
        public ActWindow actWindowPrefab;
        public AspectViz aspectPrefab;

        [Header("Table")]
        public Table<Vector2Int> table;

        [Header("Planes")]
        public RectTransform windowPlane;
        public RectTransform cardDragPlane;

        [Header("Card move speed")]
        public float normalSpeed;
        public float fastSpeed;

        [HideInInspector] public List<ActViz> acts;

        [SerializeField, HideInInspector] private List<CardViz> cards;
        [SerializeField, HideInInspector] private ActWindow _openWindow;


        public ActWindow openWindow { get => _openWindow; private set => _openWindow = value; }


        public void CloseWindow()
        {
            openWindow = null;
        }

        public void OpenWindow(ActWindow window)
        {
            if (openWindow != window)
            {
                openWindow?.Close();
                openWindow = window;
            }
        }

        public void AddCard(CardViz cardViz)
        {
            cards.Add(cardViz);
        }

        public void DestroyCard(CardViz cardViz)
        {
            cards.Remove(cardViz);
            table.RemoveCard(cardViz);
            cardViz.gameObject.SetActive(false);
            Destroy(cardViz.gameObject, 1f);
        }

        private void Awake()
        {
            Instance = this;

            acts = new List<ActViz>();
            cards = new List<CardViz>();
        }
    }
}
