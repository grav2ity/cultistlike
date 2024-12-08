using System;

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

        [SerializeField, HideInInspector] private Card decayTo;

        [SerializeField, HideInInspector] private bool paused;

        [SerializeField, HideInInspector] private float decayTime;
        [SerializeField, HideInInspector] private float elapsedTime;

        [SerializeField, HideInInspector] private Color originalColor;

        private Renderer timerRenderer;


        public float timeLeft
        {
            get => decayTime - elapsedTime;
        }


        public void StartTimer(float time, Card decayTo)
        {
            elapsedTime = 0f;
            decayTime = time;
            this.decayTo = decayTo;

            timerRenderer.enabled = false;
            enabled = true;

            originalColor = art.color;

            UpdateDisplay(time);
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

        public CardDecaySave Save()
        {
            var save = new CardDecaySave();
            save.duration = decayTime;
            save.elapsedTime = elapsedTime;
            save.decayTo = decayTo;
            save.paused = paused;
            return save;
        }

        public void Load(CardDecaySave save)
        {
            decayTime = save.duration;
            elapsedTime = save.elapsedTime;
            decayTo = save.decayTo;

            if (decayTime != 0f)
            {
                enabled = true;
            }
            if (save.paused == true)
            {
                Pause();
            }
        }

        private void UpdateDisplay(float time)
        {
            if (timerRenderer.enabled == false && (2f * timeLeft) < decayTime)
            {
                ShowTimer();
            }
            text.text = time.ToString("0.0");
        }

        private void Awake()
        {
            originalColor = art.color;
            timerRenderer = text.GetComponent<MeshRenderer>();
            timerRenderer.enabled = false;
            enabled = false;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * GameManager.Instance.timeScale;

            UpdateDisplay(timeLeft);

            if (timeLeft <= 0f)
            {
                decayTime = 0f;
                elapsedTime = 0f;
                enabled = false;
                GetComponent<CardViz>()?.OnDecayComplete(decayTo);
                art.color = originalColor;
                HideTimer();
            }
        }
    }

    [Serializable]
    public class CardDecaySave
    {
        public float duration;
        public float elapsedTime;
        public Card decayTo;
        public bool paused;
    }
}
