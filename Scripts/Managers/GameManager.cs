using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

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
        public Fragment thisAspect;
        public Fragment thisCard;
        public Fragment matchedCards;

        [Header("Time control")]
        public float timeScale;
        public float maxTime;
        public float allTime;

        [HideInInspector] public UnityEvent<CardViz> onCardInPlay;

        [SerializeField, HideInInspector] private List<TokenViz> _tokens;
        [SerializeField, HideInInspector] private FragContainer _fragments;

        [SerializeField, HideInInspector] private ActWindow _openWindow;
        [SerializeField, HideInInspector] private float elapsedTime;

        private List<Act> _initialActs;
        private List<Slot> _slotSOS;


        public FragContainer fragments { get => _fragments; }
        public List<CardViz> cards { get => fragments.cards; }
        public List<TokenViz> tokens { get => _tokens; }

        public List<Act> initialActs { get => _initialActs; private set => _initialActs = value; }
        public List<Slot> slotSOS { get => _slotSOS; private set => _slotSOS = value; }

        public ActWindow openWindow { get => _openWindow; private set => _openWindow = value; }
        public float time { get => elapsedTime; }


        public float DevTime(float time)
        {
            if (maxTime > 0)
            {
                return Math.Min(time, maxTime);
            }
            else if (allTime > 0)
            {
                return allTime;
            }
            else
            {
                return time;
            }
        }

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

        public void AddCard(CardViz cardViz) => fragments.Add(cardViz);

        public void CardInPlay(CardViz cardViz)
        {
            onCardInPlay.Invoke(cardViz);
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
                fragments.Remove(cardViz);
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
                    newTokenViz.initRule = act.onSpawn;
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
                if (token.unique == false || tokens.Find(x => x.token == token) == null)
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
            }
            return null;
        }

        public void SetTimeScale(float ts)
        {
            timeScale = ts;
        }

        public void SetMaxTime(float time)
        {
            maxTime = time;
            allTime = 0f;
        }

        public void SetAllTime(float time)
        {
            allTime = time;
            maxTime = 0f;
        }

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

            FindSlotSOS();
            FindIninitalActs();

            if (thisAspect == null || thisCard == null || matchedCards == null)
            {
                Debug.LogError("GameManager's Special fragments are missing!!");
            }
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * timeScale;
        }
    }
}
