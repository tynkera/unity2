using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExpeditionCamp
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Min(0f)] public float moveSpeed = 5f;
        [Min(0.05f)] public float attackInterval = 0.6f;
        [Min(0f)] public float attackRange = 2.2f;

        [SerializeField] private HitFeedback visual;
        [SerializeField] private Transform attackRangeView;

        public int MaxHp { get; private set; }
        public int Hp { get; private set; }
        public int Attack { get; private set; }
        public bool IsDead { get; private set; }
        public bool AutoBattle { get; set; } = true;

        public event Action HpChanged;

        private CharacterController _controller;
        private FieldManager _field;
        private float _attackTimer;
        private readonly Steering _steering = new Steering();

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            MaxHp = GameData.GetConstInt("PLAYER_MAX_HP", 1000);
            Attack = GameData.GetConstInt("PLAYER_ATK", 80);
            Hp = MaxHp;
            if (attackRangeView != null)
            {
                attackRangeView.localScale = new Vector3(attackRange * 2f, attackRange * 2f, 1f);
            }
        }

        public void Bind(FieldManager field)
        {
            _field = field;
        }

        public void Teleport(Vector3 position)
        {
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = true;
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            Monster target = FindNearestMonster();
            float targetDistance = target != null ? FlatDistance(target.transform.position) : float.MaxValue;

            Vector3 input = ReadMoveInput();
            if (input.sqrMagnitude > 0.01f)
            {
                _steering.Stop();
                _controller.SimpleMove(input.normalized * moveSpeed);
            }
            else if (AutoBattle && target != null && targetDistance > attackRange * 0.9f)
            {
                Vector3 direction = target.transform.position - transform.position;
                direction.y = 0f;
                direction = _steering.Adjust(direction.normalized, transform.position, moveSpeed);
                _controller.SimpleMove(direction * moveSpeed);
            }
            else
            {
                _steering.Stop();
                _controller.SimpleMove(Vector3.zero);
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f && target != null && targetDistance <= attackRange)
            {
                _attackTimer = attackInterval;
                visual.GetComponent<ModelMotion>()?.Strike(target.transform.position);
                target.TakeDamage(Attack);
            }
        }

        private Monster FindNearestMonster()
        {
            GameObject[] candidates = GameObject.FindGameObjectsWithTag("Monster");
            Monster nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (GameObject candidate in candidates)
            {
                Monster monster = candidate.GetComponent<Monster>();
                if (monster == null || monster.IsDead || !monster.IsSpawned)
                {
                    continue;
                }
                float distance = FlatDistance(candidate.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = monster;
                }
            }
            return nearest;
        }

        private float FlatDistance(Vector3 position)
        {
            Vector3 offset = position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private Vector3 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector3.zero;
            }
            float x = 0f;
            float z = 0f;
            if (keyboard.aKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed) z -= 1f;
            if (keyboard.wKey.isPressed) z += 1f;
            if (x == 0f && z == 0f)
            {
                return Vector3.zero;
            }
            float yaw = GameSession.Config != null ? GameSession.Config.cameraYaw : 0f;
            return Quaternion.Euler(0f, yaw, 0f) * new Vector3(x, 0f, z);
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || GameSession.Invincible)
            {
                return;
            }
            Hp = Mathf.Max(0, Hp - damage);
            visual.Flash();
            if (HpChanged != null)
            {
                HpChanged();
            }
            if (Hp <= 0)
            {
                IsDead = true;
                Debug.Log("[Field] 플레이어 체력이 0 이 되었습니다");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 1f);
            SpawnPoint.DrawCircle(transform.position, attackRange);
        }
    }
}
