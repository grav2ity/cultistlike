using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class CardInfo : MonoBehaviour
    {
        [Header("Card Info")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private GameObject aspectsGO;


        private List<AspectViz> aspects = new List<AspectViz>();

        public string Description {
            get { return description.text; }
            set { description.text = value; }
        }

        public string CardName {
            get { return cardName.text; }
            set { cardName.text = value; }
        }

        public Sprite Art {
            get { return art.sprite; }
            set { art.sprite = value; }
        }


        public void LoadCard(CardViz cardViz)
        {
            gameObject.SetActive(true);

            CardName = cardViz.card.cardName;
            Description = cardViz.card.description;

            if (cardViz.card.art != null)
            {
                Art = cardViz.card.art;
            }
            else
            {
                art.color = cardViz.card.color;
            }


            if (cardViz.aspects.Count > aspects.Count)
            {
                while (cardViz.aspects.Count > aspects.Count)
                {
                    var aspect = Instantiate(GameManager.Instance.aspectPrefab, aspectsGO.transform);
                    aspects.Add(aspect);
                }

            }
            else if (cardViz.aspects.Count < aspects.Count)
            {
                for (int i = aspects.Count; i > cardViz.aspects.Count; i--)
                {
                    aspects[i - 1].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < cardViz.aspects.Count; i++)
            {
                aspects[i].LoadAspect(cardViz.aspects[i]);
                aspects[i].gameObject.SetActive(true);
            }
        }

        public void Unload()
        {
            CardName = "";
            Description = "";
            Art = null;
            art.color = Color.white;

            for (int i = 0; i < aspects.Count; i++)
            {
                aspects[i].gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
