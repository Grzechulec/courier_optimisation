using Microsoft.AspNetCore.Routing.Template;
using Microsoft.CodeAnalysis;
using static courierOptimisation.NeighbourData;

namespace courierOptimisation
{
    class NeighbourData
    {
        public enum ActionType {Insert, Swap};
        public const int defaultCost = int.MaxValue;
        public int pathFirst;
        public int firstIndex;
        public int pathSecond;
        public int secondIndex;
        public int cost = defaultCost;
        public ActionType neighborType;

        public NeighbourData(ActionType type)
        {
            neighborType = type;
        }
    }

    public class PathsFinder
    {
        private const int _carCapacity = 15;
        //private readonly List<int> _clientsWeights = new() { 0, 9, 4, 1, 10, 5 };
        //private readonly List<int> _clientsWeights = new() { 0, 1, 1, 2, 4, 2, 4, 8, 8, 1, 2, 1, 2, 4, 4, 8, 8 };
        public List<int> _clientsWeights = new() { 0, 1, 1, 2, 4, 2, 4, 8, 8 };
        private List<List<int>> _paths = new();
        private List<int> _pathsWeights = new();
        private int _currentCost;
        private TabuList _tabuList = new();

        public List<List<int>> bestPaths { get; private set; } = new();
        public int bestPathsCost { get; private set; } = int.MaxValue;

        //private readonly List<List<int>> _distanceMatrix = new() {
        //    new() {0, 2, 2, 8, 9, 8},
        //    new() {2, 0, 3, 12, 10, 5},
        //    new() {2, 3, 0, 6, 10, 12},
        //    new() {8, 12, 6, 0, 1, 3},
        //    new() {9, 10, 10, 1, 0, 2},
        //    new() {8, 5, 12, 3, 2, 0}
        //};
        public List<List<int>> _distanceMatrix = new() {
            new() { 0, 548, 776, 696, 582, 274, 502, 194, 308, 194, 536, 502, 388, 354, 468, 776, 662 },
            new() { 548, 0, 684, 308, 194, 502, 730, 354, 696, 742, 1084, 594, 480, 674, 1016, 868, 1210 },
            new() { 776, 684, 0, 992, 878, 502, 274, 810, 468, 742, 400, 1278, 1164, 1130, 788, 1552, 754 },
            new() { 696, 308, 992, 0, 114, 650, 878, 502, 844, 890, 1232, 514, 628, 822, 1164, 560, 1358 },
            new() { 582, 194, 878, 114, 0, 536, 764, 388, 730, 776, 1118, 400, 514, 708, 1050, 674, 1244 },
            new() { 274, 502, 502, 650, 536, 0, 228, 308, 194, 240, 582, 776, 662, 628, 514, 1050, 708 },
            new() { 502, 730, 274, 878, 764, 228, 0, 536, 194, 468, 354, 1004, 890, 856, 514, 1278, 480 },
            new() { 194, 354, 810, 502, 388, 308, 536, 0, 342, 388, 730, 468, 354, 320, 662, 742, 856 },
            new() { 308, 696, 468, 844, 730, 194, 194, 342, 0, 274, 388, 810, 696, 662, 320, 1084, 514 },
            new() { 194, 742, 742, 890, 776, 240, 468, 388, 274, 0, 342, 536, 422, 388, 274, 810, 468 },
            new() { 536, 1084, 400, 1232, 1118, 582, 354, 730, 388, 342, 0, 878, 764, 730, 388, 1152, 354 },
            new() { 502, 594, 1278, 514, 400, 776, 1004, 468, 810, 536, 878, 0, 114, 308, 650, 274, 844 },
            new() { 388, 480, 1164, 628, 514, 662, 890, 354, 696, 422, 764, 114, 0, 194, 536, 388, 730 },
            new() { 354, 674, 1130, 822, 708, 628, 856, 320, 662, 388, 730, 308, 194, 0, 342, 422, 536 },
            new() { 468, 1016, 788, 1164, 1050, 514, 514, 662, 320, 274, 388, 650, 536, 342, 0, 764, 194 },
            new() { 776, 868, 1552, 560, 674, 1050, 1278, 742, 1084, 810, 1152, 274, 388, 422, 764, 0, 798 },
            new() { 662, 1210, 754, 1358, 1244, 708, 480, 856, 514, 468, 354, 844, 730, 536, 194, 798, 0 }
        };

        public PathsFinder()
        {

        }

        public void generateInitialPaths() {
            _paths.Clear();
            _tabuList.resetTabuList();

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

            _pathsWeights.AddRange(new int[_paths.Count]);
            this.countCurrentPathsWeights();
            this.orderPathsClients(Enumerable.Range(0, _paths.Count));
            this.updateBestPaths();
        }

        public void traverseSolutions()
        {
            generateInitialPaths();
            int iterations = (int)(300 * Math.Sqrt(_clientsWeights.Count));
            for (int i = 0; i < iterations; ++i)
            {
                var insertOpt = this.findBestInsertNeighbour();
                var swapOpt = this.findBestSwapNeighbour();
                var bestOpt = this.getBestNeighbour(insertOpt, swapOpt);
                if (!bestOpt.HasValue)
                {
                    _tabuList.resetTabuList();
                    this.goFromCurrentToBestSolution();
                }
                else
                {
                    var neighbour = bestOpt.Value;
                    List<TabuNode> tabuNodesList;
                    var pathsToOrder = new List<int> { neighbour.pathFirst, neighbour.pathSecond };

                    if (neighbour.neighborType == ActionType.Insert)
                    {
                        var tabuNode = new TabuNode(_paths[neighbour.pathFirst][neighbour.firstIndex], neighbour.pathFirst);
                        tabuNodesList = new List<TabuNode>() { tabuNode };
                    }
                    else
                    {
                        var firstNode = new TabuNode(_paths[neighbour.pathFirst][neighbour.firstIndex], neighbour.pathFirst);
                        var secondNode = new TabuNode(_paths[neighbour.pathSecond][neighbour.secondIndex], neighbour.pathSecond);
                        tabuNodesList = new List<TabuNode>() { firstNode, secondNode };
                    }
                    _tabuList.updateTabuList(tabuNodesList);
                    this.goFromCurrentToNeighbour(neighbour);
                    this.orderPathsClients(pathsToOrder);
                    this.updateBestPaths();
                }
            }
        }

        private Optional<NeighbourData> getBestNeighbour(Optional<NeighbourData> firstOpt, Optional<NeighbourData> secondOpt)
        {
            if (!firstOpt.HasValue && !secondOpt.HasValue)
            {
                return firstOpt;
            }
            if (!firstOpt.HasValue)
            {
                return secondOpt;
            }
            if (!secondOpt.HasValue)
            {
                return firstOpt;
            }
            return (firstOpt.Value.cost < secondOpt.Value.cost) ? firstOpt : secondOpt;
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

        private void orderPathsClients(IEnumerable<int> pathsIndexes)
        {
            foreach (var index in pathsIndexes)
            {
                _paths[index] = this.getOrderedPathClients(_paths[index]);
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

        private bool isClientToHeavyToInsert(int clientWeight, int pathWeight)
        {
            return clientWeight + pathWeight > _carCapacity;
        }

        private bool isSwapWeightToBig(int firstClientWeight, int firstPathWeight, int secondClientWeight, int secondPathWeight)
        {
            return (firstPathWeight - firstClientWeight + secondClientWeight > _carCapacity) ||
                   (secondPathWeight - secondClientWeight + firstClientWeight > _carCapacity);
        }

        private Optional<NeighbourData> findBestInsertNeighbour()
        {
            const int minPathSize = 3;
            var bestNeighbour = new NeighbourData(ActionType.Insert);
            for (int pathFrom = 0; pathFrom < _paths.Count; ++pathFrom)
            {
                var Path1 = _paths[pathFrom];
                if (Path1.Count <= minPathSize)
                {
                    continue;
                }
                for (int pathTo = 0; pathTo < _paths.Count; ++pathTo)
                {
                    var Path2 = _paths[pathTo];
                    if (pathTo == pathFrom)
                    {
                        continue;
                    }
                    for (int fromIndex = 1; fromIndex < Path1.Count - 1; ++fromIndex)
                    {
                        if (_tabuList.isOnTabuList(Path1[fromIndex], pathTo) ||
                            this.isClientToHeavyToInsert(_clientsWeights[Path1[fromIndex]], _pathsWeights[pathTo]))
                        {
                            continue;
                        }
                        for (int toIndex = 1; toIndex < Path2.Count; ++toIndex)
                        {
                            var neighbourCost = getInsertNeighbourCost(Path1, fromIndex, Path2, toIndex);
                            if (neighbourCost < bestNeighbour.cost)
                            {
                                bestNeighbour.pathFirst = pathFrom;
                                bestNeighbour.firstIndex = fromIndex;
                                bestNeighbour.pathSecond = pathTo;
                                bestNeighbour.secondIndex = toIndex;
                                bestNeighbour.cost = neighbourCost;
                            }
                        }
                    }
                }
            }
            return (bestNeighbour.cost == NeighbourData.defaultCost) ? new Optional<NeighbourData>() : bestNeighbour;
        }

        private int getInsertNeighbourCost(List<int> Path1, int fromIndex, List<int> Path2, int toIndex)
        {
            var neighbourCost = _currentCost;
            neighbourCost -= _distanceMatrix[Path1[fromIndex - 1]][Path1[fromIndex]];
            neighbourCost -= _distanceMatrix[Path1[fromIndex]][Path1[fromIndex + 1]];
            neighbourCost += _distanceMatrix[Path1[fromIndex - 1]][Path1[fromIndex + 1]];

            neighbourCost -= _distanceMatrix[Path2[toIndex - 1]][Path2[toIndex]];
            neighbourCost += _distanceMatrix[Path2[toIndex - 1]][Path1[fromIndex]];
            neighbourCost += _distanceMatrix[Path1[fromIndex]][Path2[toIndex]];
            return neighbourCost;
        }

        private Optional<NeighbourData> findBestSwapNeighbour()
        {
            var bestNeighbour = new NeighbourData(ActionType.Swap);
            for (int pathFirst = 0; pathFirst < _paths.Count; ++pathFirst)
            {
                var Path1 = _paths[pathFirst];
                for (int pathSecond = 0; pathSecond < _paths.Count; ++pathSecond)
                {
                    var Path2 = _paths[pathSecond];
                    if (pathSecond == pathFirst)
                    {
                        continue;
                    }
                    for (int firstIndex = 1; firstIndex < Path1.Count - 1; ++firstIndex)
                    {
                        if (_tabuList.isOnTabuList(Path1[firstIndex], pathSecond))
                        {
                            continue;
                        }
                        for (int secondIndex = 1; secondIndex < Path2.Count - 1; ++secondIndex)
                        {
                            if (_tabuList.isOnTabuList(Path2[secondIndex], pathFirst) ||
                                this.isSwapWeightToBig(_clientsWeights[Path1[firstIndex]], _pathsWeights[pathFirst],
                                                       _clientsWeights[Path2[secondIndex]], _pathsWeights[pathSecond]))
                            {
                                continue;
                            }

                            var neighbourCost = getSwapNeighbourCost(Path1, firstIndex, Path2, secondIndex);
                            if (neighbourCost < bestNeighbour.cost)
                            {
                                bestNeighbour.pathFirst = pathFirst;
                                bestNeighbour.firstIndex = firstIndex;
                                bestNeighbour.pathSecond = pathSecond;
                                bestNeighbour.secondIndex = secondIndex;
                                bestNeighbour.cost = neighbourCost;
                            }
                        }
                    }
                }
            }
            return (bestNeighbour.cost == NeighbourData.defaultCost) ? new Optional<NeighbourData>() : bestNeighbour;
        }

        private int getSwapNeighbourCost(List<int> Path1, int firstIndex, List<int> Path2, int secondIndex)
        {
            var neighbourCost = _currentCost;
            neighbourCost -= _distanceMatrix[Path1[firstIndex - 1]][Path1[firstIndex]];
            neighbourCost -= _distanceMatrix[Path1[firstIndex]][Path1[firstIndex + 1]];
            neighbourCost += _distanceMatrix[Path1[firstIndex - 1]][Path2[secondIndex]];
            neighbourCost += _distanceMatrix[Path2[secondIndex]][Path1[firstIndex + 1]];

            neighbourCost -= _distanceMatrix[Path2[secondIndex - 1]][Path2[secondIndex]];
            neighbourCost -= _distanceMatrix[Path2[secondIndex]][Path2[secondIndex + 1]];
            neighbourCost += _distanceMatrix[Path2[secondIndex - 1]][Path1[firstIndex]];
            neighbourCost += _distanceMatrix[Path1[firstIndex]][Path2[secondIndex + 1]];
            return neighbourCost;
        }

        private void goFromCurrentToNeighbour(NeighbourData n)
        {
            if (n.neighborType == ActionType.Insert)
            {
                this.goFromCurrentToInsertNeighbour(n);
            }
            else
            {
                this.goFromCurrentToSwapNeighbour(n);
            }
        }

        private void goFromCurrentToInsertNeighbour(NeighbourData n)
        {
            _currentCost = n.cost;
            var clientIndex = _paths[n.pathFirst][n.firstIndex];
            _pathsWeights[n.pathFirst] -= _clientsWeights[_paths[n.pathFirst][n.firstIndex]];
            _pathsWeights[n.pathSecond] += _clientsWeights[_paths[n.pathFirst][n.firstIndex]];
            _paths[n.pathFirst].RemoveAt(n.firstIndex);
            _paths[n.pathSecond].Insert(n.secondIndex, clientIndex);
        }

        private void goFromCurrentToSwapNeighbour(NeighbourData n)
        {
            _currentCost = n.cost;
            _pathsWeights[n.pathFirst] -= _clientsWeights[_paths[n.pathFirst][n.firstIndex]];
            _pathsWeights[n.pathFirst] += _clientsWeights[_paths[n.pathSecond][n.secondIndex]];

            _pathsWeights[n.pathSecond] -= _clientsWeights[_paths[n.pathSecond][n.secondIndex]];
            _pathsWeights[n.pathSecond] += _clientsWeights[_paths[n.pathFirst][n.firstIndex]];

            (_paths[n.pathFirst][n.firstIndex], _paths[n.pathSecond][n.secondIndex]) = (_paths[n.pathSecond][n.secondIndex], _paths[n.pathFirst][n.firstIndex]);
        }

        private void updateBestPaths()
        {
            if (_currentCost >= bestPathsCost) return;

            bestPathsCost = _currentCost;
            bestPaths.Clear();
            foreach (var path in _paths)
            {
                bestPaths.Add(new List<int>(new int[path.Count]));
                for (int i = 0; i < path.Count; ++i)
                {
                    bestPaths[^1][i] = path[i];
                }
            }
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
