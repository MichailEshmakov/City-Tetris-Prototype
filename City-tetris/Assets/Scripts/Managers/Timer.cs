using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class Timer : Singleton<Timer>
    {
        [SerializeField] private Slider slider;// TODO: Сделать картинку место слайдера

        [SerializeField] private float gameTime;
        [SerializeField] private bool isActive;

        [SerializeField] private float speed;
        [SerializeField] private const float SpeedIncr = 0.05f;

        [SerializeField] private const float TurnDuration = 20;

        protected override void Awake()
        {
            base.Awake();
            GameManager.OnStartLevel += OnStartLevel;
            GameManager.OnMakeTurn += OnMakeTurn;
            GameManager.OnEndLevel += OnEndLevel;
        }

        private void Start()
        {
            isActive = false;
            slider.maxValue = TurnDuration;
            slider.value = TurnDuration;
        }

        private void Update()
        {
            if (!isActive) return;
            slider.value -= speed * Time.deltaTime;

            if (!(slider.value <= 0)) return;
            isActive = false;
            GameManager.Instance.EndGame();
        }

        private void OnStartLevel()
        {
            ResetTimer();
        }

        private void OnMakeTurn()
        {
            UpdateTimer();

            if (GameManager.Instance.Turn == 1)
            {
                isActive = true;
            }
        }

        private void OnEndLevel()
        {
            isActive = false;
        }

        public void ResetTimer()
        {
            slider.value = TurnDuration;
        }

        public void UpdateTimer()
        {
            slider.value = TurnDuration;
            speed += SpeedIncr;
        }

        private void OnDisable()
        {
            GameManager.OnStartLevel -= OnStartLevel;
            GameManager.OnMakeTurn -= OnMakeTurn;
            GameManager.OnEndLevel -= OnEndLevel;
        }
    }
}
