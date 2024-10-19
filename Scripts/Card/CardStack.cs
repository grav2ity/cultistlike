using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


namespace CultistLike
{
    public class CardStack : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject stackCounterGO;

        [SerializeField] private int count;
        private const int maxCount = 99;


        public int Count { get => count; private set => SetCount(value); }


        public bool Push(CardViz cardViz)
        {
            if (Count < maxCount)
            {
                cardViz.transform.SetParent(transform);
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
