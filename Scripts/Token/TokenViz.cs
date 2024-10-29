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
        [Tooltip("Dissapears after completion.")]

        [HideInInspector] public ActLogic parent;

        [Header("Layout")]
        [SerializeField] private TextMeshPro title;
        [SerializeField] private Timer _timer;
        [SerializeField] private TextMeshPro resultCounter;
        [SerializeField] private Renderer artBack;
        [SerializeField] private Renderer highlight;
        [SerializeField] private SpriteRenderer art;

        [Header("Table")]
        [Tooltip("Size on the table for an Array based table; final size is (1,1) + 2*(x,y)")]
        [SerializeField] private Vector2Int CellCount;

        [SerializeField, HideInInspector] private ActWindow _actWindow;


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
                    return;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            actWindow.BringUp();
        }

        public void Dissolve()
        {
            this.interactive = false;

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
            if (count == 0)
            {
                resultCounter.gameObject.SetActive(false);
            }
            else
            {
                resultCounter.gameObject.SetActive(true);
                resultCounter.text = count.ToString();
            }
        }

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void ShowTimer(bool p = true)
        {
            timer.gameObject.SetActive(p);
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

            actWindow.timer.SetFollowing(timer);
            ShowTimer(false);

            GameManager.Instance.tokens.Add(this);
        }
    }
}
