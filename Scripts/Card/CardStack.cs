using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


namespace CultistLike
{
    public class CardStack : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [HideInInspector] public bool stackDrag;

        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject stackCounterGO;

        [SerializeField, HideInInspector] private int count;

        private CardViz parent;
        private const int maxCount = 99;


        public int Count { get => count; private set => SetCount(value); }


        public void OnBeginDrag(PointerEventData eventData)
        {
            stackDrag = true;
            eventData.pointerDrag = parent.gameObject;
            parent.OnBeginDrag(eventData);
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
                if (parent.fragments.parent != null)
                {
                    parent.fragments.parent.Add(cardViz);
                }
                cardViz.stack = parent;
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
                if (parent.fragments.parent != null)
                {
                    parent.fragments.parent.Remove(cardViz);
                }
                cardViz.stack = null;
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

                Push(stack.parent);
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

        private void Awake()
        {
            parent = GetComponentInParent<CardViz>();
        }

        private void Start()
        {
            SetCount(count);
        }
    }
}
