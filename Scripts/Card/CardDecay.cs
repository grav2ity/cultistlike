using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class CardDecay : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private SpriteRenderer art;

        [SerializeField, HideInInspector] private float decayTime;

        [SerializeField, HideInInspector] private float startTime;
        [SerializeField, HideInInspector] private float endTime;

        [SerializeField, HideInInspector] private UnityEvent timeUpEvent;

        private Renderer timerRenderer;

        [SerializeField, HideInInspector] private Color originalColor;


        public float timeLeft
        {
            get => endTime - GameManager.Instance.time;
        }

        public float duration
        {
            get => endTime - startTime;
        }

        public void StartTimer(float time, UnityAction action = null)
        {
            decayTime = time;
            timerRenderer.enabled = false;
            startTime = GameManager.Instance.time;
            endTime = startTime + time;
            enabled = true;
            timeUpEvent.RemoveAllListeners();

            originalColor = art.color;

            if (action != null)
            {
                timeUpEvent.AddListener(action);
            }
            UpdateDisplay(duration);
        }

        private void UpdateDisplay(float time)
        {
            if (timerRenderer.enabled == false && (2f * timeLeft) < decayTime)
            {
                ShowTimer();
            }
            text.text = time.ToString("0.0");
        }

        public void ShowTimer()
        {
            art.color = Color.grey;
            timerRenderer.enabled = true;
        }

        public void HideTimer()
        {
            art.color = originalColor;
            timerRenderer.enabled = false;
        }

        private void Awake()
        {
            originalColor = art.color;

            timerRenderer = text.GetComponent<MeshRenderer>();
            startTime = endTime = 0f;
        }

        private void Start()
        {
            timerRenderer.enabled = false;
        }

        private void Update()
        {
            UpdateDisplay(timeLeft);

            if (timeLeft <= 0f)
            {
                startTime = endTime = 0f;
                enabled = false;
                timeUpEvent.Invoke();
                art.color = originalColor;
                HideTimer();
            }
        }
    }
}
