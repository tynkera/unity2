using System.Collections;
using UnityEngine;

namespace ExpeditionCamp
{
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float flashSecond = 0.12f;

        private Vector3 _baseScale;
        private Coroutine _flash;
        private Renderer[] _meshes;
        private MaterialPropertyBlock _block;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _block = new MaterialPropertyBlock();
        }

        public void Flash()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            if (_flash != null)
            {
                StopCoroutine(_flash);
            }
            bool player=transform.parent.GetComponent<PlayerController>()!=null;
            GetComponent<CombatEffects>()?.Hit(player);
            if(player && Camera.main!=null) Camera.main.GetComponent<CameraFollow>()?.Impulse(.10f);
            _flash = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            Tint(true);
            transform.localScale = Vector3.Scale(_baseScale,new Vector3(1.10f,.88f,1.10f));
            yield return new WaitForSeconds(flashSecond);
            Tint(false);
            transform.localScale = _baseScale;
            _flash = null;
        }

        private void Tint(bool flash)
        {
            if(target != null) target.color = flash ? flashColor : Color.white;
            if (_meshes == null || _meshes.Length == 0)
            {
                _meshes = GetComponentsInChildren<MeshRenderer>();
            }
            foreach(var r in _meshes)
            {
                _block.Clear();
                if(flash) _block.SetColor("_BaseColor", flashColor);
                r.SetPropertyBlock(_block);
            }
        }

        public IEnumerator FadeOut(float second)
        {
            if (_flash != null) StopCoroutine(_flash);
            Tint(false);
            float elapsed = 0f;
            Color start = Color.white;
            while (elapsed < second)
            {
                elapsed += Time.deltaTime;
                float rate = Mathf.Clamp01(elapsed / second);
                if(target != null) target.color = new Color(start.r, start.g, start.b, 1f - rate);
                transform.localScale = _baseScale * (1f - rate * 0.6f);
                yield return null;
            }
        }
    }
}
