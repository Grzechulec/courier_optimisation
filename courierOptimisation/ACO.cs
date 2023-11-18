namespace courierOptimisation
{
    public class ACO
    {
        private int numberOfCities;
        private int numberOfAnts;
        private double alpha, beta, evaporationRate;
        private List<List<double>> distanceMatrix;
        private List<List<double>> pheromoneMatrix;
        private List<Ant> ants;

        public ACO(int numberOfCities, int numberOfAnts, double alpha, double beta, double evaporationRate, List<List<double>> distanceMatrix)
        {
            this.numberOfCities = numberOfCities;
            this.numberOfAnts = numberOfAnts;
            this.alpha = alpha;
            this.beta = beta;
            this.evaporationRate = evaporationRate;
            this.distanceMatrix = distanceMatrix;

            pheromoneMatrix = new();
            InitializePheromoneMatrix();

            ants = new List<Ant>();
            for (int i = 0; i < numberOfAnts; i++)
            {
                ants.Add(new Ant(numberOfCities));
            }
        }

        public void Run(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var ant in ants)
                {
                    ant.BuildTour(pheromoneMatrix, distanceMatrix, alpha, beta);
                }

                UpdatePheromoneMatrix();
            }
        }

        private void InitializePheromoneMatrix()
        {
            double initialPheromoneValue = 1.0 / (numberOfCities * GetAverageDistance(distanceMatrix));

            for (int i = 0; i < numberOfCities; i++)
            {
                for (int j = 0; j < numberOfCities; j++)
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
            for (int i = 0; i < numberOfCities; i++)
            {
                for (int j = 0; j < numberOfCities; j++)
                {
                    pheromoneMatrix[i][j] *= (1 - evaporationRate);
                }
            }

            // Dodawanie nowych feromonów
            foreach (var ant in ants)
            {
                double contribution = 1.0 / ant.TourLength(distanceMatrix);
                for (int i = 0; i < ant.Tour.Count - 1; i++)
                {
                    int cityX = ant.Tour[i];
                    int cityY = ant.Tour[i + 1];
                    pheromoneMatrix[cityX][cityY] += contribution;
                    pheromoneMatrix[cityY][cityX] += contribution; // Symetria ścieżki
                }
            }
        }
    }

    class Ant
    {
        private int numberOfCities;
        public List<int> Tour { get; private set; }

        public double TourLength(List<List<double>> distanceMatrix)
        {
            double length = 0.0;

            for (int i = 0; i < Tour.Count - 1; i++)
            {
                int currentCity = Tour[i];
                int nextCity = Tour[i + 1];
                length += distanceMatrix[currentCity][nextCity];
            }

            // Obejmuje powrót do miasta początkowego, jeśli trasa jest zamknięta
            if (Tour.Count > 1)
            {
                int lastCity = Tour[Tour.Count - 1];
                int firstCity = Tour[0];
                length += distanceMatrix[lastCity][firstCity];
            }

            return length;
        }

        public Ant(int numberOfCities)
        {
            this.numberOfCities = numberOfCities;
            Tour = new List<int>();
        }

        public void BuildTour(List<List<double>> pheromoneMatrix, List<List<double>> distanceMatrix, double alpha, double beta)
        {
            // Budowa trasy przez mrówkę
            Tour.Clear();
            Tour.Add(0); // Losowe miasto początkowe

            while (Tour.Count < numberOfCities)
            {
                int currentCity = Tour.Last();
                int nextCity = ChooseNextCity(currentCity, pheromoneMatrix, distanceMatrix, alpha, beta);
                Tour.Add(nextCity);
            }

            // Dodanie powrotu do miasta początkowego
            Tour.Add(Tour[0]);
        }
        private int ChooseNextCity(int currentCity, List<List<double>> pheromoneMatrix, List<List<double>> distanceMatrix, double alpha, double beta)
        {
            double[] probabilities = new double[numberOfCities];
            double probabilitySum = 0.0;

            for (int i = 0; i < numberOfCities; i++)
            {
                if (!Tour.Contains(i))
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
                if (!Tour.Contains(i))
                {
                    randomPoint -= probabilities[i];
                    if (randomPoint <= 0) return i;
                }
            }

            return Tour[0]; // W przypadku błędu, powrót do miasta początkowego
        }
    }
}
