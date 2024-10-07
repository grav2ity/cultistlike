using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;


namespace CultistLike
{
    public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Drag Options")]
        [Tooltip("Object centered on cursor while dragging")]
        public bool centerOnCursor;
        [Tooltip("Return object to previous location if it was not docked after dropping")]
        public bool undrag;

        [HideInInspector] public RectTransform draggingPlane;

        [SerializeField, HideInInspector] private bool _interactive;

        private bool _isDragging; //is actually dragging (moving object along)
        private Vector3 mouseOffset;
        private Vector3 dragOrigin;
        private ICardDock dragOriginDock;


        public bool isDragging { get => _isDragging; private set => _isDragging = value; }
        public bool interactive { get => _interactive; set => _interactive = value; }


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Assert(isDragging == false);

            if (CanDrag(eventData) == false || interactive == false)
                return;

            isDragging = true;

            dragOrigin = transform.position;
            dragOriginDock = transform.parent?.GetComponentInParent<ICardDock>();
            dragOriginDock?.OnCardUndock(gameObject);

            foreach(var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            transform.SetParent(draggingPlane.transform);

            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane,
                                                                        eventData.position,
                                                                        eventData.pressEventCamera,
                                                                        out globalMousePos))
            {
                var screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane,
                                                                            screenPosition,
                                                                            eventData.pressEventCamera,
                                                                            out mouseOffset))
                {
                    mouseOffset = globalMousePos - mouseOffset;
                }
            }

            SetDraggedPosition(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (isDragging == false || CanDrag(eventData) == false || interactive == false)
                return;

            SetDraggedPosition(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (isDragging == false)
                return;

            isDragging = false;

            foreach(var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }

            if (undrag == true && transform.parent?.GetComponentInParent<ICardDock>() == null)
            {
                Undrag();
            }
        }

        public void Undrag()
        {
            interactive = false;
            transform.DOMove(dragOrigin, GameManager.Instance.normalSpeed).
                OnComplete(() => {
                    dragOriginDock?.OnCardDock(gameObject);
                    interactive = true;
                });
        }

        private void Awake()
        {
            interactive = true;
        }

        private bool CanDrag(PointerEventData eventData) =>
            eventData.button == PointerEventData.InputButton.Left &&
            draggingPlane != null;

        private void SetDraggedPosition(PointerEventData eventData)
        {
            Vector3 worldMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane,
                                                                        eventData.position,
                                                                        eventData.pressEventCamera,
                                                                        out worldMousePos))
            {
                if (centerOnCursor == false)
                {
                    worldMousePos = worldMousePos - mouseOffset;
                }
                transform.position = worldMousePos;
            }
        }
    }
}
