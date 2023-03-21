namespace courierOptimisation
{
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

    class TabuList
    {
        private const int _listSize = 14;
        private List<TabuNode> _list;
        private int _currentIndex = 0;

        public TabuList()
        {
            _list = new(new TabuNode[_listSize]);
            for (int i = 0; i < _listSize; ++i)
            {
                _list[i] = new TabuNode();
            }
        }

        public void updateTabuList(List<TabuNode> nodes)
        {
            foreach (var node in nodes)
            {
                _list[_currentIndex] = node;
                ++_currentIndex;
                if (_currentIndex >= _list.Count)
                {
                    _currentIndex = 0;
                }
            }
        }

        public bool isOnTabuList(int clientIndex, int pathIndex)
        {
            return _list.FindIndex(node => node.clientIndex == clientIndex && node.pathIndex == pathIndex) >= 0;
        }

        public void resetTabuList()
        {
            for (int i = 0; i < _list.Count; ++i)
            {
                _list[i].clientIndex = TabuNode.initVal;
                _list[i].pathIndex = TabuNode.initVal;
            }
            _currentIndex = 0;
        }
    }
}
