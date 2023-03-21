using Microsoft.CodeAnalysis;

namespace courierOptimisation
{
    class NeighbourData
    {
        public const int defaultCost = int.MaxValue;
        public int pathFrom;
        public int fromIndex;
        public int pathTo;
        public int toIndex;
        public int cost = defaultCost;
    }

    class TabuNode
    {
        public const int initVal = -1;
        public int clientIndex;
        public int pathIndex;

        public TabuNode(int clientIn = initVal, int pathIn = initVal)
        {
            clientIndex = clientIn;
            pathIndex = pathIn;
        }
    }

    public class PathsFinder
    {
        private const int _carCapacity = 20;
        private readonly List<List<int>> _distanceMatrix;
        private readonly List<int> _clientsWeights = new() {0, 9, 4, 1, 10, 5};
        private List<List<int>> _paths = new();
        private List<int> _pathsWeights = new();
        private int _currentCost;
        private List<TabuNode> _tabuList;
        private int _currentTabuIndex = 0;

        public List<List<int>> bestPaths { get; private set; } = new();
        public int bestPathsCost { get; private set; } = int.MaxValue;

        public PathsFinder()
        {
            _distanceMatrix = new List<List<int>>()
            {
                new() {0, 2, 2, 8, 9, 8},
                new() {2, 0, 3, 12, 10, 5},
                new() {2, 3, 0, 6, 10, 12},
                new() {8, 12, 6, 0, 1, 3},
                new() {9, 10, 10, 1, 0, 2},
                new() {8, 5, 12, 3, 2, 0}
            };

            const int tabuListSize = 7;
            _tabuList = new List<TabuNode>();
            for (int i = 0; i < tabuListSize; ++i)
            {
                _tabuList.Add(new TabuNode());
            }
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

            _pathsWeights = new List<int>(new int[_paths.Count]);
            this.countCurrentPathsWeights();
            this.orderPathsClients();
            this.updateBestPaths();
        }

        public void traverseSolutions()
        {
            for (int i = 0; i < 2 * _clientsWeights.Count; ++i)
            {
                var neighobourOpt = this.findBestInsertNeighbour();
                if (neighobourOpt.HasValue)
                {
                    this.updateTabuList(neighobourOpt.Value);
                    this.goFromCurrentToNeighbour(neighobourOpt.Value);
                    this.orderPathsClients();
                    this.updateBestPaths();
                }
                else
                {
                    this.resetTabuList();
                    this.goFromCurrentToBestSolution();
                }
            }
        }

        private void countCurrentPathsWeights()
        {
            int i = 0;
            foreach (var path in _paths)
            {
                var pathWeight = 0;
                foreach (var clientIndex in path)
                {
                    pathWeight += _clientsWeights[clientIndex];
                }
                _pathsWeights[i++] = pathWeight;
            }
        }

        private void orderPathsClients()
        {
            for (int i = 0; i < _paths.Count; ++i)
            {
                _paths[i] = this.getOrderedPathClients(_paths[i]);
            }
            _currentCost = this.getCurrentCost();
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

        private Optional<NeighbourData> findBestInsertNeighbour()
        {
            var bestNeighbour = new NeighbourData();
            for (int pathFrom = 0; pathFrom < _paths.Count; ++pathFrom)
            {
                if (_paths[pathFrom].Count <= 1)
                {
                    continue;
                }
                for (int pathTo = 0; pathTo < _paths.Count; ++pathTo)
                {
                    if (pathTo == pathFrom)
                    {
                        continue;
                    }
                    for (int fromIndex = 1; fromIndex < _paths[pathFrom].Count - 1; ++fromIndex)
                    {
                        if (this.isOnTabuList(_paths[pathFrom][fromIndex], pathTo) ||
                            this.isClientToHeavyForPath(_clientsWeights[_paths[pathFrom][fromIndex]], _pathsWeights[pathTo]))
                        {
                            continue;
                        }
                        for (int toIndex = 1; toIndex < _paths[pathTo].Count; ++toIndex)
                        {
                            var neighbourCost = _currentCost;
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

            return (bestNeighbour.cost == NeighbourData.defaultCost) ? new Optional<NeighbourData>() : bestNeighbour;
        }

        private void goFromCurrentToNeighbour(NeighbourData n)
        {
            _currentCost = n.cost;
            var clientIndex = _paths[n.pathFrom][n.fromIndex];
            _pathsWeights[n.pathFrom] -= _clientsWeights[_paths[n.pathFrom][n.fromIndex]];
            _pathsWeights[n.pathTo] += _clientsWeights[_paths[n.pathFrom][n.fromIndex]];
            _paths[n.pathFrom].RemoveAt(n.fromIndex);
            _paths[n.pathTo].Insert(n.toIndex, clientIndex);
        }

        private void updateBestPaths()
        {
            if (_currentCost >= bestPathsCost) return;

            bestPathsCost = _currentCost;
            bestPaths = new();
            foreach (var path in _paths)
            {
                bestPaths.Add(new List<int>(new int[path.Count]));
                for (int i = 0; i < path.Count; ++i)
                {
                    bestPaths[^1][i] = path[i];
                }
            }
        }

        private void updateTabuList(NeighbourData n)
        {
            _tabuList[_currentTabuIndex] = new TabuNode(_paths[n.pathFrom][n.fromIndex], n.pathFrom);
            ++_currentTabuIndex;
            if (_currentTabuIndex >= _tabuList.Count)
            {
                _currentTabuIndex = 0;
            }
        }

        private bool isOnTabuList(int clientIndex, int pathIndex)
        {
            return _tabuList.FindIndex(node => node.clientIndex == clientIndex && node.pathIndex == pathIndex) >= 0;
        }

        private void resetTabuList()
        {
            for (int i = 0; i < _tabuList.Count; ++i)
            {
                _tabuList[i].clientIndex = TabuNode.initVal;
                _tabuList[i].pathIndex = TabuNode.initVal;
            }
            _currentTabuIndex = 0;
        }

        private void goFromCurrentToBestSolution()
        {
            for (int i = 0; i < _paths.Count; ++i)
            {
                _paths[i] = new List<int>(new int[bestPaths[i].Count]);
                for (int j = 0; j < _paths[i].Count; ++j)
                {
                    _paths[i][j] = bestPaths[i][j];
                }
            }
            this.countCurrentPathsWeights();
            _currentCost = bestPathsCost;
        }
    }
}
