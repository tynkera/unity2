using UnityEngine;

namespace ExpeditionCamp
{
    [DefaultExecutionOrder(-1000)]
    public class GameContext : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private QAConfig qaConfig;

        public static QAConfig QA { get; private set; }

        private void Awake()
        {
            if (qaConfig != null)
            {
                QA = qaConfig;
            }
            if (!GameRandom.IsInitialized)
            {
                GameRandom.Initialize(qaConfig != null ? qaConfig.randomSeed : 0);
                Debug.Log("[Random] seed " + GameRandom.Seed + (GameRandom.IsFixed ? " (고정)" : " (고정 X)"));
            }
            GameSession.SetConfig(gameConfig);
            GameData.Load();
            if (GameData.LoadError == null)
            {
                SaveSystem.EnsureLoaded();
            }
        }
    }
}
