using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class AspectInfo : MonoBehaviour
    {
        [Header("Card Info")]
        [SerializeField] private Image art;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI aspectName;


        public string Description {
            get { return description.text; }
            set { description.text = value; }
        }

        public string AspectName {
            get { return aspectName.text; }
            set { aspectName.text = value; }
        }

        public Sprite Art {
            get { return art.sprite; }
            set { art.sprite = value; }
        }


        public void LoadAspect(Aspect aspect)
        {
            gameObject.SetActive(true);

            AspectName = aspect.aspectName;
            Description = aspect.text;

            if (aspect.art != null)
            {
                Art = aspect.art;
            }
            else
            {
                art.color = aspect.color;
            }
        }

        public void Unload()
        {
            AspectName = "";
            Description = "";
            Art = null;
            art.color = Color.white;

            gameObject.SetActive(false);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
