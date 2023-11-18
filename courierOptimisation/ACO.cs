namespace courierOptimisation
{
    public class ACO
    {
        private int numberOfCities;
        private int numberOfAnts;
        private double alpha, beta, evaporationRate;
        private double[,] distanceMatrix;
        private double[,] pheromoneMatrix;
        private List<Ant> ants;

        public ACO(int numberOfCities, int numberOfAnts, double alpha, double beta, double evaporationRate, double[,] distanceMatrix)
        {
            this.numberOfCities = numberOfCities;
            this.numberOfAnts = numberOfAnts;
            this.alpha = alpha;
            this.beta = beta;
            this.evaporationRate = evaporationRate;
            this.distanceMatrix = distanceMatrix;

            pheromoneMatrix = new double[numberOfCities, numberOfCities];
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
            // Inicjalizacja macierzy feromonów
        }

        private void UpdatePheromoneMatrix()
        {
            // Aktualizacja macierzy feromonów
        }
    }

    class Ant
    {
        private int numberOfCities;
        public List<int> Tour { get; private set; }

        public Ant(int numberOfCities)
        {
            this.numberOfCities = numberOfCities;
            Tour = new List<int>();
        }

        public void BuildTour(double[,] pheromoneMatrix, double[,] distanceMatrix, double alpha, double beta)
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
        private int ChooseNextCity(int currentCity, double[,] pheromoneMatrix, double[,] distanceMatrix, double alpha, double beta)
        {
            double[] probabilities = new double[numberOfCities];
            double probabilitySum = 0.0;

            for (int i = 0; i < numberOfCities; i++)
            {
                if (!Tour.Contains(i))
                {
                    double pheromone = Math.Pow(pheromoneMatrix[currentCity, i], alpha);
                    double heuristic = Math.Pow(1.0 / distanceMatrix[currentCity, i], beta);
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
