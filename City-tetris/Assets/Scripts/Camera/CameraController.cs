using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Camera
{
    public class CameraController : Singleton<CameraController>
    {
        public Vector3 upperLimit;
        public Vector3 lowerLimit;

        public UnityEngine.Camera cam;

        public float movementSpeed;

        private Vector3 position;
        private Vector3 startPosition;

        private readonly Vector3 verticalVec = new Vector3(1, 0, 1);
        private readonly Vector3 horizontalVec = new Vector3(1, 0, -1);

        protected override void Awake()
        {
            base.Awake();
            GameManager.OnStartLevel += OnStartLevel;
        }

        private void Update()
        {
            position = transform.position;
            // TODO: Убрать управление камерой
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            position += (horizontalVec * horizontal + verticalVec * vertical) * movementSpeed * Time.deltaTime;

            position.x = Mathf.Clamp(position.x, lowerLimit.x, upperLimit.x);
            position.z = Mathf.Clamp(position.z, lowerLimit.z, upperLimit.z);
            // TODO limit the camera on x,z axis according to ground size

            transform.position = position;
        }

        private void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
            startPosition = transform.position;
        }

        private void OnStartLevel()
        {
            ResetCamera();
        }

        public void ResetCamera()
        {
            cam.orthographicSize = 7;
            transform.position = startPosition;
        }

        public void ZoomOut(int i)
        {
            GetComponent<UnityEngine.Camera>().orthographicSize += i;
        }

        private void OnDisable()
        {
            GameManager.OnStartLevel -= OnStartLevel;
        }
    }
}