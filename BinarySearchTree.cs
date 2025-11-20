namespace SP_AUS_Lib.Structures
{
    public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        protected class BSTNode<NKey, NValue> where NKey : IComparable<NKey>
        {
            public BSTNode<NKey, NValue>? parent;
            public BSTNode<NKey, NValue>? leftSon;
            public BSTNode<NKey, NValue>? rightSon;
            public NKey nodeKey;
            public NValue nodeValue;
            public int nodeHeight;

            public BSTNode(NKey nodeKey, NValue nodeValue)
            {
                parent = null;
                leftSon = null;
                rightSon = null;
                this.nodeKey = nodeKey;
                this.nodeValue = nodeValue;
                nodeHeight = 1;
            }

            public virtual void CopyDataFromNode(BSTNode<NKey, NValue> other)
            {
                nodeKey = other.nodeKey;
                nodeValue = other.nodeValue;
            }

            public bool DeleteSon(BSTNode<NKey, NValue> son)
            {
                if (son == leftSon)
                {
                    leftSon = null;
                    return true;
                }

                if (son == rightSon)
                {
                    rightSon = null;
                }
                return false;
            }
        }

        protected BSTNode<TKey, TValue>? root;
        protected int count;
        protected Comparer<TKey> comparer;

        public BinarySearchTree()
        {
            root = null;
            count = 0;
            comparer = Comparer<TKey>.Default;
        }

        public BinarySearchTree(Comparer<TKey> comparer)
        {
            root = null;
            count = 0;
            this.comparer = comparer;
        }

        /// <summary>
        /// Returns the root value of the structure.
        /// </summary>
        public TValue? Root { get { return root != null ? root.nodeValue : default; } }

        /// <summary>
        /// Returns the number of elements in the structure.
        /// </summary>
        public int Count
        {
            get => count;
        }

        /// <summary>
        /// Returns or sets the value found at the given key.
        /// </summary>
        public TValue? this[TKey key]
        {
            get
            {
                Find(key, out TValue? value);
                return value;
            }
            set
            {
                var _node = FindNode(key);
                if (_node != null && value != null)
                {
                    _node.nodeValue = value;
                }
            }
        }

        /// <summary>
        /// Clears the structure.
        /// </summary>
        public void Clear()
        {
            root = null;
        }

        /// <summary>
        /// Returns the minimum value in the structure.
        /// </summary>
        public TValue? Min()
        {
            if (root == null)
            {
                return default;
            }

            var _currentNode = root;

            while (_currentNode.leftSon != null)
            {
                _currentNode = _currentNode.leftSon;
            }

            return _currentNode.nodeValue;
        }

        /// <summary>
        /// Returns the maximum value in the structure.
        /// </summary>
        public TValue? Max()
        {
            if (root == null)
            {
                return default;
            }

            var _currentNode = root;

            while (_currentNode.rightSon != null)
            {
                _currentNode = _currentNode.rightSon;
            }

            return _currentNode.nodeValue;
        }

        /// <summary>
        /// Returns the value found by the given key.
        /// </summary>
        public virtual bool Find(TKey key, out TValue? value)
        {
            var _node = FindNode(key);
            if (_node != null)
            {
                value = _node.nodeValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Returns a list of values in given range, defined by keys, from the structure.
        /// </summary>
        public virtual List<TValue> Find(TKey from, TKey to, Predicate<TValue>? predicate = null)
        {

            List<TValue> _findList = new();

            if (root == null)
            {
                return _findList;
            }

            var _currNode = root;

            //stavy, 0 - prichádza z rodiča, 1 - prichádza z ľava, 2 - prichádza z prava
            byte _state = 0;

            while (_currNode != null)
            {
                int _cmpFrom = comparer.Compare(_currNode.nodeKey, from);
                int _cmpTo = comparer.Compare(_currNode.nodeKey, to);
                if (_state == 0)
                {
                    // Posun na najľavejšieho syna
                    if (_currNode.leftSon != null && _cmpFrom > 0)
                    {
                        _state = 0;
                        _currNode = _currNode.leftSon;
                    }
                    else
                    {
                        if (_cmpFrom >= 0 && _cmpTo <= 0 && (predicate == null || predicate(_currNode.nodeValue)))
                        {
                            _findList.Add(_currNode.nodeValue);
                        }

                        // Ak má pravého syna, ide tam, inak hore
                        if (_currNode.rightSon != null && _cmpTo < 0)
                        {
                            _state = 0;
                            _currNode = _currNode.rightSon;
                        }
                        else
                        {
                            _state = 2;
                        }
                    }
                }
                else if (_state == 1)
                {
                    if (_cmpFrom >= 0 && _cmpTo <= 0)
                    {
                        _findList.Add(_currNode.nodeValue);
                    }

                    if (_currNode.rightSon != null && _cmpTo < 0)
                    {
                        _state = 0;
                        _currNode = _currNode.rightSon;
                    }
                    else
                    {
                        _state = 2;
                    }
                }
                else
                {
                    if (_currNode.parent == null)
                    {
                        _currNode = null;
                    }
                    else if (_currNode == _currNode.parent.leftSon)
                    {
                        _currNode = _currNode.parent;
                        _state = 1;
                    }
                    else
                    {
                        _currNode = _currNode.parent;
                        _state = 2;
                    }
                }
            }

            return _findList;
        }

        /// <summary>
        /// Inserts a value into the structure with the given key.
        /// </summary>
        public virtual bool Insert(TKey key, TValue value)
        {
            if (key == null)
            {
                return false;
            }

            if (root == null)
            {
                root = new(key, value);
                count++;
                return true;
            }

            BSTNode<TKey, TValue>? _parent = null;
            BSTNode<TKey, TValue>? _curr = root;
            bool _isLeft = false;

            while (_curr != null)
            {
                int _cmpVal = comparer.Compare(_curr.nodeKey, key);

                _parent = _curr;
                if (_cmpVal < 0)
                {
                    _curr = _curr.rightSon;
                    _isLeft = false;
                }
                else if (_cmpVal > 0)
                {
                    _curr = _curr.leftSon;
                    _isLeft = true;
                }
                else
                {
                    return false;
                }
            }

            BSTNode<TKey, TValue> _newNode = new(key, value)
            {
                parent = _parent
            };

            if (_isLeft)
            {
                _parent.leftSon = _newNode;
            }
            else
            {
                _parent.rightSon = _newNode;
            }

            count++;
            return true;
        }

        /// <summary>
        /// Deletes a value from the stucture found by the given key.
        /// </summary>
        public virtual bool Delete(TKey key)
        {
            var _node = FindNode(key);

            //ak sa hľadaná hodnota v strome nenachádza
            if (_node == null)
            {
                return false;
            }

            var _parent = _node.parent;

            //ak vrchol je listom, vymaže sa
            if (_node.leftSon == null && _node.rightSon == null)
            {
                if (_parent == null)
                {
                    root = null;
                }
                else
                {
                    _parent.DeleteSon(_node);
                }
            }
            //ak má vrchol 1 syna, syn sa premiestni na miesto mazaného vrcholu
            else if (_node.leftSon == null || _node.rightSon == null)
            {
                var _son = _node.leftSon ?? _node.rightSon;

                if (_parent == null)
                {
                    root = _son;
                    if (root != null)
                    {
                        root.parent = null;
                    }
                }
                else
                {
                    if (_parent.leftSon == _node)
                    {
                        _parent.leftSon = _son;
                    }
                    else
                    {
                        _parent.rightSon = _son;
                    }
                    if (_son != null)
                    {
                        _son.parent = _parent;
                    }
                }
            }
            //ak má vrchol 2 synov, nájde sa predchodca s ktorým vymení hodnoty a predchodcov ľavý syn sa prelinkuje na následíkovho otca
            else
            {
                var _pred = GetPredecessor(_node);
                if (_pred != null)
                {
                    _node.CopyDataFromNode(_pred);
                    var _predParent = _pred.parent;

                    if (_predParent != null)
                    {
                        if (_predParent.leftSon == _pred)
                        {
                            _predParent.leftSon = _pred.leftSon;
                        }
                        else
                        {
                            _predParent.rightSon = _pred.leftSon;
                        }

                        if (_pred.leftSon != null)
                        {
                            _pred.leftSon.parent = _predParent;
                        }

                        _parent = _predParent;
                    }
                }
            }

            count--;
            return true;
        }

        /// <summary>
        /// Returns a list containing all elements of the structure in sorted order.
        /// </summary>
        public List<TValue> InOrderTraversal()
        {
            List<TValue> _inOrderList = new();

            if (root == null)
            {
                return _inOrderList;
            }

            var _currNode = root;

            //stavy, 0 - prichádza z rodiča, 1 - prichádza z ľava, 2 - prichádza z prava
            byte _state = 0;

            while (_currNode != null)
            {
                if (_state == 0)
                {
                    // Posun na najľavejšieho syna z otca
                    if (_currNode.leftSon != null)
                    {
                        _state = 0;
                        _currNode = _currNode.leftSon;
                    }
                    else
                    {
                        _inOrderList.Add(_currNode.nodeValue);

                        if (_currNode.rightSon != null)
                        {
                            // Posun na pravého syna z otca
                            _state = 0;
                            _currNode = _currNode.rightSon;
                        }
                        else
                        {
                            // Vracia sa od pravého syna
                            _state = 2;
                        }
                    }
                }
                else if (_state == 1)
                {
                    _inOrderList.Add(_currNode.nodeValue);

                    if (_currNode.rightSon != null)
                    {
                        // Posun na pravého syna z ľavého
                        _state = 0;
                        _currNode = _currNode.rightSon;
                    }
                    else
                    {
                        // Posun na otca z pravého syna
                        _state = 2; 
                    }
                }
                else
                {
                    if (_currNode.parent == null)
                    {
                        _currNode = null;
                    }
                    else if (_currNode == _currNode.parent.leftSon)
                    {
                        // Posun na otca z ľavého syna
                        _currNode = _currNode.parent;
                        _state = 1;
                    }
                    else
                    {
                        // Posun na otca z pravého syna
                        _currNode = _currNode.parent;
                        _state = 2;
                    }
                }
            }

            return _inOrderList;
        }

        /// <summary>
        /// Returns a list containing the level order traversal of the structure.
        /// </summary>
        public List<TValue> LevelOrderTraversal()
        {
            List<TValue>? _levelOrderList = new();

            if (root == null)
            {
                return _levelOrderList;
            }

            Queue<BSTNode<TKey, TValue>> _queue = new();

            _queue.Enqueue(root);

            while (_queue.Count > 0)
            {
                var _node = _queue.Dequeue();
                _levelOrderList.Add(_node.nodeValue);

                if (_node.leftSon != null)
                {
                    _queue.Enqueue(_node.leftSon);
                }
                if (_node.rightSon != null)
                {
                    _queue.Enqueue(_node.rightSon);
                }
            }

            return _levelOrderList;
        }

        protected BSTNode<TKey, TValue>? FindNode(TKey key)
        {
            BSTNode<TKey, TValue>? _currNode = root;
            int _cmpVal;

            while (true)
            {
                if (_currNode == null)
                {
                    return null;
                }

                _cmpVal = comparer.Compare(_currNode.nodeKey, key);

                if (_cmpVal < 0)
                {
                    _currNode = _currNode.rightSon;
                }
                else if (_cmpVal > 0)
                {
                    _currNode = _currNode.leftSon;
                }
                else
                {
                    return _currNode;
                }
            }
        }

        protected BSTNode<TKey, TValue>? GetPredecessor(BSTNode<TKey, TValue> node)
        {
            BSTNode<TKey, TValue>? _curr = node.leftSon;

            while (_curr != null && _curr.rightSon != null)
            {
                _curr = _curr.rightSon;
            }

            return _curr;
        }
    }
}