using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        public double Score { get; private set; }

        public ScoreManager()
        {
            Score = 0;
        }

        public void CountScore(Grid map, Vector2Int point, Module module)
        {
            var pointsOfModule = GetPointsOfModule(point, module);
            var visited = new HashSet<Vector2Int>(pointsOfModule);
            var type = module.Type;
            var result = new List<Vector2Int>(pointsOfModule);
            var pointsToCheck = new Queue<Vector2Int>(GetNeighborhoodsOfModule(pointsOfModule));
            while (pointsToCheck.Count != 0)
            {
                var currentPoint = pointsToCheck.Dequeue();

                if (visited.Contains(currentPoint))
                    continue;

                visited.Add(currentPoint);
                var curCell = map.array[currentPoint.x, currentPoint.y];

                if (curCell == null)
                    continue;

                var curModule = curCell.Module;
                if ((curModule == null) || (type != curModule.Type))
                    continue;

                result.Add(currentPoint);

                foreach (var adjacentPoint in FindNeighborhoodsPoints(currentPoint))
                    pointsToCheck.Enqueue(adjacentPoint);
            }

            var moduleSize = module.shape.Length;
            Score += moduleSize * (1 + 0.5 * (result.Count - 1));
        }

        private static List<Vector2Int> GetPointsOfModule(Vector2Int clickPoint, Module module)
        {
            return module.shape.Select(point =>
                    new Vector2Int(clickPoint.x + Convert.ToInt32(point.x),
                        clickPoint.y + Convert.ToInt32(point.y)))
                .ToList();
        }

        private static IEnumerable<Vector2Int> FindNeighborhoodsPoints(Vector2Int point)
        {
            var adjacentPoints = new List<Vector2Int>
            {
                new Vector2Int(point.x - 1, point.y),
                new Vector2Int(point.x + 1, point.y),
                new Vector2Int(point.x, point.y - 1),
                new Vector2Int(point.x, point.y + 1)
            };

            return adjacentPoints.ToArray();
        }

        private static IEnumerable<Vector2Int> GetNeighborhoodsOfModule(List<Vector2Int> pointsOfModule)
        {
            var adjacentPointsOfModule = new List<Vector2Int>();
            foreach (var adjacentPoint in
                from point in pointsOfModule
                from adjacentPoint in FindNeighborhoodsPoints(point)
                where !adjacentPointsOfModule.Contains(adjacentPoint) select adjacentPoint)
            {
                adjacentPointsOfModule.Add(adjacentPoint);
            }

            return adjacentPointsOfModule;
        }
    }
}
