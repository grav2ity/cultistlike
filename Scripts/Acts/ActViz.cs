using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;
using TMPro;

namespace CultistLike
{
    public class ActViz : Viz, IDropHandler, IPointerClickHandler
    {
        [Header("Act")]
        public Act act;

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

        public void LoadAct(Act act)
        {
            if (act == null)
                return;

            this.act = act;
            title.text = act.actName;
            if (act.art != null)
            {
                art.sprite = act.art;
            }
            artBack.material.SetColor("_Color", act.color);
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

        public void SetAct(Act c)
        {
            act = c;
        }

        public void SetHighlight(bool p)
        {
            highlight.enabled = p;
        }

        public void ShowTimer(bool p = true)
        {
            timer.gameObject.SetActive(p);
        }

        private void Start()
        {
            draggingPlane = GameManager.Instance.cardDragPlane;

            LoadAct(act);

            actWindow = Instantiate(GameManager.Instance.actWindowPrefab,
                                    GameManager.Instance.windowPlane);
            actWindow.LoadAct(this);

            actWindow.timer.SetFollowing(timer);
            ShowTimer(false);

            GameManager.Instance.acts.Add(this);
        }
    }
}
