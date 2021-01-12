using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts
{
    public class Module : MonoBehaviour
    {
        public Renderer mainRenderer;

        public enum TypeEnum
        {
            Main,
            Residential,
            Industrial,
            Center,
            Unique
        }

        private TypeEnum type;

        public Vector2[] shape;

        [SerializeField] private GameObject[] placingMarkerQuads;

        public TypeEnum Type
        {
            get => type;
            set
            {
                type = value;
                mainRenderer.material.color = BuildingManager.Instance.typeColorDict[value];
            }
        }

        private void Start()
        {
            SetPlacingMarkerActive(false);
        }

        public void Rotate(float degrees)
        {
            var radians = Mathf.Deg2Rad * -degrees;

            for (var i = 0; i < shape.Length; i++)
            {
                var tempX = shape[i].x;
                var tempZ = shape[i].y;

                shape[i].x = (int)Mathf.Round(tempX * Mathf.Cos(radians) - tempZ * Mathf.Sin(radians));
                shape[i].y = (int)Mathf.Round(tempX * Mathf.Sin(radians) + tempZ * Mathf.Cos(radians));
            }

            var newDegree = transform.rotation.eulerAngles.y + degrees;
            transform.rotation = Quaternion.Euler(new Vector3(0, newDegree, 0));
        }

        public void MarkAvailablity(bool isAvailable)
        {
            foreach (var quad in placingMarkerQuads)
            {
                quad.GetComponent<Renderer>().material.color = isAvailable ? Color.green : Color.red;
            }
        }

        public void SetPlacingMarkerActive(bool isActive)
        {
            foreach (var quad in placingMarkerQuads)
            {
                quad.SetActive(isActive);
            }
        }
    }
}
