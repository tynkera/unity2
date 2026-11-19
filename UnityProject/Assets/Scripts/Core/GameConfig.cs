using UnityEngine;

namespace ExpeditionCamp
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ExpeditionCamp/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game")]
        [Min(0f)] public float timeScale = 1f;

        [Header("Field Camera")]
        [Min(1f)] public float cameraDistance = 16f;
        [Range(10f, 89f)] public float cameraPitch = 50f;
        public float cameraYaw = 45f;
        public float cameraLookHeight = 1f;

        [Header("Field")]
        [Min(1f)] public float dropItemLifetime = 30f;
        [Min(0f)] public float waveInterval = 1.5f;
        [Min(0f)] public float dropMagnetSpeed = 12f;
    }
}
