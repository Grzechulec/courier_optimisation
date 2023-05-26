namespace courierOptimisation.Models
{
    public class PointModel
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointModel (int x, int y)
        {
            X = x;
            Y = y;
        }

        public string PrintPoint()
        {
            return "(" + X + "," + Y + ")";
        }
    }
}
