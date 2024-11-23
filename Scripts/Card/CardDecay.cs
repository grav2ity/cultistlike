using UnityEngine;
using UnityEngine.Events;

using TMPro;


namespace CultistLike
{
    public class CardDecay : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private SpriteRenderer art;

        [Header("Options")]
        public bool pauseOnHide;

        [SerializeField, HideInInspector] private bool paused;

        [SerializeField, HideInInspector] private float decayTime;
        [SerializeField, HideInInspector] private float elapsedTime;

        [SerializeField, HideInInspector] private UnityEvent timeUpEvent;

        [SerializeField, HideInInspector] private Color originalColor;

        private Renderer timerRenderer;


        public float timeLeft
        {
            get => decayTime - elapsedTime;
        }


        public void StartTimer(float time, UnityAction action = null)
        {
            elapsedTime = 0f;
            decayTime = time;

            timerRenderer.enabled = false;
            enabled = true;
            timeUpEvent.RemoveAllListeners();

            originalColor = art.color;

            if (action != null)
            {
                timeUpEvent.AddListener(action);
            }
            UpdateDisplay(time);
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

        public void Pause()
        {
            if (enabled == true)
            {
                paused = true;
                enabled = false;
            }
        }

        public void Unpause()
        {
            if (paused == true)
            {
                paused = false;
                enabled = true;
            }
        }

        private void Awake()
        {
            originalColor = art.color;
            timerRenderer = text.GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            timerRenderer.enabled = false;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * GameManager.Instance.timeScale;

            UpdateDisplay(timeLeft);

            if (timeLeft <= 0f)
            {
                enabled = false;
                timeUpEvent.Invoke();
                art.color = originalColor;
                HideTimer();
            }
        }
    }
}
