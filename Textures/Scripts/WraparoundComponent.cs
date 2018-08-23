using UnityEngine;

namespace Asteroids.Component
{
    public class WraparoundComponent : MonoBehaviour 
    {
        private float _cameraOrthographicSize;
        private float _screenRatio;
        private float _screenOrthographicWidth;

        private void Start()
        {
            _cameraOrthographicSize = Camera.main.orthographicSize;
            _screenRatio = Screen.width / (float)Screen.height;
            _screenOrthographicWidth = _cameraOrthographicSize * _screenRatio;
        }

        private void Update()
        {
            Wraparound();
        }

        private void Wraparound()
        {
            Vector2 position = transform.position;

            if(position.y > _cameraOrthographicSize)
            {
                position.y = -_cameraOrthographicSize;
            }

            if(position.y < -_cameraOrthographicSize)
            {
                position.y = _cameraOrthographicSize;
            }

            if (position.x > _screenOrthographicWidth)
            {
                position.x = -_screenOrthographicWidth;
            }

            if (position.x < -_screenOrthographicWidth)
            {
                position.x = _screenOrthographicWidth;
            }

            transform.position = position;
        }
    }
}
