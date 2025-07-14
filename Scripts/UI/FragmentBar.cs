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
        [Tooltip("Auto reflects content of the nearest FragTree (searching upwards in transform hierarchy).")]
        public bool autoUpdate;
        public bool vertical;
        public bool showAspects;
        public bool showCards;
        public bool showSpecial;
        public bool showHidden;

        [Header("Special fragments")]
        public Aspect allowed;
        public Aspect forbidden;
        public Card drawn;


        [SerializeField, HideInInspector] private List<FragmentViz> fragVizs;
        [SerializeField, HideInInspector] private int index;


        public void Load(FragTree fragments)
        {
            Unload();
            if (fragments != null)
            {
                index = 0;

                if (showCards)
                {
                    foreach (var card in fragments.cards)
                    {
                        Load(card);
                    }
                }
                if (showAspects)
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

                if (slot.required.Count > 0 && showSpecial)
                {
                    Load(allowed);
                }
                foreach (var frag in slot.required)
                {
                    Load(frag);
                }
                if (slot.forbidden.Count > 0 && showSpecial)
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
            foreach (var t in fragVizs)
            {
                t.gameObject.SetActive(false);
            }

            index = 0;
        }

        private void Load<T>(T frag) where T : IFrag
        {
            if (showHidden == false && frag.Hidden())
                return;

            if (index < fragVizs.Count)
            {
                fragVizs[index].Load(frag);
                fragVizs[index].gameObject.SetActive(true);
                index++;
            }
            else
            {
                var fragViz = Instantiate(GameManager.Instance.fragmentPrefab,
                                          vertical ? verticalGO.transform : horizontalGO.transform);
                fragVizs.Add(fragViz);
                Load(frag);
            }
        }

        private void Load(CardViz cardViz)
        {
            if (showHidden == false && cardViz.card.Hidden())
                return;

            if (index < fragVizs.Count)
            {
                if (cardViz.faceDown)
                {
                    fragVizs[index].Load(drawn);
                }
                else
                {
                    fragVizs[index].Load(cardViz);
                }
                fragVizs[index].gameObject.SetActive(true);
                index++;
            }
            else
            {
                var fragViz = Instantiate(GameManager.Instance.fragmentPrefab,
                                          vertical ? verticalGO.transform : horizontalGO.transform);
                fragVizs.Add(fragViz);
                Load(cardViz);
            }
        }

        private void Awake()
        {
            if (autoUpdate)
            {
                var fragTree = transform.GetComponentInNearestParent<FragTree>();
                if (fragTree != null)
                {
                    fragTree.ChangeEvent += () => Load(fragTree);
                }
            }
        }

        //TODO
        private void Start()
        {
            if (autoUpdate)
            {
                var fragTree = transform.GetComponentInNearestParent<FragTree>();
                if (fragTree != null)
                {
                    Load(fragTree);
                }
            }
        }
    }
}
