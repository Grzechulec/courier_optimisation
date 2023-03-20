namespace courierOptimisation
{
    public class PathsFinder
    {
        private List<List<uint>> distanceMatrix_;
        private List<int> clientsWeights_ = new() {9, 4, 1, 10, 5};
        public List<List<int>> Paths { get; private set; } = new();
        private const int carCapacity_ = 20;

        public PathsFinder()
        {
            distanceMatrix_ = new List<List<uint>>()
            {
                new List<uint>() {0, 2 , 2, 8, 9, 8},
                new List<uint>() {2, 0, 3, 12, 10, 5},
                new List<uint>() {2, 3, 0, 6, 10, 5},
                new List<uint>() {8, 12, 6, 0, 1, 3},
                new List<uint>() {9, 10, 10, 1, 0, 2},
                new List<uint>() {8, 5, 12, 3, 2, 0}
            };
        }

        public void generateInitialPaths() {
            var weightsIndex = new List<Tuple<int, int>>();
            for (int i = 0; i < clientsWeights_.Count; ++i) 
            {
                weightsIndex.Add(new Tuple<int, int>(clientsWeights_[i], i));   
            }
            weightsIndex = weightsIndex.OrderByDescending(x => x.Item1).ToList();

            while (weightsIndex.Count > 0) 
            {
                var capLeft = carCapacity_;
                Paths.Add(new List<int>());
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
                    Paths.Last().Add(weightsIndex[maxIndex].Item2);
                    weightsIndex.RemoveAt(maxIndex);
                    capLeft -= weightsIndex[maxIndex].Item1;
                }
            }
        }
    }
}
