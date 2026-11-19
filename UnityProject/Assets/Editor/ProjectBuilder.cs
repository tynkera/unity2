using UnityEditor;
using UnityEngine;

namespace ExpeditionCamp.EditorTools
{
    public static class ProjectBuilder
    {
        [MenuItem("ExpeditionCamp/Rebuild All")]
        public static void BuildAll()
        {
            AssetDatabase.Refresh();
            ProjectSetup.Run();
            PrefabBuilder.BuildAll();
            GameSceneBuilder.BuildAll();
            VisualRedesign.ApplyScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProjectBuilder] 설정 · 프리팹 · 게임 씬 4개 · 연습 씬 6개를 만들었습니다");
        }
    }
}
