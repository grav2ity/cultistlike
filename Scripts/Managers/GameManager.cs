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
        public AspectViz aspectPrefab;

        [Header("Table")]
        public Table<Vector2Int> table;

        [Header("Planes")]
        public RectTransform windowPlane;
        public RectTransform cardDragPlane;

        [Header("Card move speed")]
        public float normalSpeed;
        public float fastSpeed;
        public float rotateSpeed;

        public List<Slot> slotTypes;

        [HideInInspector] public List<TokenViz> tokens;

        [SerializeField, HideInInspector] private List<CardViz> cards;
        [SerializeField, HideInInspector] private ActWindow _openWindow;

        [SerializeField, HideInInspector] private float elapsedTime;
        private float timeScale;

        private List<Act> initialActs;


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
            cards.Add(cardViz);
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

        public void SpawnAct(Act act, Viz viz)
        {
            if (act != null && act.token != null && viz != null)
            {

                var newTokenViz = SpawnToken(act.token, viz);
                if (newTokenViz != null)
                {
                    newTokenViz.SetToken(act.token);
                    newTokenViz.autoPlay = act;
                }
            }
        }

        public TokenViz SpawnToken(Token token, Viz viz)
        {
            if (token != null && viz != null)
            {
                var newTokenViz = UnityEngine.Object.Instantiate(GameManager.Instance.tokenPrefab,
                                                                 viz.transform.position, Quaternion.identity);
                newTokenViz.SetToken(token);

                var root = newTokenViz.transform;
                var localScale = root.localScale;

                GameManager.Instance.table.Place(viz, new List<Viz> { newTokenViz });

                root.localScale = new Vector3(0f, 0f, 0f);
                root.DOScale(localScale, 1);

                return newTokenViz;
            }
            return null;
        }

        public List<CardViz> GetCards()
        {
            return cards;
        }

        public void SetTimeScale(float ts)
        {
            timeScale = ts;
        }

        public List<Act> GetInitialActs() => initialActs;

        private void FindIninitalActs()
        {
            var acts = Resources.LoadAll("", typeof(Act)).Cast<Act>().ToArray();
            foreach (var act in acts)
            {
                if (act.initial == true)
                {
                    initialActs.Add(act);
                }
            }
        }

        private void Awake()
        {
            Instance = this;

            timeScale = 1f;

            tokens = new List<TokenViz>();
            cards = new List<CardViz>();
            initialActs = new List<Act>();

            FindIninitalActs();

        #if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        #endif
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * timeScale;
        }
    }
}
