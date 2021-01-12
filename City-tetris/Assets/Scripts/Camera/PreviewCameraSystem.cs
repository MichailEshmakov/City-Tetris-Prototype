using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Camera
{
    public class PreviewCameraSystem : Singleton<PreviewCameraSystem>
    {
        [SerializeField]
        private Transform modelPosition;

        private UnityEngine.Camera cam;

        private Module module;

        public Module Module
        {
            get => module;
            set
            {
                module = value;
                module.transform.position = modelPosition.position - (Vector3)FindModuleCenter(module.shape);
                module.transform.rotation = modelPosition.rotation;

                cam.orthographicSize = FindModuleLength(module.shape) / 1.5f + 1.5f;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            GameManager.OnEndLevel += OnEndLevel;
        }

        // Start is called before the first frame update
        private void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
        }

        public void OnEndLevel()
        {
            Destroy(Module.gameObject);
        }

        private float FindModuleLength(IReadOnlyList<Vector2> moduleShape)
        {
            var length = moduleShape[0].magnitude;

            return moduleShape.Select(shape => shape.magnitude).Prepend(length).Max();
        }

        private Vector2 FindModuleCenter(IReadOnlyCollection<Vector2> moduleShape)
        {
            var moduleCenter = moduleShape.Aggregate(Vector2.zero, (current, vector) => current + vector);

            moduleCenter /= moduleShape.Count;

            return moduleCenter;
        }

        private void OnDisable()
        {
            GameManager.OnEndLevel -= OnEndLevel;
        }
    }
}
