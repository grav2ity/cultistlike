using System;
using UnityEngine;


namespace CultistLike
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;

        public static UIManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                }

                return _instance;
            }
        }

        public CardInfo cardInfo;
        public AspectInfo aspectInfo;

        private int autoSave;

        private void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                GameManager.Instance.Pause();
            }

        #if UNITY_EDITOR
            float axis = Input.GetAxis("Mouse ScrollWheel");
            if (axis != 0f && Input.GetKey(KeyCode.LeftControl) == true)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    var normalSpeed = GameManager.Instance.normalSpeed;
                    GameManager.Instance.normalSpeed = GameManager.Instance.fastSpeed;

                    Transform objectHit = hit.transform;
                    if (objectHit.gameObject.GetComponentInParent<CardViz>() != null)
                    {
                        // Selection.activeGameObject = hit.collider.gameObject;
                        if (axis < 0f) return;

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
                        if (axis < 0f) return;

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
                        if (axis < 0f) return;

                        objectHit.gameObject.GetComponentInParent<TokenViz>().actWindow.FastForward();
                    }
                    else if (objectHit.gameObject.GetComponentInParent<ActWindow>() != null)
                    {
                        if (axis < 0f) return;

                        objectHit.gameObject.GetComponentInParent<ActWindow>().FastForward();
                    }
                    else if (objectHit.gameObject.GetComponentInParent<ArrayTable>() != null)
                    {
                        if (axis > 0f)
                            GameManager.Instance.timeSkip++;
                        else if (axis < 0f)
                            GameManager.Instance.timeSkip--;



                    }

                    GameManager.Instance.normalSpeed = normalSpeed;
                }
            }
            else if (axis != 0f && Input.GetKey(KeyCode.LeftShift) == true)
            {
                if (axis > 0f)
                    autoSave++;
                else if (axis < 0f)
                    autoSave--;

                autoSave = Math.Clamp(autoSave, 0, SaveManager.Instance.maxSaves);
                SaveManager.Instance.LoadAuto(autoSave);
            }
        #endif
        }
    }
}
