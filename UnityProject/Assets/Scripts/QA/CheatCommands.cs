#if QA_BRIDGE
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp.QA
{
    public enum StageMoveResult
    {
        Started,
        Locked,
        NotFound,
        SceneNotInBuild
    }

    public static class CheatCommands
    {
        public static readonly string[] Names =
        {
            "add_gold", "add_item", "kill_all", "invincible", "speed", "reset_save"
        };

        public static void AddGold(int amount)
        {
            SaveSystem.EnsureLoaded();
            SaveSystem.Data.gold += amount;
            SaveSystem.Save();
            Debug.Log("[Cheat] add_gold " + amount);
        }

        public static void AddItem(int itemId, int count)
        {
            SaveSystem.EnsureLoaded();
            Inventory.AddItem(itemId, count);
            SaveSystem.Save();
            Debug.Log("[Cheat] add_item " + itemId + " x" + count);
        }

        public static int KillAll()
        {
            FieldManager field = FieldManager.Current;
            if (field == null)
            {
                Debug.Log("[Cheat] kill_all · 필드가 아니라서 처치할 몬스터가 없습니다");
                return 0;
            }
            int killed = field.KillAll();
            Debug.Log("[Cheat] kill_all " + killed);
            return killed;
        }

        public static void SetInvincible(bool value)
        {
            GameSession.Invincible = value;
            Debug.Log("[Cheat] invincible " + value);
        }

        public static void SetSpeed(float multiplier)
        {
            GameSession.SetSpeedMultiplier(multiplier);
            Debug.Log("[Cheat] speed x" + multiplier + " · Time.timeScale " + Time.timeScale);
        }

        public static void ResetSave()
        {
            SaveSystem.ResetAndSave();
            Debug.Log("[Cheat] reset_save");
        }

        public static StageMoveResult CheckStage(int stageId)
        {
            if (GameData.GetStage(stageId) == null)
            {
                return StageMoveResult.NotFound;
            }
            if (!SaveSystem.IsStageUnlocked(stageId) || !GameData.IsPlayableStage(stageId))
            {
                return StageMoveResult.Locked;
            }
            if (!Application.CanStreamedLevelBeLoaded(GameSession.GetSceneName(stageId)))
            {
                return StageMoveResult.SceneNotInBuild;
            }
            return StageMoveResult.Started;
        }

        public static AsyncOperation MoveToStage(int stageId, bool ignoreLock)
        {
            StageMoveResult check = CheckStage(stageId);
            if (check == StageMoveResult.Locked && ignoreLock)
            {
                check = StageMoveResult.Started;
            }
            if (check != StageMoveResult.Started)
            {
                Debug.Log("[Cheat] stage " + stageId + " 이동 불가 · " + check);
                return null;
            }
            return GameSession.StartStage(stageId);
        }

        public static AsyncOperation GoToLobby()
        {
            GameSession.CurrentStageId = 0;
            return SceneManager.LoadSceneAsync(GameSession.LobbyScene, LoadSceneMode.Single);
        }
    }
}
#endif
