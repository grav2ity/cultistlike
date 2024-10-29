using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using DG.Tweening;


namespace CultistLike
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Prefabs")]
        public CardViz cardPrefab;
        public TokenViz tokenPrefab;
        public ActWindow actWindowPrefab;
        public FragmentViz fragmentPrefab;

        [Header("Table")]
        public Table<Vector2Int> table;

        [Header("Planes")]
        public RectTransform windowPlane;
        public RectTransform cardDragPlane;

        [Header("Card transform time")]
        public float normalSpeed;
        public float fastSpeed;
        public float rotateSpeed;
        public float scaleSpeed;

        [Header("Special fragments")]
        public Fragment thisCard;
        public Fragment matchedCard;

        [HideInInspector] public List<Slot> slotSOS;

        [SerializeField, HideInInspector] private List<TokenViz> _tokens;
        [SerializeField, HideInInspector] private List<CardViz> _cards;

        [SerializeField, HideInInspector] private ActWindow _openWindow;
        [SerializeField, HideInInspector] private float elapsedTime;

        private float timeScale;
        private List<Act> initialActs;


        public List<CardViz> cards { get => _cards; }
        public List<TokenViz> tokens { get => _tokens; }

        public ActWindow openWindow { get => _openWindow; private set => _openWindow = value; }
        public float time { get => elapsedTime; }


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
            if (cardViz != null && cards.Contains(cardViz) == false)
            {
                cards.Add(cardViz);
            }
        }

        public CardViz CreateCard(Card card)
        {
            var cardViz = UnityEngine.Object.Instantiate(cardPrefab);
            cardViz.LoadCard(card);
            AddCard(cardViz);
            return cardViz;
        }

        public void DestroyCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards.Remove(cardViz);
                table.RemoveCard(cardViz);
                cardViz.gameObject.SetActive(false);
                Destroy(cardViz.gameObject, 1f);
            }
        }

        public void AddToken(TokenViz tokenViz)
        {
            if (tokenViz != null && tokens.Contains(tokenViz) == false)
            {
                tokens.Add(tokenViz);
            }
        }

        public void DestroyToken(TokenViz tokenViz)
        {
            if (tokenViz != null)
            {
                tokens.Remove(tokenViz);
                table.Remove(tokenViz);
                tokenViz.gameObject.SetActive(false);
                Destroy(tokenViz.gameObject, 1f);
            }
        }

        public TokenViz SpawnAct(Act act, ActLogic parent)
        {
            if (act != null && act.token != null && parent != null && parent.tokenViz != null)
            {

                var newTokenViz = SpawnToken(act.token, parent.tokenViz);
                if (newTokenViz != null)
                {
                    newTokenViz.autoPlay = act;
                    newTokenViz.parent = parent;
                    return newTokenViz;
                }
            }
            return null;
        }

        public TokenViz SpawnToken(Token token, Viz viz)
        {
            if (token != null && viz != null)
            {
                var newTokenViz = UnityEngine.Object.Instantiate(GameManager.Instance.tokenPrefab,
                                                                 viz.transform.position, Quaternion.identity);
                newTokenViz.LoadToken(token);

                var root = newTokenViz.transform;
                var localScale = root.localScale;

                GameManager.Instance.table.Place(viz, new List<Viz> { newTokenViz });

                root.localScale = new Vector3(0f, 0f, 0f);
                root.DOScale(localScale, 1);

                return newTokenViz;
            }
            return null;
        }

        // public List<CardViz> GetCards() {
        //     return cards;
        // }

        public void SetTimeScale(float ts)
        {
            timeScale = ts;
        }

        public List<Act> GetInitialActs() => initialActs;

        private void FindIninitalActs()
        {
            initialActs = new List<Act>();
            var acts = Resources.LoadAll("", typeof(Act)).Cast<Act>().ToArray();
            foreach (var act in acts)
            {
                if (act.initial == true)
                {
                    initialActs.Add(act);
                }
            }
        }

        private void FindSlotSOS()
        {
            slotSOS = new List<Slot>(Resources.LoadAll("", typeof(Slot)).Cast<Slot>().ToArray());
        }

        private void Awake()
        {
            Instance = this;

            timeScale = 1f;

            FindIninitalActs();
            FindSlotSOS();
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * timeScale;
        }
    }
}
