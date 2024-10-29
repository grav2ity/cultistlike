using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using DG.Tweening;


namespace CultistLike
{
    /// <summary>
    /// Base class of a <c>Table</c> that locates cards by <c>T</c>.
    /// e.g. <c>Vector2Int</c> for array based table; <c>Vector3</c> for position based table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Table<T> : MonoBehaviour, ICardDock, IDropHandler, IPointerDownHandler
    {
        protected Dictionary<GameObject, T> lastLocations = new Dictionary<GameObject, T>();

        [SerializeField, HideInInspector] protected List<CardViz> cards;


        public abstract Vector3 ToLocalPosition(T t);
        public abstract T FromLocalPosition(Vector3 t);

        /// <summary>
        /// Find a free location on or in the vicinity of <c>t</c>.
        /// </summary>
        /// <param name="t">Location around which to begin search.</param>
        /// <param name="viz">Object to be placed.</param>
        /// <returns>True if <c>t</c> is set to a free location.</returns>
        public abstract bool FindFreeLocation(ref T t, Viz viz);

        /// <summary>
        /// Find free locations for multiple objects.
        /// </summary>
        /// <param name="viz">Object around which to begin search.</param>
        /// <param name="l">Objects to be placed.</param>
        /// <returns></returns>
        public abstract List<T> FindFreeLocations(Viz viz, List<Viz> l);

        public abstract void Remove(Viz viz);


        public virtual void OnDrop(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Drag drag = eventData.pointerDrag.GetComponent<Drag>();
                if (drag == null || drag.isDragging == false)
                {
                    return;
                }

                OnCardDock(eventData.pointerDrag);
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left &&
                eventData.pointerPressRaycast.gameObject.GetComponent<CardViz>() == null)
            {
                UIManager.Instance?.cardInfo?.Unload();
                UIManager.Instance?.aspectInfo?.Unload();
            }
        }

        public virtual void OnCardDock(GameObject go)
        {
            var viz = go.GetComponent<Viz>();
            if (viz == null)
                return;

            var localPosition = go.transform.position - transform.position;
            var loc = FromLocalPosition(localPosition);

            if (FindFreeLocation(ref loc, viz))
            {
                Place(viz, loc, GameManager.Instance.fastSpeed);
            }
            else
            {
                Debug.LogError("Could not find free location on the table.");
            }
        }

        public virtual void OnCardUndock(GameObject go)
        {
            var cardViz = go.GetComponent<CardViz>();
            if (cardViz != null)
            {
                cards.Remove(cardViz);
            }
        }

        /// <summary>
        /// Place multiple objects on free locations on the table.
        /// </summary>
        /// <param name="viz">Object around which to begin search for free locations.</param>
        /// <param name="l">Objects to be placed.</param>
        /// <returns></returns>
        public virtual void Place(Viz viz, List<Viz> l) {

            List<T> locations = FindFreeLocations(viz, l);
            int i = 0;
            foreach (var lviz in l)
            {
                if (i >= locations.Count)
                {
                    Debug.LogError("Could not find enough free space on the table.");
                    break;
                }
                Place(lviz, locations[i], GameManager.Instance.normalSpeed);
                i++;
            }
        }

        /// <summary>
        /// Place object on the table.
        /// </summary>
        /// <param name="viz">Object to be placed.</param>
        /// <param name="t">Location where the object will be placed.</param>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public virtual void Place(Viz viz, T t, float moveSpeed)
        {
            viz.transform.SetParent(transform);
            DOMove(viz, t, moveSpeed);

            lastLocations[viz.gameObject] = t;
            AddCard(viz.GetComponent<CardViz>());
        }

        /// <summary>
        /// Return to the last or the nearest location.
        /// </summary>
        /// <param name="viz"></param>
        /// <returns></returns>
        public virtual void ReturnToTable(Viz viz)
        {
            if (viz == null)
            {
                return;
            }

            T v;
            if (lastLocations.TryGetValue(viz.gameObject, out v) == true)
            {
                if (FindFreeLocation(ref v, viz))
                {
                    Place(viz, v, GameManager.Instance.normalSpeed);
                }
            }
            else
            {
                OnCardDock(viz.gameObject);
            }
        }

        public virtual List<CardViz> GetCards()
        {
            return cards;
        }

        public virtual void AddCard(CardViz cardViz)
        {
            if (cardViz != null && cards.Contains(cardViz) == false)
            {
                cards.Add(cardViz);
            }
        }

        public virtual void RemoveCard(CardViz cardViz)
        {
            cards.Remove(cardViz);
        }

        public virtual void HighlightCards(List<CardViz> cards)
        {
            if (cards != null && cards.Count > 0)
            {
                StartCoroutine(HighlightCardsE(cards));
            }
        }

        /// <summary>
        /// Finalize placement on the table.
        /// </summary>
        /// <returns></returns>
        protected virtual void PutOn(GameObject go)
        {
            go.transform.SetParent(transform);
            Vector3 v = go.transform.localPosition;
            go.transform.localPosition = new Vector3(v.x, v.y, 0);
        }

        protected virtual List<CardViz> FindCards()
        {
            List<CardViz> cards = new List<CardViz>();

            GetComponentsInChildren<CardViz>(false, cards);
            return cards;
        }

        protected void DOMove(Viz viz, T t, float speed)
        {
            bool prevInteractive = viz.interactive;
            viz.interactive = false;
            Vector3 targetPosition = ToLocalPosition(t) + transform.position;
            viz.transform.DOMove(targetPosition, speed).
                OnComplete(() => { viz.interactive = prevInteractive; PutOn(viz.gameObject); });
        }

        protected IEnumerator HighlightCardsE(List<CardViz> cards)
        {
            foreach (var card in cards)
            {
                card?.SetHighlight(true);
            }

            yield return new WaitForSeconds(1f);

            foreach (var card in cards)
            {
                card?.SetHighlight(false);
            }
        }
    }
}
