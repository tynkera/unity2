using UnityEngine;

namespace ExpeditionCamp
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform target;

        Vector3 smoothedFocus, velocity;
        float shake, cooldown;
        public void Impulse(float strength)
        {
            if(cooldown>0)return;
            shake=Mathf.Min(strength,.12f);cooldown=.18f;
        }
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            Snap();
        }

        private void Start()
        {
            Snap();
        }

        private void LateUpdate()
        {
            if(config==null || target==null)return;
            cooldown-=Time.deltaTime;
            smoothedFocus=Vector3.SmoothDamp(smoothedFocus,target.position+Vector3.up*config.cameraLookHeight,ref velocity,.10f);
            transform.position=CalculatePosition(smoothedFocus,config);
            transform.LookAt(smoothedFocus);
            if(shake>0){transform.position+=transform.right*(Mathf.Sin(Time.time*83)*shake)+transform.up*(Mathf.Cos(Time.time*67)*shake*.5f);shake=Mathf.MoveTowards(shake,0,Time.deltaTime*.8f);}
        }

        public void Snap()
        {
            if (config == null || target == null)
            {
                return;
            }
            Vector3 focus = target.position + Vector3.up * config.cameraLookHeight;
            smoothedFocus=focus;velocity=Vector3.zero;
            transform.position = CalculatePosition(focus, config);
            transform.LookAt(focus);
        }

        public static Vector3 CalculatePosition(Vector3 focus, GameConfig config)
        {
            Quaternion rotation = Quaternion.Euler(config.cameraPitch, config.cameraYaw, 0f);
            return focus + rotation * new Vector3(0f, 0f, -config.cameraDistance);
        }
    }
}
