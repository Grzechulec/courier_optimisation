using System.IO;

namespace courierOptimisation
{
    class BestNeighbour
    {
        public int pathFrom;
        public int fromIndex;
        public int pathTo;
        public int toIndex;
        public int cost = int.MaxValue;
    }

    public class PathsFinder
    {
        private List<List<int>> _distanceMatrix;
        private List<int> _clientsWeights = new() {0, 9, 4, 1, 10, 5};
        private List<List<int>> _paths = new();
        private List<int> _pathsWeights = new();
        public List<List<int>> bestPaths { get; private set; } = new();
        private const int _carCapacity = 20;

        public PathsFinder()
        {
            _distanceMatrix = new List<List<int>>()
            {
                new List<int>() {0, 2, 2, 8, 9, 8},
                new List<int>() {2, 0, 3, 12, 10, 5},
                new List<int>() {2, 3, 0, 6, 10, 12},
                new List<int>() {8, 12, 6, 0, 1, 3},
                new List<int>() {9, 10, 10, 1, 0, 2},
                new List<int>() {8, 5, 12, 3, 2, 0}
            };
        }

        public void generateInitialPaths() {
            var weightsIndex = new List<Tuple<int, int>>(); // (weight, index)
            for (int i = 1; i < _clientsWeights.Count; ++i) 
            {
                weightsIndex.Add(new Tuple<int, int>(_clientsWeights[i], i));   
            }
            weightsIndex = weightsIndex.OrderByDescending(x => x.Item1).ToList();

            while (weightsIndex.Count > 0) 
            {
                var capLeft = _carCapacity;
                _paths.Add(new List<int>{0});
                while (weightsIndex.Count > 0 && weightsIndex.Last().Item1 <= capLeft) 
                {
                    var maxIndex = weightsIndex.Count - 1;
                    for (int i = 0; i < weightsIndex.Count; ++i) 
                    {
                        if (weightsIndex[i].Item1 <= capLeft && weightsIndex[i].Item1 > weightsIndex[maxIndex].Item1) 
                        {
                            maxIndex = i;
                            break;
                        }
                    }
                    capLeft -= weightsIndex[maxIndex].Item1;
                    _paths.Last().Add(weightsIndex[maxIndex].Item2);
                    weightsIndex.RemoveAt(maxIndex);
                }
                _paths.Last().Add(0);
            }

            this.countIniitalPathsWeights();
            this.orderPathsClients();
            bestPaths = _paths;

            foreach (var path in _paths)
            {
                Console.WriteLine(String.Join(" ", path));
            }
            Console.WriteLine(this.getCurrentCost());
            var n = this.findBestInsertNeighbour();
            Console.WriteLine(n.pathFrom);
            Console.WriteLine(n.fromIndex);
            Console.WriteLine(n.pathTo);
            Console.WriteLine(n.toIndex);
            Console.WriteLine(n.cost);
        }

        private void countIniitalPathsWeights()
        {
            for (int i = 0; i < _paths.Count; ++i)
            {
                var pathWeight = 0;
                foreach (var clientIndex in _paths[i])
                {
                    pathWeight += _clientsWeights[clientIndex];
                }
                _pathsWeights.Add(pathWeight);
            }
        }

        private void orderPathsClients()
        {
            for (int i = 0; i < _paths.Count; ++i)
            {
                _paths[i] = this.getOrderedPathClients(_paths[i]);
            }
        }

        private List<int> getOrderedPathClients(List<int> path)
        {
            List<bool> isInNewPath = Enumerable.Repeat(false, path.Count).ToList();
            var newPath = new List<int>(new int[path.Count]);
            newPath[0] = 0;
            newPath[path.Count - 1] = 0;
            isInNewPath[0] = true;
            isInNewPath[path.Count - 1] = true;

            for (int i = 1; i < newPath.Count - 1; ++i)
            {
                int nearestNeighbour = -1;
                int minDistance = int.MaxValue;
                for (int j = 1; j < path.Count - 1; ++j)
                {
                    var distance = _distanceMatrix[newPath[i - 1]][path[j]];
                    if (!isInNewPath[j] && distance > 0 && distance < minDistance)
                    {
                        minDistance = distance;
                        nearestNeighbour = j;
                    }
                }
                newPath[i] = path[nearestNeighbour];
                isInNewPath[nearestNeighbour] = true;
            }

            return newPath;
        }

        public int getCurrentCost()
        {
            int cost = 0;
            foreach (var path in _paths)
            {
                for (int i = 0; i < path.Count - 1; ++i)
                {
                    cost += _distanceMatrix[path[i]][path[i + 1]];
                }
            }
            return cost;
        }

        private bool isClientToHeavyForPath(int clientWeight, int pathWeight)
        {
            return clientWeight + pathWeight > _carCapacity;
        }

        private BestNeighbour findBestInsertNeighbour()
        {
            var bestNeighbour = new BestNeighbour();
            var currentCost = getCurrentCost();
            for (int pathFrom = 0; pathFrom < _paths.Count; ++pathFrom)
            {
                if (_paths[pathFrom].Count <= 1)
                {
                    continue;
                }
                for (int pathTo = 0; pathTo < _paths.Count; ++pathTo)
                {
                    if (pathTo != pathFrom)
                    {
                        for (int fromIndex = 1; fromIndex < _paths[pathFrom].Count - 1; ++fromIndex)
                        {
                            if (isClientToHeavyForPath(_clientsWeights[_paths[pathFrom][fromIndex]], _pathsWeights[pathTo]))
                            {
                                continue;
                            }
                            for (int toIndex = 1; toIndex < _paths[pathTo].Count; ++toIndex)
                            {
                                var neighbourCost = currentCost;
                                neighbourCost -= _distanceMatrix[_paths[pathFrom][fromIndex - 1]][_paths[pathFrom][fromIndex]];
                                neighbourCost -= _distanceMatrix[_paths[pathFrom][fromIndex]][_paths[pathFrom][fromIndex + 1]];
                                neighbourCost += _distanceMatrix[_paths[pathFrom][fromIndex - 1]][_paths[pathFrom][fromIndex + 1]];

                                neighbourCost -= _distanceMatrix[_paths[pathTo][toIndex - 1]][_paths[pathTo][toIndex]];
                                neighbourCost += _distanceMatrix[_paths[pathTo][toIndex - 1]][_paths[pathFrom][fromIndex]];
                                neighbourCost += _distanceMatrix[_paths[pathFrom][fromIndex]][_paths[pathTo][toIndex]];

                                if (neighbourCost < bestNeighbour.cost)
                                {
                                    bestNeighbour.pathFrom = pathFrom;
                                    bestNeighbour.fromIndex = fromIndex;
                                    bestNeighbour.pathTo = pathTo;
                                    bestNeighbour.toIndex = toIndex;
                                    bestNeighbour.cost = neighbourCost;
                                }
                            }
                        }
                    }
                }
            }

            return bestNeighbour;
        }
    }
}
