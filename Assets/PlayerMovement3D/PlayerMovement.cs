using UnityEngine;

namespace PlayerMovement3D
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 10f;
        [SerializeField] private float horizontalMouseMovement = 1f;
        [SerializeField] private float verticalMouseMovement = 1f;
        [SerializeField] private bool invertVerticalMouse;
        [SerializeField] private bool invertHorizontalMouse;

        public Camera cam;

        public void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Update()
        {
            var xMovement = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;
            var yMovement = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;

            transform.Translate(yMovement, 0, xMovement);

            var h = horizontalMouseMovement * Input.GetAxis("Mouse X") * (invertHorizontalMouse ? -1 : 1);
            var v = verticalMouseMovement * Input.GetAxis("Mouse Y") * (invertVerticalMouse ? -1 : 1);

            transform.eulerAngles += new Vector3(0, h, 0);

            if(cam.transform.eulerAngles.x + v > 90 && cam.transform.eulerAngles.x + v < 270) v = 0;
            cam.transform.eulerAngles += new Vector3(v, 0, 0);
        } 
        
        public void SetMovementSpeed(float speed) => movementSpeed = speed;

        public float GetMovementSpeed() => movementSpeed;

        public void SetHorizontalSpeed(float speed) => horizontalMouseMovement = speed;
        
        public float GetHorizontalSpeed() => horizontalMouseMovement;

        public void SetVerticalSpeed(float speed) => verticalMouseMovement = speed;
        
        public float GetVerticalSpeed() => verticalMouseMovement;

        public void SetInvertHorizontal(bool toggle) => invertHorizontalMouse = toggle;
        
        public bool GetInvertHorizontal() => invertHorizontalMouse;

        public void SetInvertVertical(bool toggle) => invertVerticalMouse = toggle;
        
        public bool GetInvertVertical() => invertVerticalMouse;
    }
}
