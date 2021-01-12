namespace Assets.Scripts
{
    public class Cell
    {
        public bool IsActive { get; set; }

        public bool IsEmpty => Module == null && Obstacle == null;

        public Module Module { get; set; }

        public Obstacle Obstacle { get; set; }
    }
}
