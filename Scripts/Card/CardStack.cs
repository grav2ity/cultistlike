using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


namespace CultistLike
{
    public class CardStack : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject stackCounterGO;

        public bool stackDrag;

        [SerializeField, HideInInspector] private int count;

        private const int maxCount = 99;


        public int Count { get => count; private set => SetCount(value); }


        public void OnBeginDrag(PointerEventData eventData)
        {
            stackDrag = true;
            var cardViz = GetComponentInParent<CardViz>();
            eventData.pointerDrag = cardViz.gameObject;
            cardViz.OnBeginDrag(eventData);
            stackDrag = false;
        }

        //need this
        public void OnDrag(PointerEventData eventData) {}

        public bool Push(CardViz cardViz)
        {
            if (Count < maxCount)
            {
                cardViz.transform.SetParent(transform);
                cardViz.transform.localPosition = Vector3.zero;
                //this would disable Decay timer which is fine since stacking Cards with Decay is not allowed
                cardViz.gameObject.SetActive(false);
                SetCount(count + 1);
                return true;
            }
            else
            {
                return false;
            }
        }

        public CardViz Pop()
        {
            var cardViz = GetComponentInChildren<CardViz>(true);
            if (cardViz != null)
            {
                cardViz.gameObject.SetActive(true);
                SetCount(count - 1);
            }

            return cardViz;
        }

        public bool Merge(CardStack stack)
        {
            if (Count + stack.Count < maxCount)
            {
                var cardViz = stack.Pop();
                while (cardViz != null)
                {
                    Push(cardViz);
                    cardViz = stack.Pop();
                }

                Push(stack.GetComponentInParent<CardViz>());
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetCount(int count)
        {
            this.count = count;
            text.text = count.ToString();
            stackCounterGO.SetActive(count > 1);
        }

        private void Start()
        {
            SetCount(count);
        }
    }
}
