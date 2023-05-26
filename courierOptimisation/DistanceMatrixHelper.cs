using courierOptimisation.Models;
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
        public static List<List<int>> GenerateDistanceMatrix(List<PointModel> points)
        {
            List<List<int>> distanceMatrix = new();
            foreach (var point in points)
            {
                AddPoint(point, points, distanceMatrix);
            }
            return distanceMatrix;
        }

        //to test
        public static void AddPoint(PointModel point, List<PointModel> points, List<List<int>> distanceMatrix)
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

        private static void UpdateRow(PointModel newPoint, PointModel rowPoint, List<int> row)
        {
            int distance = GetDistance(newPoint, rowPoint);
            row.Add(distance);
        }

        private static void AddNewRow(PointModel newPoint, List<PointModel> points, List<List<int>> distanceMatrix)
        {
            List<int> row = new();
            for (int i = 0; i < distanceMatrix.Count; i++)
            {
                row.Add(GetDistance(points[i], newPoint));
            }
            row.Add(0);
            distanceMatrix.Add(row);
        }

        private static int GetDistance(PointModel point1, PointModel point2)
        {
            return (int)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        public static List<PointModel> ConvertStringsToPoints(List<string> strings)
        {
            List<PointModel> points = new();
            foreach (string s in strings)
            {
                string[] numbers = s.Split(' ');
                points.Add( new PointModel(int.Parse(numbers[0]), int.Parse(numbers[1])));
            }
            return points;
        }
    }
}
