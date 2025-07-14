using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class Timer : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image image;

        [SerializeField, HideInInspector] private float _duration;
        [SerializeField, HideInInspector] private float elapsedTime;

        [SerializeField, HideInInspector] private UnityEvent timeUpEvent;

        [SerializeField, HideInInspector] private Timer following;


        public float timeLeft => following != null ? following.timeLeft : _duration - elapsedTime;

        public float duration => following != null ? following.duration : _duration;

        public void OnEnable()
        {
            image.fillAmount = 0f;
        }

        public void StartTimer(float time, UnityAction action = null)
        {
            elapsedTime = 0f;
            _duration = time;
        #if UNITY_EDITOR
            _duration = GameManager.Instance.DevTime(time);
        #endif

            enabled = true;
            timeUpEvent.RemoveAllListeners();
            timeUpEvent.AddListener(action);

            UpdateDisplay(time);
        }

        public void SetFollowing(Timer timer)
        {
            if (timer != this && timer.following != this)
            {
                following = timer;
                enabled = true;
            }
        }

        public TimerSave Save()
        {
            var save = new TimerSave
            {
                duration = duration,
                elapsedTime = elapsedTime
            };
            return save;
        }

        public void Load(TimerSave save, UnityAction action = null)
        {
            _duration = save.duration;
            elapsedTime = save.elapsedTime;

            if (duration != 0f)
            {
                enabled = true;
                timeUpEvent.AddListener(action);
            }
        }

        private void UpdateDisplay(float time)
        {
            text.text = time.ToString("0.0");
            if (duration > 0f)
            {
                image.fillAmount = 0.9f * (1f - (time / duration));
            }
        }

        private void Awake()
        {
            GetComponentInChildren<Canvas>().worldCamera = Camera.main;

            image.fillAmount = 0f;
            enabled = false;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * GameManager.Instance.timeScale;

            UpdateDisplay(timeLeft);

            if (timeLeft <= 0f)
            {
                image.fillAmount = 0.9f;
                if (following == null)
                {
                    _duration = 0f;
                    elapsedTime = 0f;
                    enabled = false;
                }
                timeUpEvent.Invoke();
            }
        }
    }

    [Serializable]
    public class TimerSave
    {
        public float duration;
        public float elapsedTime;
    }
}
