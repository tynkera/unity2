using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp
{
    public static class GameSession
    {
        public const string BootScene = "Boot";
        public const string LobbyScene = "Lobby";

        public static int CurrentStageId { get; set; }
        public static bool Invincible { get; set; }
        public static float SpeedMultiplier { get; private set; } = 1f;

        private static GameConfig _config;

        public static GameConfig Config
        {
            get { return _config; }
        }

        public static void SetConfig(GameConfig config)
        {
            if (config != null)
            {
                _config = config;
            }
            ApplyTimeScale();
        }

        public static void SetSpeedMultiplier(float multiplier)
        {
            SpeedMultiplier = multiplier;
            ApplyTimeScale();
        }

        public static void ApplyTimeScale()
        {
            float baseScale = _config != null ? _config.timeScale : 1f;
            Time.timeScale = baseScale * SpeedMultiplier;
        }

        public static string GetSceneName(int stageId)
        {
            return DataLoader.ReadText(GameData.GetStage(stageId), "scene_name");
        }

        public static AsyncOperation StartStage(int stageId)
        {
            CurrentStageId = stageId;
            string sceneName = GetSceneName(stageId);
            Debug.Log("[Stage] " + stageId + " 스테이지를 시작합니다: " + sceneName);
            return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        public static void ResetSession()
        {
            CurrentStageId = 0;
            Invincible = false;
            SpeedMultiplier = 1f;
            ApplyTimeScale();
        }
    }
}
