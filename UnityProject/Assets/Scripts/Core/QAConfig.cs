using UnityEngine;

namespace ExpeditionCamp
{
    [CreateAssetMenu(fileName = "QAConfig", menuName = "ExpeditionCamp/QA Config")]
    public class QAConfig : ScriptableObject
    {
        public int randomSeed = 0;
        public bool showDebugOnStart = false;
    }
}
