using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Managers
{
    public class BuildingManager : Singleton<BuildingManager>
    {
        [SerializeField] Module mainModulePrefab;
        [SerializeField] Module[] modulePrefabs;
        [SerializeField] Obstacle obstacle;

        public GameObject debugUnactive;

        public List<Module> placedMods;
        public List<Obstacle> placedObstacles;

        private Grid grid;

        private GameObject[,] activeMarkers;
        private int activeRingsAmount;

        private bool isKeepingModuleInMouse;

        // Оставил в виде массива, а не формулы, чтобы было проще кастомизировать значения.
        private readonly int[] expandValues = new int[13] { 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        private int currentExpandCount;

        private readonly int[] zoomValues = new int[13] { 6, 7, 8, 12, 25, 50, 50, 50, 50, 50, 50, 50, 50 };
        private int currentZoomCount;

        public const int GridSize = 99;
        [SerializeField] const int StartingRingsAmount = 2;
        public float zoomCoefficient = 0.666f;

        [SerializeField] readonly float residentialProbability = 0.3f;
        [SerializeField] readonly float industrialProbability = 0.3f;
        [SerializeField] readonly float centerProbability = 0.3f;
        [SerializeField] readonly float uniqueProbability = 0.1f;

        public Material defaultModuleMaterial;
        public Color MainColor;
        public Color ResidentialColor;
        public Color IndustrialColor;
        public Color CenterColor;
        public Color UniqueColor;

        // TODO: Подумать над тем, как еще можно связать воедино поля типа, вероятности и цвета
        private Dictionary<Module.TypeEnum, float> typeProbabilityDict;
        public Dictionary<Module.TypeEnum, Color> typeColorDict;

        [SerializeField] private PreviewCameraSystem previewCameraSystem;
        [SerializeField] private PreviewCameraSystem deliveryCameraSystem;

        [SerializeField] private Toggle tetrisToggle;

        protected override void Awake()
        {
            base.Awake();

            GameManager.OnStartLevel += OnStartLevel;

            activeRingsAmount = StartingRingsAmount;
            NormalizeProbability();

            typeProbabilityDict = new Dictionary<Module.TypeEnum, float>
            {
                {Module.TypeEnum.Residential, residentialProbability},
                {Module.TypeEnum.Industrial, industrialProbability},
                {Module.TypeEnum.Center, centerProbability},
                {Module.TypeEnum.Unique, uniqueProbability}
            };

            typeColorDict = new Dictionary<Module.TypeEnum, Color>
            {
                {Module.TypeEnum.Main, MainColor},
                {Module.TypeEnum.Residential, ResidentialColor},
                {Module.TypeEnum.Industrial, IndustrialColor},
                {Module.TypeEnum.Center, CenterColor},
                {Module.TypeEnum.Unique, UniqueColor}
            };
        }

        private void Start()
        {
            // TODO: Придумать друоге обозначение неактивных клеток. Например, цветом
            activeMarkers = new GameObject[GridSize, GridSize];

            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    activeMarkers[i, j] = Instantiate(debugUnactive, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
        }

        #region events

        private void OnStartLevel()
        {
            ResetBuilder();
            previewCameraSystem.Module = GenerateModule();
            deliveryCameraSystem.Module = GenerateModule();
            isKeepingModuleInMouse = false;
            StartCoroutine(WaitTapping());
        }

        #endregion

        private void NormalizeProbability()
        {
            var probabilities = new [] { residentialProbability, industrialProbability, centerProbability , uniqueProbability };

            var sumProbability = 0f;

            for (var i = 0; i < probabilities.Length; i++)
            {
                if (probabilities[i] < 0f)
                {
                    Debug.LogError("Вероятность выпадения модуля меньше нуля");
                    probabilities[i] = 0f;
                }

                sumProbability += probabilities[i];
            }

            if (sumProbability == 0f)
            {
                for (var i = 0; i < probabilities.Length; i++)
                {
                    probabilities[i] = 1f / probabilities.Length;
                }
            }
            else if (sumProbability != 1f)
            {
                for (var i = 0; i < probabilities.Length; i++)
                {
                    probabilities[i] = probabilities[i] * (1f / sumProbability);
                }
            }
        }

        private Module GenerateModule()
        {
            var module = Instantiate(modulePrefabs[Random.Range(0, tetrisToggle.isOn ? modulePrefabs.Length : 1)]);

            var typeProbability = Random.value;
            var remainedProbability = 1f;

            module.mainRenderer.material = defaultModuleMaterial;

            foreach (var typeProbPair in typeProbabilityDict)
            {
                remainedProbability -= typeProbPair.Value;
                if (!(typeProbability >= remainedProbability)) continue;
                module.Type = typeProbPair.Key;
                break;
            }

            return module;
        }

        private IEnumerator WaitTapping()
        {
            while (!isKeepingModuleInMouse && GameManager.Instance.CurrentState == GameManager.GameStatus.Play)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                    Debug.DrawRay(ray.origin, ray.direction, Color.red);

                    if (Physics.Raycast(ray) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        TakeModuleInMouse();
                    }
                }

                yield return null;
            }
        }

        public void TakeModuleInMouse()
        {
            if (isKeepingModuleInMouse || GameManager.Instance.CurrentState != GameManager.GameStatus.Play) return;

            var module = deliveryCameraSystem.Module;
            deliveryCameraSystem.Module = previewCameraSystem.Module;
            previewCameraSystem.Module = GenerateModule();

            if (module == null) return;
            isKeepingModuleInMouse = true;
            StartCoroutine(KeepModuleToMouse(module));
        }

        private IEnumerator KeepModuleToMouse(Module module)
        {
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            var isModulePlaced = false;
            float? x = null;
            float? z = null;
            var isAvailable = false;

            module.transform.rotation = Quaternion.identity;
            module.SetPlacingMarkerActive(true);


            while (isKeepingModuleInMouse && !isModulePlaced)
            {
                var ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

                if (groundPlane.Raycast(ray, out float position))
                {
                    if (Input.GetMouseButton(0))
                    {
                        var worldPosition = ray.GetPoint(position);

                        x = (int)Mathf.Round(worldPosition.x);
                        z = (int)Mathf.Round(worldPosition.z);
                        isAvailable = grid.CanBePlaced((int)(x), (int)(z), module);
                        module.transform.position = new Vector3((float)x, 0.5f, (float)z);
                        module.MarkAvailablity(isAvailable);
                    }

                    if (isAvailable && Input.GetMouseButtonUp(0))
                    {
                        isModulePlaced = PlaceKeepingModule((int)(x), (int)(z), module);
                    }

                    // TODO: Поворот на пробел не получится сделать на телефоне. Его нужно либо убрать, либо изменить условие
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        module.Rotate(90);
                    }

                }

                yield return null;
            }

            if (!isModulePlaced)
            {
                Destroy(module.gameObject);
            }
        }

        private bool PlaceKeepingModule(int posX, int posZ, Module module)
        {
            if (grid.Add(posX, posZ, module))
            {
                isKeepingModuleInMouse = false;
                module.transform.position = new Vector3(module.transform.position.x, 0, module.transform.position.z);
                placedMods.Add(module);
                GameManager.Instance.MakeTurn(grid, posX, posZ, module);

                if ((float)grid.OccupiedCount / grid.ActiveCount > zoomCoefficient)
                {
                    ZoomOut();
                }

                module.gameObject.layer = 0;
                module.SetPlacingMarkerActive(false);
                StartCoroutine(WaitTapping());
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ResetBuilder()
        {
            currentExpandCount = 1;
            currentZoomCount = 0;
            activeRingsAmount = StartingRingsAmount;
            grid = new Grid(GridSize);
            grid.ActivateArea(activeRingsAmount);
            ReRenderActiveCells();

            var t = grid.array.GetLength(0) / 2;

            var mainModule = Instantiate(mainModulePrefab, new Vector3(t, 0, t), Quaternion.identity);
            mainModule.mainRenderer.material = defaultModuleMaterial;
            mainModule.Type = Module.TypeEnum.Main;
            grid.Add(t, t, mainModule);

            isKeepingModuleInMouse = false;

            foreach (var build in placedMods)
            {
                Destroy(build.gameObject);
            }
            placedMods.Clear();

            foreach (var obst in placedObstacles)
            {
                Destroy(obst.gameObject);
            }
            placedObstacles.Clear();

            for (var i = 1; i <= StartingRingsAmount; i++)
            {
                GenerateObstacles(i);
            }

            StartCoroutine(WaitTapping());

            if (tetrisToggle.isOn)
            {
                ZoomOut();
                zoomCoefficient = 0.6f;
            }
            else
            {
                zoomCoefficient = 0.666f;
            }
        }

        private void ReRenderActiveCells()
        {
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    activeMarkers[i, j].SetActive(!grid.array[i, j].IsActive);
                }
            }
        }

        private void ZoomOut()
        {
            activeRingsAmount += expandValues[currentExpandCount];
            grid.ActivateArea(activeRingsAmount);
            ReRenderActiveCells();

            CameraController.Instance.ZoomOut(zoomValues[currentZoomCount]);

            currentExpandCount++;
            currentZoomCount++;

            for (var i = activeRingsAmount - expandValues[currentExpandCount - 1] + 1;
                i <= activeRingsAmount; i++)
            {
                GenerateObstacles(i);
            }

        }

        private void GenerateObstacles(int ringNumber)
        {
            var anchor = GridSize / 2 - ringNumber;
            var sideLength = 2 * ringNumber + 1;
            var obstaclesAmount = ringNumber + expandValues[currentExpandCount - 1];

            var coordinates = new Vector2Int[obstaclesAmount];

            var newObstacleCoords = GenerateCoordinatesOnRing(sideLength);

            for (var i = 0; i < coordinates.Length; i++)
            {
                while (coordinates.Contains(newObstacleCoords))
                {
                    newObstacleCoords = GenerateCoordinatesOnRing(sideLength);
                }

                coordinates[i] = newObstacleCoords;

                placedObstacles.Add(
                    Instantiate(obstacle,
                        new Vector3(anchor + coordinates[i].x, 0, anchor + coordinates[i].y),
                        Quaternion.identity));

                grid.AddObstacle(anchor + coordinates[i].x, anchor + coordinates[i].y, obstacle);
            }
        }

        private Vector2Int GenerateCoordinatesOnRing(int sideLength)
        {
            var result = new Vector2Int();

            var temp = Random.Range(0, 2);
            switch (temp)
            {
                case 0:
                    result.x = Random.Range(0, 2) * (sideLength - 1);
                    result.y = Random.Range(0, sideLength);
                    break;
                case 1:
                    result.x = Random.Range(1, sideLength - 1);
                    result.y = Random.Range(0, 2) * (sideLength - 1);
                    break;
            }

            return result;
        }


        private void OnDisable()
        {
            GameManager.OnStartLevel -= OnStartLevel;
        }
    }
}
