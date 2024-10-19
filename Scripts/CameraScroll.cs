using UnityEngine;

namespace CultistLike
{
    public class CameraScroll : MonoBehaviour
    {
        public float horizontalSpeed = 1.0f;
        public float verticalSpeed = 1.0f;
        public float zoomSpeed = 4.0f;


        private void Update()
        {
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");

            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                transform.Translate(new Vector3(-h, -v, 0));
            }

            float z = zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
            transform.Translate(new Vector3(0, 0, z));
        }
    }
}