using UnityEngine;


namespace CultistLike
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public CardInfo cardInfo;
        public AspectInfo aspectInfo;

        private void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                GameManager.Instance.Pause();
            }

        #if UNITY_EDITOR
            if (Input.GetAxis("Mouse ScrollWheel") > 0f && Input.GetKey(KeyCode.LeftControl) == true)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;
                    if (objectHit.gameObject.GetComponentInParent<CardViz>() != null)
                    {
                        var cardViz = objectHit.gameObject.GetComponentInParent<CardViz>();
                        if (cardViz.interactive == true)
                        {
                            if (cardViz.decay.enabled == true)
                            {
                                cardViz.decay.TimeUp();
                            }
                            else
                            {
                                cardViz.DoubleClick();
                            }
                        }
                    }
                    else if (objectHit.gameObject.GetComponentInParent<SlotViz>() != null)
                    {
                        var slotViz = objectHit.gameObject.GetComponentInParent<SlotViz>();
                        foreach (var cardViz in slotViz.MatchingCards())
                        {
                            if (slotViz.slottedCard == null)
                            {
                                var cardVizY = cardViz.Yield();
                                slotViz.Grab(cardVizY, true);
                                break;
                            }
                        }
                    }
                    else if (objectHit.gameObject.GetComponentInParent<TokenViz>() != null)
                    {
                        objectHit.gameObject.GetComponentInParent<TokenViz>().actWindow.FastForward();
                    }
                    else if (objectHit.gameObject.GetComponentInParent<ActWindow>() != null)
                    {
                        objectHit.gameObject.GetComponentInParent<ActWindow>().FastForward();
                    }
                }
            }
        #endif

        }

        private void Awake()
        {
            Instance = this;
        }
    }
}
