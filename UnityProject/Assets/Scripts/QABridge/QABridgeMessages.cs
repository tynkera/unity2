#if QA_BRIDGE
using System;

namespace ExpeditionCamp.QA
{
    [Serializable]
    public class HealthResponse
    {
        public bool qaBridge;
        public string scene;
        public int port;
        public bool batchMode;
    }

    [Serializable]
    public class ObjectInfo
    {
        public string name;
        public string path;
        public bool active;
        public bool interactable;
        public string text;
    }

    [Serializable]
    public class HierarchyResponse
    {
        public string scene;
        public int count;
        public ObjectInfo[] objects;
    }

    [Serializable]
    public class ObjectResponse
    {
        public bool found;
        public bool active;
        public bool interactable;
        public string text;
        public string path;
    }

    [Serializable]
    public class TextResponse
    {
        public string name;
        public string text;
    }

    [Serializable]
    public class ClickResponse
    {
        public bool clicked;
        public string name;
        public string scene;
    }

    [Serializable]
    public class ResetResponse
    {
        public bool ok;
        public string scene;
    }

    [Serializable]
    public class ScreenshotResponse
    {
        public string path;
    }

    [Serializable]
    public class ErrorResponse
    {
        public string error;
        public string detail;
    }

    [Serializable]
    public class DuplicateResponse
    {
        public string error;
        public string name;
        public int count;
        public string[] paths;
    }

    [Serializable]
    public class NameRequest
    {
        public string name;
    }

    [Serializable]
    public class PathRequest
    {
        public string path;
    }
    [Serializable]
    public class SceneResponse
    {
        public string scene;
        public int stage_id;
    }

    [Serializable]
    public class StageRequest
    {
        public int stage_id;
    }

    [Serializable]
    public class CountResponse
    {
        public string tag;
        public int count;
    }

    [Serializable]
    public class CheatRequest
    {
        public string name;
        public string value;
        public int item_id;
        public int count;
    }

    [Serializable]
    public class CheatResponse
    {
        public bool ok;
        public string name;
        public int affected;
        public string scene;
    }

    [Serializable]
    public class InventoryItem
    {
        public int item_id;
        public int count;
    }

    [Serializable]
    public class InventoryResponse
    {
        public int gold;
        public int level;
        public int exp;
        public InventoryItem[] items;
    }
    [Serializable]
    public class FieldMonster
    {
        public string name;
        public string tag;
        public float x;
        public float z;
        public float move_speed;
        public float attack_range;
        public int hp;
    }

    [Serializable]
    public class FieldResponse
    {
        public string scene;
        public int stage_id;
        public int wave;
        public int total_waves;
        public bool finished;
        public bool cleared;
        public int player_hp;
        public int player_max_hp;
        public float player_x;
        public float player_z;
        public FieldMonster[] monsters;
    }
}
#endif
