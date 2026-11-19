using UnityEngine;
namespace ExpeditionCamp
{
    // Presentation only: follows displacement without rotating the gameplay collider.
    public class ModelMotion : MonoBehaviour
    {
        public bool flying;
        Vector3 previous;
        float phase, strike;
        Quaternion facing = Quaternion.identity;
        void OnEnable() { previous = transform.parent.position; }
        public void Strike(Vector3 target)
        {
            Vector3 d = target - transform.parent.position; d.y = 0;
            if(d.sqrMagnitude > .001f) facing = Quaternion.LookRotation(d);
            strike = 1;
            GetComponent<CombatEffects>()?.Swing();
        }
        void LateUpdate()
        {
            Vector3 delta = transform.parent.position - previous; delta.y = 0;
            previous = transform.parent.position;
            float speed = delta.magnitude / Mathf.Max(Time.deltaTime,.001f);
            if(delta.sqrMagnitude > .0001f) facing = Quaternion.LookRotation(delta);
            phase += Time.deltaTime * (speed > .2f ? 13 : 2);
            strike = Mathf.MoveTowards(strike,0,Time.deltaTime * 6);
            float bob = flying ? Mathf.Sin(phase)*.1f : Mathf.Abs(Mathf.Sin(phase))*Mathf.Min(speed*.025f,.10f);
            transform.localPosition = Vector3.up * bob + facing * Vector3.forward * (Mathf.Sin(strike*Mathf.PI)*.32f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, facing * Quaternion.Euler(strike* -18,0,Mathf.Sin(phase)*Mathf.Min(speed,3)),Time.deltaTime*14);
        }
    }
}
