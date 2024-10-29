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

        [SerializeField, HideInInspector] private float startTime;
        [SerializeField, HideInInspector] private float endTime;

        [SerializeField, HideInInspector] private UnityEvent timeUpEvent;

        [SerializeField, HideInInspector] private Timer following;


        public float timeLeft
        {
            get => following != null ? following.timeLeft : endTime - GameManager.Instance.time;
        }

        public float duration
        {
            get => following != null ? following.duration : endTime - startTime;
        }

        public void OnEnable()
        {
            image.fillAmount = 0f;
        }

        public void StartTimer(float time, UnityAction action = null)
        {
            startTime = GameManager.Instance.time;
            endTime = startTime + time;
            enabled = true;
            timeUpEvent.RemoveAllListeners();
            timeUpEvent.AddListener(action);
            UpdateDisplay(duration);
        }

        public void SetFollowing(Timer timer)
        {
            if (timer != this && timer.following != this)
            {
                following = timer;
                enabled = true;
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

            startTime = endTime = 0f;
            image.fillAmount = 0f;
            enabled = false;
        }

        private void Update()
        {
            UpdateDisplay(timeLeft);

            if (timeLeft <= 0f)
            {
                startTime = endTime = 0f;
                image.fillAmount = 0.9f;
                if (following == null)
                {
                    enabled = false;
                }
                timeUpEvent.Invoke();
            }
        }
    }
}
