using UnityEngine;

namespace ExpeditionCamp
{
    public class Steering
    {
        private const float CheckInterval = 0.4f;
        private const float DetourSecond = 0.8f;
        private const float DetourAngle = 75f;
        private const float StuckRate = 0.25f;

        private float _nextCheck;
        private Vector3 _lastPosition;
        private float _detourUntil;
        private float _sign = 1f;
        private bool _tracking;

        public Vector3 Adjust(Vector3 desired, Vector3 position, float speed)
        {
            float now = Time.time;
            if (!_tracking)
            {
                _tracking = true;
                _lastPosition = position;
                _nextCheck = now + CheckInterval;
            }
            else if (now >= _nextCheck)
            {
                Vector3 moved = position - _lastPosition;
                moved.y = 0f;
                bool stuck = moved.magnitude < speed * CheckInterval * StuckRate;
                if (stuck && now >= _detourUntil)
                {
                    _sign = -_sign;
                    _detourUntil = now + DetourSecond;
                }
                _lastPosition = position;
                _nextCheck = now + CheckInterval;
            }

            if (now < _detourUntil)
            {
                return Quaternion.Euler(0f, DetourAngle * _sign, 0f) * desired;
            }
            return desired;
        }

        public void Stop()
        {
            _tracking = false;
        }
    }
}
