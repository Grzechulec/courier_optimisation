using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace courierOptimisation.Models
{
    public class IndexModel
    {
        public List<List<int>> Paths { get; set; }
        public List<List<int>> DistanceMatrix  { get; set; }
        public List<PointModel> Points { get; set; }
        public List<int> Weights { get; set; }
        public string? JsonPoints { get; set; } = null;
        public string? JsonPaths { get; set; } = null;
        public int InitCost { get; set; } = 0;
        public double Cost { get; set; } = 0;
        public IndexModel()
        {
            Paths = new List<List<int>>();
            DistanceMatrix = new List<List<int>>();
            Points = new List<PointModel>();
            Weights = new List<int>();
        }
        public string PointsToJson()
        {
            string json = JsonSerializer.Serialize(Points);
            this.JsonPoints = json;
            return json;
        }

        public string PathsToJson()
        {
            string json = JsonSerializer.Serialize(Paths);
            this.JsonPaths = json;
            return json;
        }
    }
}
