using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameObject beginningUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject overUI;

        [SerializeField] private Text overScore;
        [SerializeField] private Text gameScore;
        [SerializeField] private Text turnNumber;

        [SerializeField] private int urbanScore;

        public int Turn { get; private set; }

        public static event Action OnStartLevel;
        public static event Action OnMakeTurn;
        public static event Action OnEndLevel;
        public static ScoreManager scoreManager;

        public enum GameStatus
        {
            Start, Play, GameOver
        }

        public GameStatus CurrentState { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CurrentState = GameStatus.Start;
        }

        private void Start()
        {
            beginningUI.SetActive(true);
        }

        public void StartGame()
        {
            CurrentState = GameStatus.Play;
            OnStartLevel?.Invoke();
            scoreManager = new ScoreManager();
            beginningUI.SetActive(false);
            overUI.SetActive(false);
            gameUI.SetActive(true);
            urbanScore = 0;
            Turn = 0;

            MakeTurn(null, 0, 0, null);
        }

        public void EndGame()
        {
            overScore.text = "Кол-во заработанных очков: " + urbanScore.ToString();
            gameUI.SetActive(false);
            overUI.SetActive(true);

            CurrentState = GameStatus.GameOver;
            OnEndLevel?.Invoke();
        }

        public void MakeTurn(Grid grid, int x, int z , Module module)
        {
            OnMakeTurn?.Invoke();

            Turn++;
            if (grid != null)
                scoreManager.CountScore(grid, new Vector2Int(x,z), module);

            gameScore.text = scoreManager.Score.ToString(CultureInfo.InvariantCulture);
            turnNumber.text = Turn.ToString();
        }
    }
}
