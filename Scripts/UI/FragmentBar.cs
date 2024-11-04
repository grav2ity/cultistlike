using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class FragmentBar : MonoBehaviour
    {
        [Header("Layout")]
        public GameObject horizontalGO;
        public GameObject verticalGO;

        [Header("Options")]
        public bool vertical;
        public bool showAspects;
        public bool showCards;
        public bool showSpecial;

        [Header("Special fragments")]
        public Aspect allowed;
        public Aspect forbidden;


        [SerializeField, HideInInspector] private List<FragmentViz> fragVizs;
        [SerializeField, HideInInspector] private int index;


        public void Load(FragContainer fragments)
        {
            Unload();
            if (fragments != null)
            {
                index = 0;
                gameObject.SetActive(true);

                if (showCards == true)
                {
                    foreach (var card in fragments.cards)
                    {
                        Load(card);
                    }
                }
                if (showAspects == true)
                {
                    foreach (var frag in fragments.fragments)
                    {
                        Load(frag);
                    }
                }
            }
        }

        public void Load(Slot slot)
        {
            Unload();
            if (slot != null)
            {
                index = 0;
                gameObject.SetActive(true);

                if (slot.required.Count > 0 && showSpecial == true)
                {
                    Load(allowed);
                }
                foreach (var frag in slot.required)
                {
                    Load(frag);
                }
                if (slot.forbidden.Count > 0 && showSpecial == true)
                {
                    Load(forbidden);
                }
                foreach (var frag in slot.forbidden)
                {
                    Load(frag);
                }
            }
        }

        public void Unload()
        {
            for (int i = 0; i < fragVizs.Count; i++)
            {
                fragVizs[i].gameObject.SetActive(false);
            }

            index = 0;
            gameObject.SetActive(false);
        }

        private void Load<T>(T frag) where T : IFrag
        {
            if (index < fragVizs.Count)
            {
                fragVizs[index].Load<T>(frag);
                fragVizs[index].gameObject.SetActive(true);
                index++;
            }
            else
            {
                var fragViz = Instantiate(GameManager.Instance.fragmentPrefab,
                                          vertical == true ? verticalGO.transform : horizontalGO.transform);
                fragVizs.Add(fragViz);
                Load<T>(frag);
            }
        }

        private void Load(CardViz frag)
        {
            if (index < fragVizs.Count)
            {
                fragVizs[index].Load(frag);
                fragVizs[index].gameObject.SetActive(true);
                index++;
            }
            else
            {
                var fragViz = Instantiate(GameManager.Instance.fragmentPrefab,
                                          vertical == true ? verticalGO.transform : horizontalGO.transform);
                fragVizs.Add(fragViz);
                Load(frag);
            }
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
