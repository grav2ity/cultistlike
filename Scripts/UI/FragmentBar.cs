using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class FragmentBar : MonoBehaviour
    {
        private List<AspectViz> aspects = new List<AspectViz>();


        public void Load(FragContainer fragments)
        {
            if (fragments == null)
            {
                Unload();
            }
            else
            {
                gameObject.SetActive(true);

                ActivateVizs(fragments.aspects.Count);

                for (int i = 0; i < fragments.aspects.Count; i++)
                {
                    aspects[i].LoadAspect(fragments.aspects[i]);
                    aspects[i].gameObject.SetActive(true);
                }
            }
        }

        public void Unload()
        {
            for (int i = 0; i < aspects.Count; i++)
            {
                aspects[i].gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void ActivateVizs(int count)
        {
            if (count > aspects.Count)
            {
                while (count > aspects.Count)
                {
                    var aspect = Instantiate(GameManager.Instance.aspectPrefab, transform);
                    aspects.Add(aspect);
                }

            }
            else if (count < aspects.Count)
            {
                for (int i = aspects.Count; i > count; i--)
                {
                    aspects[i - 1].gameObject.SetActive(false);
                }
            }
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
