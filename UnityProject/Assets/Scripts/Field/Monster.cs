using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    [RequireComponent(typeof(CharacterController))]
    public class Monster : MonoBehaviour
    {
        [SerializeField] private string prefabKey;
        [Min(0f)] public float moveSpeed = 2f;
        [Min(0f)] public float attackRange = 1.6f;
        [Min(0.1f)] public float attackInterval = 1.5f;
        [Min(0f)] public float hpBarHeight = 2.4f;

        [SerializeField] private HitFeedback visual;
        [SerializeField] private Transform hpBar;
        [SerializeField] private Image hpFill;

        public int MonsterId { get; private set; }
        public int MaxHp { get; private set; }
        public int Hp { get; private set; }
        public int Attack { get; private set; }
        public int DropGroupId { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsSpawned { get; private set; }

        public string PrefabKey
        {
            get { return prefabKey; }
        }

        private CharacterController _controller;
        private FieldManager _field;
        private float _attackTimer;
        private readonly Steering _steering = new Steering();

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (hpBar != null)
            {
                hpBar.localPosition = new Vector3(0f, hpBarHeight, 0f);
            }
        }

        public void Init(int monsterId, FieldManager owner)
        {
            Dictionary<string, string> row = GameData.GetMonster(monsterId);
            MonsterId = monsterId;
            MaxHp = DataLoader.ReadInt(row, "base_hp", 1);
            Hp = MaxHp;
            Attack = DataLoader.ReadInt(row, "base_atk", 0);
            DropGroupId = DataLoader.ReadInt(row, "drop_group_id", 0);
            _field = owner;
            _attackTimer = attackInterval;
            IsSpawned = true;
            UpdateBar();
        }

        private void Update()
        {
            if (IsDead || _field == null)
            {
                return;
            }

            PlayerController player = _field.Player;
            if (player == null || player.IsDead)
            {
                _controller.SimpleMove(Vector3.zero);
                return;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance > attackRange)
            {
                Vector3 direction = _steering.Adjust(toPlayer / distance, transform.position, moveSpeed);
                _controller.SimpleMove(direction * moveSpeed);
                return;
            }

            _steering.Stop();
            _controller.SimpleMove(Vector3.zero);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackTimer = attackInterval;
                visual.GetComponent<ModelMotion>()?.Strike(player.transform.position);
                player.TakeDamage(Attack);
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
            {
                return;
            }
            Hp = Mathf.Max(0, Hp - damage);
            visual.Flash();
            UpdateBar();
            if (Hp <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            if (IsDead)
            {
                return;
            }
            IsDead = true;
            Hp = 0;
            gameObject.tag = "Untagged";
            _controller.enabled = false;
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
            if (_field != null)
            {
                _field.OnMonsterKilled(this);
            }
            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            yield return visual.FadeOut(0.4f);
            Destroy(gameObject);
        }

        private void UpdateBar()
        {
            if (hpFill != null && MaxHp > 0)
            {
                hpFill.fillAmount = (float)Hp / MaxHp;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
            SpawnPoint.DrawCircle(transform.position, attackRange);
        }
    }
}
