namespace Assets.Scripts
{
    public class Grid
    {
        public Cell[,] array;

        public int ActiveCount { get; private set; }

        public int OccupiedCount { get; private set; }

        public Cell this[int x, int y] => array[x, y];

        public Grid(int size)
        {
            array = new Cell[size, size];
            ActiveCount = 0;
            OccupiedCount = 0;

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    array[i, j] = new Cell();
                }
            }
        }

        /// <summary>
        /// Не выйдет ли модуль за карту, если его разместить на клетку с этими координатами
        /// </summary>
        private bool IsOnMap(int x, int z, Module module)
        {
            for (var i = 0; i < module.shape.Length; i++)
            {
                if (x + (int)module.shape[i].x >= array.GetLength(0) ||
                    z + (int)module.shape[i].y >= array.GetLength(1))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Нет ли уже модуля модуля на клетках, который займет модуль, если его разместить на клетку с этими координатами
        /// </summary>
        private bool IsFree(int x, int z, Module module)
        {
            for (var i = 0; i < module.shape.Length; i++)
            {
                if (!array[x + (int)module.shape[i].x, z + (int)module.shape[i].y].IsEmpty)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Не окажется ли модуль на недоступной для строительства клетке
        /// по причине недостаточного расширения зоны строительства, если его разместить на клетку с этими координатами
        /// </summary>
        private bool IsActive(int x, int z, Module module)
        {
            for (var i = 0; i < module.shape.Length; i++)
            {
                if (!array[x + (int)module.shape[i].x, z + (int)module.shape[i].y].IsActive)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Будут ли у модуля соседи, если его разместить на клетку с этими координатами
        /// </summary>
        private bool IsNeighbourNearOrMain(int x, int z, Module module)
        {
            for (var i = 0; i < module.shape.Length; i++)
            {
                if (IsNeighbourNear(x + (int)module.shape[i].x, z + (int)module.shape[i].y))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Есть ли модули у клеток соседней с той, у которой такие координаты
        /// </summary>
        private bool IsNeighbourNear(int x, int z)
        {
            return !((x >= array.GetLength(0) || array[x + 1, z].Module == null)
                     && (z >= array.GetLength(1) || array[x, z + 1].Module == null)
                     && (x < 0 || array[x - 1, z].Module == null)
                     && (z < 0 || array[x, z - 1].Module == null));
        }

        public bool CanBePlaced(int x, int z, Module module)
            => IsOnMap(x, z, module)
               && IsFree(x, z, module)
               && IsActive(x, z, module)
               && IsNeighbourNearOrMain(x, z, module);

        public bool Add(int x, int z, Module module)
        {
            if (!CanBePlaced(x, z, module) && module.Type != Module.TypeEnum.Main)
            {
                return false;
            }

            for (var i = 0; i < module.shape.Length; i++)
            {
                array[x + (int)module.shape[i].x, z + (int)module.shape[i].y].Module = module;
            }

            OccupiedCount += module.shape.Length;

            return true;
        }

        public void ActivateArea(int ringsCount)
        {
            ActiveCount = 0;

            var temp = array.GetLength(0) / 2;
            temp -= ringsCount;

            for (var i = 0; i <= ringsCount * 2; i++)
            {
                for (var j = 0; j <= ringsCount * 2; j++)
                {
                    array[temp + i, temp + j].IsActive = true;
                    ActiveCount++;
                }
            }
        }

        // Пока поддерживаем только одномодульные препятствия.
        public bool AddObstacle(int x, int z, Obstacle obstacle)
        {
            if (!array[x, z].IsActive || !array[x, z].IsEmpty
                                      || x >= array.GetLength(0) || z >= array.GetLength(1))
            {
                return false;
            }

            array[x, z].Obstacle = obstacle;

            // Пока не будем поддерживать логику установки модулей на препятствия.
            OccupiedCount++;

            return true;
        }
    }
}
