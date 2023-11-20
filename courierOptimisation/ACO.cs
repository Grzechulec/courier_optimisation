using Microsoft.Extensions.Options;

namespace courierOptimisation
{
    public class ACOOptions
    {
        public int NumberOfCities { get; set; }
        public int NumberOfAnts { get; set; }
        public double Alpha { get; set; }
        public double Beta { get; set; }
        public double EvaporationRate { get; set; }
        public int VehicleCapacity { get; set; }
    }
    public class ACO
    {
        private ACOOptions options;
        public List<List<double>> distanceMatrix;
        private List<List<double>> pheromoneMatrix;
        public List<int> demands;
        private List<Ant> ants;
        public List<List<int>> shortestTours;
        public double shortestPath;

        public ACO(IOptions<ACOOptions> optionsAccessor)
        {
            this.distanceMatrix = new List<List<double>>();
            this.pheromoneMatrix = new List<List<double>>();
            this.options = optionsAccessor.Value;
            this.shortestTours = new();

            pheromoneMatrix = new();
            InitializePheromoneMatrix();

            ants = new List<Ant>();
            for (int i = 0; i < options.NumberOfAnts; i++)
            {
                ants.Add(new Ant(options.NumberOfCities, options.VehicleCapacity));
            }
        }

        public void Run(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var ant in ants)
                {
                    ant.BuildTour(pheromoneMatrix, distanceMatrix, demands, options.Alpha, options.Beta);
                }

                UpdatePheromoneMatrix();
                UpdateShortestPath();
            }

        }

        private void UpdateShortestPath()
        {
            double shortestPath = double.MaxValue;

            foreach (var ant in ants)
            {
                double tourLength = ant.TourLength(distanceMatrix);

                if (tourLength < shortestPath)
                {
                    shortestPath = tourLength;
                    shortestTours = ant.Tours;
                }
            }
            this.shortestPath = shortestPath;
        }


        public void InitializePheromoneMatrix()
        {
            pheromoneMatrix.Clear();
            double initialPheromoneValue = 1.0 / (options.NumberOfCities * GetAverageDistance(distanceMatrix));

            for (int i = 0; i < options.NumberOfCities; i++)
            {
                List<double> row = new List<double>(new double[options.NumberOfCities]);
                pheromoneMatrix.Add(row);

                for (int j = 0; j < options.NumberOfCities; j++)
                {
                    pheromoneMatrix[i][j] = initialPheromoneValue;
                }
            }
        }

        private double GetAverageDistance(List<List<double>> distanceMatrix)
        {
            double totalDistance = 0.0;
            int count = 0;

            for (int i = 0; i < distanceMatrix.Count; i++)
            {
                for (int j = i + 1; j < distanceMatrix[i].Count; j++)
                {
                    totalDistance += distanceMatrix[i][j];
                    count++;
                }
            }

            return count > 0 ? totalDistance / count : 0.0;
        }

        private void UpdatePheromoneMatrix()
        {
            // Parowanie feromonów
            for (int i = 0; i < options.NumberOfCities; i++)
            {
                for (int j = 0; j < options.NumberOfCities; j++)
                {
                    pheromoneMatrix[i][j] *= (1 - options.EvaporationRate);
                }
            }

            // Dodawanie nowych feromonów
            foreach (var ant in ants)
            {
                double contribution = 1.0 / ant.TourLength(distanceMatrix);
                foreach (var tour in ant.Tours)
                {
                    for (int i = 0; i < tour.Count - 1; i++)
                    {
                        int cityX = tour[i];
                        int cityY = tour[i + 1];
                        pheromoneMatrix[cityX][cityY] += contribution;
                        pheromoneMatrix[cityY][cityX] += contribution; // Symetria ścieżki
                    }
                }
            }
        }
    }

    class Ant
    {
        private int numberOfCities;
        public List<List<int>> Tours { get; private set; }
        private int vehicleCapacity;

        public double TourLength(List<List<double>> distanceMatrix)
        {
            double length = 0.0;
            foreach (List<int> tour in Tours)
            {
                for (int i = 0; i < tour.Count -1; i++)
                {
                    int currentCity = tour[i];
                    int nextCity = tour[i + 1];
                    length += distanceMatrix[currentCity][nextCity];
                }

                // Obejmuje powrót do miasta początkowego, jeśli trasa jest zamknięta
                /*if (tour.Count > 1)
                {
                    int lastCity = tour[tour.Count - 1];
                    int firstCity = tour[0];
                    length += distanceMatrix[lastCity][firstCity];
                }*/
            }
            return length;
        }

        public Ant(int numberOfCities, int vehicleCapacity)
        {
            this.numberOfCities = numberOfCities;
            this.vehicleCapacity = vehicleCapacity;
            Tours = new List<List<int>>();
        }

        public void BuildTour(List<List<double>> pheromoneMatrix, List<List<double>> distanceMatrix, List<int> demands, double alpha, double beta)
        {
            Tours.Clear();
            Tours.Add(new List<int> { 0 }); // Start with depot as first city in the first tour

            while (Tours.SelectMany(list => list).Count(item => item != 0) < numberOfCities-1)
            {
                foreach (var tour in Tours)
                {
                    while (CanAddMoreCities(tour, demands))
                    {
                        int currentCity = tour.Last();
                        int nextCity = ChooseNextCity(currentCity, pheromoneMatrix, distanceMatrix, demands, alpha, beta, tour);
                        if (nextCity == -1) // No feasible city found
                        {
                            break;
                        }
                        tour.Add(nextCity);
                    }
                    if (tour.Count > 1 && tour.Last() != 0)
                    {
                        tour.Add(0);
                    }
                }

                // Add a new tour if not all cities are covered
                if (Tours.Sum(t => t.Count) - Tours.SelectMany(list => list).Count(item => item == 0) + 1 < numberOfCities) // Subtract Tours.Count to exclude depots
                {
                    Tours.Add(new List<int> { 0 }); // Start new tour from depot
                }

            }

        }

        private bool CanAddMoreCities(List<int> tour, List<int> demands)
        {
            int currentLoad = tour.Sum(city => demands[city]);
            return currentLoad < vehicleCapacity && tour.Count < numberOfCities;
        }

        private int getCurrentLoad(List<int> tour, List<int> demands)
        {
            int sum = 0;
            foreach (int  city in tour)
            {
                sum += demands[city];
            }
            return sum;
        }
        private int ChooseNextCity(int currentCity, List<List<double>> pheromoneMatrix, List<List<double>> distanceMatrix, List<int> demands, double alpha, double beta, List<int> currentTour)
        {
            double[] probabilities = new double[numberOfCities];
            double probabilitySum = 0.0;

            for (int i = 0; i < numberOfCities; i++)
            {
                if (!Tours.Any(x => x.Contains(i)) && getCurrentLoad(currentTour, demands) + demands[i] <= vehicleCapacity)
                {
                    double pheromone = Math.Pow(pheromoneMatrix[currentCity][i], alpha);
                    double heuristic = Math.Pow(1.0 / distanceMatrix[currentCity][i], beta);
                    probabilities[i] = pheromone * heuristic;
                    probabilitySum += probabilities[i];
                }
            }

            // Losowanie miasta na podstawie obliczonych prawdopodobieństw
            double randomPoint = new Random().NextDouble() * probabilitySum;
            for (int i = 0; i < probabilities.Length; i++)
            {
                if (!Tours.Any(x => x.Contains(i)))
                {
                    randomPoint -= probabilities[i];
                    if (randomPoint <= 0) return i;
                }
            }

            return -1; // W przypadku błędu, powrót do miasta początkowego
        }
    }
}
