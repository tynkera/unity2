using UnityEngine;

namespace ExpeditionCamp
{
    public class SpawnPoint : MonoBehaviour
    {
        [Min(0f)] public float spawnRadius = 1.5f;

        public Vector3 PickPosition()
        {
            float angle = GameRandom.SpawnRange(0f, Mathf.PI * 2f);
            float distance = GameRandom.SpawnRange(0f, spawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            return transform.position + offset;
        }

        public Monster FindTemplate(string prefabKey)
        {
            Monster[] placed = GetComponentsInChildren<Monster>(true);
            foreach (Monster monster in placed)
            {
                if (monster.PrefabKey == prefabKey)
                {
                    return monster;
                }
            }
            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 1f);
            DrawCircle(transform.position, spawnRadius);
        }

        public static void DrawCircle(Vector3 center, float radius)
        {
            const int segments = 40;
            Vector3 previous = center + new Vector3(radius, 0.05f, 0f);
            for (int index = 1; index <= segments; index++)
            {
                float angle = index * Mathf.PI * 2f / segments;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
