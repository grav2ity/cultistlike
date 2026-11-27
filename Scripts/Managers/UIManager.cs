using System;
using UnityEditor;
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

        public static void InspectorSelect(ScriptableObject so)
        {
        #if UNITY_EDITOR
                Selection.activeObject = so;
                EditorGUIUtility.PingObject(so);
        #endif
        }

        private void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                GameManager.Instance.Pause();
            }

        #if UNITY_EDITOR
            float axis = Input.GetAxis("Mouse ScrollWheel");
            bool lmButton = Input.GetMouseButtonUp(0);

            if ((axis != 0f || lmButton == true) && Input.GetKey(KeyCode.LeftControl) == true)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    var normalSpeed = GameManager.Instance.normalSpeed;
                    GameManager.Instance.normalSpeed = GameManager.Instance.fastSpeed;

                    Transform objectHit = hit.transform;
                    CardViz cardViz;
                    SlotViz slotViz;
                    TokenViz tokenViz;
                    ActWindow actWindow;

                    if ((cardViz = objectHit.gameObject.GetComponentInParent<CardViz>()) != null)
                    {
                        if (lmButton)
                            InspectorSelect(cardViz.card);
                        else if (axis > 0f)
                        {
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
                    }
                    else if ((slotViz = objectHit.gameObject.GetComponentInParent<SlotViz>()) != null)
                    {
                        if (lmButton)
                            InspectorSelect(slotViz.slot);
                        else if (axis > 0f)
                        {
                            foreach (var cV in slotViz.MatchingCards())
                            {
                                if (slotViz.slottedCard == null)
                                {
                                    var cardVizY = cV.Yield();
                                    slotViz.Grab(cardVizY, true);
                                    break;
                                }
                            }
                        }
                    }
                    else if ((tokenViz = objectHit.gameObject.GetComponentInParent<TokenViz>()) != null)
                    {
                        if (lmButton)
                            InspectorSelect(tokenViz.token);
                        else if (axis > 0f)
                            tokenViz.actWindow.FastForward();
                    }
                    else if ((actWindow = objectHit.gameObject.GetComponentInParent<ActWindow>()) != null)
                    {
                        if (lmButton)
                            InspectorSelect(actWindow.act);
                        else if (axis > 0f)
                            actWindow.FastForward();
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
            else if (axis != 0f && Input.GetKey(KeyCode.LeftAlt) == true)
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
