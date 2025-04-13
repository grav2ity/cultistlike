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

        [Header("Root")]
        public FragTree root;

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
        [SerializeField, HideInInspector] private List<ActWindow> _windows;

        [SerializeField, HideInInspector] private ActWindow _openWindow;
        [SerializeField, HideInInspector] private float elapsedTime;

        private List<Act> _initialActs;
        private List<Slot> _slotSOS;

        public List<CardViz> cards { get => root.cards; }
        public List<TokenViz> tokens { get => _tokens; }
        public List<ActWindow> windows { get => _windows; }

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

        public void CardInPlay(CardViz cardViz)
        {
            onCardInPlay.Invoke(cardViz);
        }

        public CardViz CreateCard()
        {
            var cardViz = UnityEngine.Object.Instantiate(cardPrefab);
            cardViz.draggingPlane = GameManager.Instance.cardDragPlane;
            return cardViz;
        }

        public CardViz CreateCard(Card card)
        {
            var cardViz = CreateCard();
            cardViz.LoadCard(card);
            return cardViz;
        }

        public void DestroyCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cardViz.Parent(null);
                cardViz.gameObject.SetActive(false);
                Destroy(cardViz.gameObject, 0.1f);
            }
        }

        public void AddToken(TokenViz tokenViz)
        {
            if (tokenViz != null && tokens.Contains(tokenViz) == false)
            {
                tokens.Add(tokenViz);
            }
        }

        public TokenViz CreateToken()
        {
            var tokenViz = UnityEngine.Object.Instantiate(tokenPrefab);
            AddToken(tokenViz);
            return tokenViz;
        }

        public TokenViz CreateToken(Token token)
        {
            var tokenViz = CreateToken();
            tokenViz.LoadToken(token);
            return tokenViz;
        }

        public void DestroyToken(TokenViz tokenViz)
        {
            if (tokenViz != null)
            {
                tokens.Remove(tokenViz);
                table.Remove(tokenViz);
                tokenViz.gameObject.SetActive(false);
                Destroy(tokenViz.gameObject, 0.1f);
            }
        }

        public void AddWindow(ActWindow actWindow)
        {
            if (actWindow != null && windows.Contains(actWindow) == false)
            {
                windows.Add(actWindow);
            }
        }

        public ActWindow CreateWindow()
        {
            var actWindow = UnityEngine.Object.Instantiate(actWindowPrefab, windowPlane);
            AddWindow(actWindow);
            return actWindow;
        }

        public void DestroyWindow(ActWindow actWindow)
        {
            if (actWindow != null)
            {
                windows.Remove(actWindow);
                actWindow.gameObject.SetActive(false);
                Destroy(actWindow.gameObject, 0.1f);
            }
        }

        public TokenViz SpawnAct(Act act, ActLogic parent)
        {
            if (act != null && act.token != null)
            {
                var newTokenViz = SpawnToken(act.token, parent?.tokenViz);
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
            if (token != null)
            {
                if (token.unique == false || tokens.Find(x => x.token == token) == null)
                {
                    var newPosition = viz != null ? viz.transform.position : Vector3.zero;
                    var newTokenViz = UnityEngine.Object.Instantiate(tokenPrefab, newPosition, Quaternion.identity);
                    newTokenViz.LoadToken(token);

                    var root = newTokenViz.transform;
                    var localScale = root.localScale;

                    if (viz != null)
                    {
                        GameManager.Instance.table.Place(viz, new List<Viz> { newTokenViz });
                    }
                    else
                    {
                        GameManager.Instance.table.ReturnToTable(newTokenViz);
                    }

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

        public void Save()
        {
            var save = new GameManagerSave();
            DOTween.CompleteAll(true);

            foreach (var cardViz in cards)
            {
                save.cards.Add(cardViz.Save());
            }

            foreach (var tokenViz in tokens)
            {
                save.tokens.Add(tokenViz.Save());
            }

            save.decks = DeckManager.Instance.deckInsts;

            save.table = table.Save();

            var jsonSave = JsonUtility.ToJson(save);
            SaveManager.Instance.Save(jsonSave);
        }

        public void Load()
        {
            Reset();

            var jsonSave = SaveManager.Instance.Load();

            GameManagerSave save = new GameManagerSave(jsonSave);


            foreach (var cardSave in save.cards)
            {
                var cardViz = GameManager.Instance.CreateCard();
                SaveManager.Instance.RegisterCard(cardSave.ID, cardViz);
            }

            foreach (var cardSave in save.cards)
            {
                SaveManager.Instance.CardFromID(cardSave.ID).Load(cardSave);
            }

            foreach (var tokenSave in save.tokens)
            {
                var tokenViz = GameManager.Instance.CreateToken();
                tokenViz.Load(tokenSave);
                tokenViz.Parent(table.transform);
            }

            foreach (var cardSave in save.cards)
            {
                var cardViz = SaveManager.Instance.CardFromID(cardSave.ID);
                if (cardViz.transform.parent == null)
                {
                    cardViz.Parent(table.transform);
                }
            }

            DeckManager.Instance.Load(save.decks);

            //load table last
            table.Load(save.table);
        }

        public void Reset()
        {
            DOTween.CompleteAll(true);

            for (int i=cards.Count-1; i>=0; i--)
            {
                DestroyCard(cards[i]);
            }

            for (int i=tokens.Count-1; i>=0; i--)
            {
                DestroyToken(tokens[i]);
            }
            tokens.Clear();

            for (int i=windows.Count-1; i>=0; i--)
            {
                DestroyWindow(windows[i]);
            }
            windows.Clear();

            openWindow = null;
            UIManager.Instance?.cardInfo?.Unload();
            UIManager.Instance?.aspectInfo?.Unload();
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

    [Serializable]
    public class GameManagerSave
    {
        public List<CardVizSave> cards;
        public List<TokenVizSave> tokens;
        public List<DeckInst> decks;
        public string table;

        public GameManagerSave()
        {
            cards = new List<CardVizSave>();
            tokens = new List<TokenVizSave>();
        }

        public GameManagerSave(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
