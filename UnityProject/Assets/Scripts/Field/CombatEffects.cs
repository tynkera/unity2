using UnityEngine;
namespace ExpeditionCamp
{
    public class CombatEffects : MonoBehaviour
    {
        [SerializeField] ParticleSystem sparks;
        [SerializeField] LineRenderer slash;
        float life;
        public void Hit(bool player)
        {
            var emit=new ParticleSystem.EmitParams();emit.startColor=player?new Color(1,.35f,.15f):new Color(1,.85f,.4f);
            sparks.Play();
            sparks.Emit(emit,player?9:6);
        }
        public void Swing()
        {
            life=.16f;slash.enabled=true;
        }
        void LateUpdate()
        {
            if(life<=0)return;
            life-=Time.deltaTime;float t=1-Mathf.Clamp01(life/.16f);
            slash.widthMultiplier=.1f*(1-t);
            for(int i=0;i<12;i++)
            {
                float a=Mathf.Lerp(-65,65,i/11f)*Mathf.Deg2Rad;
                slash.SetPosition(i,new Vector3(Mathf.Sin(a)*1.25f,.8f+Mathf.Sin(i/11f*Mathf.PI)*.15f,Mathf.Cos(a)*1.25f));
            }
            if(life<=0)slash.enabled=false;
        }
    }
}
