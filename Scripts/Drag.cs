using System;

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

        [SerializeField] private bool _interactive;

        private bool _isDragging; //is actually dragging (moving object along)
        private Vector3 mouseOffset;
        private Vector3 dragOrigin;
        private ICardDock dragOriginDock;

        private bool isBeingMoved;
        private Vector3 moveTarget;


        public bool isDragging { get => _isDragging; protected set => _isDragging = value; }
        public bool interactive { get => _interactive; set => _interactive = value; }

        public Vector3 targetPosition => isBeingMoved == true ? moveTarget : transform.position;


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Assert(isDragging == false);

            if (CanDrag(eventData) == false || interactive == false)
                return;

            isDragging = true;

            dragOrigin = transform.position;

            dragOriginDock = transform.parent?.GetComponentInParent<ICardDock>();

            Parent(draggingPlane.transform);
            dragOriginDock?.OnCardUndock(gameObject);

            foreach(var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

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

        public virtual void Parent(Transform newParent)
        {
            var oldParent = transform.parent;
            if (oldParent != newParent)
            {
                transform.SetParent(newParent);
                oldParent?.GetComponentInParent<FragTree>()?.OnChange();
                newParent?.GetComponentInParent<FragTree>()?.OnChange();
            }
        }

        public virtual void DOMove(Vector3 target, float speed, Action<Drag> onStart = null, Action<Drag> onComplete = null)
        {
            bool prevInteractive = interactive;
            interactive = false;

            moveTarget = target;
            isBeingMoved = true;

            if (onStart != null)
            {
                onStart(this);
            }

            transform.DOMove(target, speed).
                OnComplete(() =>
                {
                    interactive = prevInteractive;
                    isBeingMoved = false;
                    if (onComplete != null)
                    {
                        onComplete(this);
                    }
                });
        }

        public void Undrag()
        {
            //TODO dock OnStart?
            DOMove(dragOrigin, GameManager.Instance.normalSpeed, null, x =>
                   dragOriginDock?.OnCardDock(gameObject));
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
