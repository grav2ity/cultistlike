using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class CardInfo : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private GameObject aspectsGO;
        [SerializeField] private FragmentBar fragmentBar;


        private List<AspectViz> aspects = new List<AspectViz>();

        public string Description {
            get { return description.text; }
            set { description.text = value; }
        }

        public string CardName {
            get { return cardName.text; }
            set { cardName.text = value; }
        }


        public void LoadCard(CardViz cardViz)
        {
            gameObject.SetActive(true);

            CardName = cardViz.card.label;
            Description = cardViz.card.description;

            if (cardViz.card.art != null)
            {
                art.sprite = cardViz.card.art;
                art.color = Color.white;
            }
            else
            {
                art.sprite = null;
                art.color = cardViz.card.color;
            }

            fragmentBar.Load(cardViz.fragments);
        }

        public void Unload()
        {
            CardName = "";
            Description = "";
            art.sprite = null;
            art.color = Color.white;

            fragmentBar.Unload();

            gameObject.SetActive(false);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
