using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = FindObjectOfType<T>();
            }
            else if (Instance != FindObjectOfType<T>())
            {
                Destroy(FindObjectOfType<T>());
            }
        }
    }
}
