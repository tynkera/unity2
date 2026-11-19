using System.Collections.Generic;
using UnityEngine;

namespace ExpeditionCamp
{
    [RequireComponent(typeof(Rigidbody))]
    public class DropItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private Transform bobRoot;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 3f;

        public int ItemId { get; private set; }
        public int Count { get; private set; }

        private FieldManager _field;
        private float _lifetime;
        private Transform _magnetTarget;
        private float _magnetSpeed;
        private Rigidbody _body;
        private bool _collected;
        private Vector3 _bobBase;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _bobBase = bobRoot.localPosition;
        }

        public void Init(int itemId, int count, FieldManager owner, float lifetime)
        {
            ItemId = itemId;
            Count = count;
            _field = owner;
            _lifetime = lifetime;
            Dictionary<string, string> row = GameData.GetItem(itemId);
            icon.sprite = IconLoader.Load(DataLoader.ReadText(row, "icon_path"));
        }

        public void StartMagnet(Transform target, float speed)
        {
            _magnetTarget = target;
            _magnetSpeed = speed;
        }

        private void Update()
        {
            bobRoot.localPosition = _bobBase + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);

            if (_magnetTarget != null)
            {
                return;
            }

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                if (_field != null)
                {
                    _field.OnDropExpired(this);
                }
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (_magnetTarget == null)
            {
                return;
            }
            Vector3 goal = _magnetTarget.position + Vector3.up * 0.5f;
            _body.MovePosition(Vector3.MoveTowards(_body.position, goal, _magnetSpeed * Time.fixedDeltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || !other.CompareTag("Player"))
            {
                return;
            }
            _collected = true;
            if (_field != null)
            {
                _field.Collect(this);
            }
            Destroy(gameObject);
        }
    }
}
