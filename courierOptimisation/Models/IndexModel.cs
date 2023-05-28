using System.Text.Json;

namespace courierOptimisation.Models
{
    public class IndexModel
    {
        public List<List<int>> Paths { get; set; }
        public List<List<int>> DistanceMatrix  { get; set; }
        public List<PointModel> Points { get; set; }
        public string? Json { get; set; } = null;
        public IndexModel()
        {
            Paths = new List<List<int>>();
            DistanceMatrix = new List<List<int>>();
            Points = new List<PointModel>();
        }
        public string PointsToJson()
        {
            string json = JsonSerializer.Serialize(Points);
            this.Json = json;
            return json;
        }
    }
}
