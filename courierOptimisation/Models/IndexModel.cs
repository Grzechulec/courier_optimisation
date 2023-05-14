namespace courierOptimisation.Models
{
    public class IndexModel
    {
        public List<List<int>> Paths { get; set; }
        public List<(int, int)> Points { get; set; }
        public IndexModel()
        {
            Paths = new List<List<int>>();
            Points = new List<(int, int)>();
        }
    }
}
