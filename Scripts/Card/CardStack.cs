using UnityEngine;

using TMPro;


namespace CultistLike
{
    public class CardStack : MonoBehaviour
    {
        [SerializeField] public GameObject stackCounterGO;

        [Header("Layout")]
        [SerializeField] private TextMeshPro text;

        [SerializeField, HideInInspector] private int count;

        private CardViz parent;
        private const int maxCount = 99;


        public int Count => count;


        public bool Push(CardViz cardViz)
        {
            if (Count < maxCount)
            {
                cardViz.Parent(transform);
                cardViz.transform.localPosition = Vector3.zero;
                //this would disable Decay timer which is fine since stacking Cards with Decay is not allowed
                cardViz.gameObject.SetActive(false);
                SetCount(count + 1);
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
            var cardViz = Top();
            if (cardViz != null)
            {
                cardViz.gameObject.SetActive(true);
                SetCount(count - 1);
                cardViz.stack = null;
            }

            return cardViz;
        }

        public CardViz Top() => GetComponentInChildren<CardViz>(true);

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
            stackCounterGO.GetComponent<SphereCollider>().enabled = count > 1;
            stackCounterGO.SetActive(count > 1);
        }

        private void Awake()
        {
            parent = transform.GetComponentInNearestParent<CardViz>();
        }

        private void Start()
        {
            SetCount(count);
        }
    }
}
