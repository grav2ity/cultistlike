using System;

using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;
using TMPro;


namespace CultistLike
{
    public class TokenViz : Viz, IDropHandler, IPointerClickHandler
    {
        [Header("Token")]
        public Token token;
        public Act autoPlay;
        public Rule initRule;

        [HideInInspector] public ActLogic parent;

        [Header("Layout")]
        [SerializeField] private TextMeshPro title;
        [SerializeField] private Timer _timer;
        [SerializeField] private TextMeshPro resultCounter;
        [SerializeField] private GameObject resultCounterGO;
        [SerializeField] private Renderer artBack;
        [SerializeField] private Renderer highlight;
        [SerializeField] private SpriteRenderer art;

        [Header("Table")]
        [Tooltip("Size on the table for an Array based table; final size is (1,1) + 2*(x,y)")]
        [SerializeField] private Vector2Int CellCount;

        [SerializeField, HideInInspector] private ActWindow _actWindow;
        [SerializeField, HideInInspector] private int resultCount;


        public ActWindow actWindow { get => _actWindow; private set => _actWindow = value; }
        public Timer timer { get => _timer; private set => _timer = value; }

        public override Vector2Int GetCellSize() => CellCount;


        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var droppedCard = eventData.pointerDrag.GetComponent<CardViz>();
                if (droppedCard != null && droppedCard.isDragging == true)
                {
                    actWindow.TrySlotAndBringUp(droppedCard);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            actWindow.BringUp();
        }

        public void Dissolve()
        {
            interactive = false;

            transform.DOScale(new Vector3(0f, 0f, 0f), 1).
                OnComplete(() => { GameManager.Instance.DestroyToken(this); });
        }

        public void LoadToken(Token token)
        {
            if (token != null)
            {
                this.token = token;
                title.text = token.label;
                if (token.art != null)
                {
                    art.sprite = token.art;
                }
                artBack.material.SetColor("_Color", token.color);
            }
        }

        public void SetResultCount(int count)
        {
            resultCount = count;
            resultCounter.text = count.ToString();
            resultCounterGO.SetActive(resultCount > 0);
        }

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void ShowTimer(bool p = true)
        {
            timer.gameObject.SetActive(p);
        }

        //used by modifiers. distinct from SlotViz grab
        public bool Grab(CardViz cardViz, bool bringUp = false)
        {
            if (cardViz.free == true)
            {
                Vector3 target;
                if (actWindow.gameObject.activeInHierarchy == true)
                {
                    target = actWindow.transform.position;
                }
                else
                {
                    target = targetPosition;
                }

                Action<CardViz> onComplete = x =>
                {
                    actWindow.ParentCardToWindow(x);
                };

                cardViz.Grab(target, null, onComplete);
                return true;
            }
            return false;
        }

        private void Awake()
        {
            if (token != null)
            {
                LoadToken(token);
            }
        }

        private void Start()
        {
            draggingPlane = GameManager.Instance.cardDragPlane;

            if (token == null)
            {
                Debug.LogError("Please set Token for " + this.name);
            }

            actWindow = Instantiate(GameManager.Instance.actWindowPrefab,
                                    GameManager.Instance.windowPlane);
            actWindow.LoadToken(this);
            actWindow.GetComponent<ActLogic>().SetParent(parent);
            actWindow.GetComponent<ActLogic>().ForceRule(initRule);

            actWindow.timer.SetFollowing(timer);
            ShowTimer(false);

            GameManager.Instance.tokens.Add(this);
        }
    }
}
