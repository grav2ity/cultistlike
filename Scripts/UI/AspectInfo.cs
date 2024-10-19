using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class AspectInfo : MonoBehaviour
    {
        [Header("Layout")]
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


        public void LoadAspect(Aspect aspect)
        {
            gameObject.SetActive(true);

            AspectName = aspect.label;
            Description = aspect.description;

            if (aspect.art != null)
            {
                art.sprite = aspect.art;
                art.color = Color.white;
            }
            else
            {
                art.sprite = null;
                art.color = aspect.color;
            }
        }

        public void Unload()
        {
            AspectName = "";
            Description = "";
            art.sprite = null;
            art.color = Color.white;

            gameObject.SetActive(false);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
