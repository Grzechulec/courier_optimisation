using Microsoft.AspNetCore.Mvc;

namespace courierOptimisation
{
    public static class DistanceMatrixHelper
    {
        public static bool IsValid(int xCord, int yCord, int xPlaneSize, int yPlaneSize)
        {
            return xCord < xPlaneSize && yCord < yPlaneSize && xCord >= 0 && yCord >= 0;
        } 
        
        //to test
        public static List<List<int>> GenerateDistanceMatrix(List<(int, int)> points)
        {
            List<List<int>> distanceMatrix = new();
            foreach (var point in points)
            {
                AddPoint(point, points, distanceMatrix);
            }
            return distanceMatrix;
        }

        //to test
        public static void AddPoint((int, int) point, List<(int, int)> points, List<List<int>> distanceMatrix)
        {
            if (distanceMatrix.Count == 0)
            {
                distanceMatrix.Add(new List<int>() { 0 });
                return;
            }
            for (int i = 0; i < distanceMatrix.Count; i++)
            {
                UpdateRow(point, points[i], distanceMatrix[i]);
            }
            AddNewRow(point, points, distanceMatrix);
        }

        private static void UpdateRow((int, int) newPoint, (int, int) rowPoint, List<int> row)
        {
            int distance = GetDistance(newPoint, rowPoint);
            row.Add(distance);
        }

        private static void AddNewRow((int, int) newPoint, List<(int, int)> points, List<List<int>> distanceMatrix)
        {
            List<int> row = new();
            for (int i = 0; i < distanceMatrix.Count; i++)
            {
                row.Add(GetDistance(points[i], newPoint));
            }
            row.Add(0);
            distanceMatrix.Add(row);
        }

        private static int GetDistance((int,int) point1, (int, int) point2)
        {
            return (int)Math.Sqrt(Math.Pow(point2.Item1 - point1.Item1, 2) + Math.Pow(point2.Item2 - point1.Item2, 2));
        }

        public static List<(int, int)> ConvertStringsToPoints(List<string> strings)
        {
            List<(int, int)> points = new();
            foreach (string s in strings)
            {
                string[] numbers = s.Split(' ');
                points.Add((int.Parse(numbers[0]), int.Parse(numbers[1])));
            }
            return points;
        }
    }
}
