using System.Xml.Linq;

namespace SP_AUS_Lib.Structures
{
    public class AVLTree<TKey, TValue> : BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        public AVLTree() : base() { }
        public AVLTree(Comparer<TKey> comparer) : base(comparer) { }

        // Len pre testovanie
        public bool AVLTreeTest()
        {
            Stack<BSTNode<TKey, TValue>> _stack = new(); //zásobník pre uchovávanie cesty

            var _currNode = root;

            while (_currNode != null || _stack.Count > 0) //končí ak zásobník je prázdny a už nenašiel ďalšieho syna
            {
                while (_currNode != null) //posun na najľavejšieho syna
                {
                    _stack.Push(_currNode);
                    _currNode = _currNode.leftSon;
                }

                _currNode = _stack.Pop();

                int _height = _currNode.leftSon?.nodeHeight ?? 0;
                int _right = _currNode.rightSon?.nodeHeight ?? 0;

                if (_right > _height)
                {
                    _height = _right;
                }

                _currNode.nodeHeight = _height + 1;

                int _bf = GetBalanceFactor(_currNode);
                if (_bf < -1 || _bf > 1)
                {
                    Console.WriteLine($"Bf error ({_bf}) node:{_currNode.nodeKey}");
                    return false;
                }
                _currNode = _currNode.rightSon; //prechádza na pravý podstrom
            }

            return true;
        }

        /// <summary>
        /// Inserts a value into the structure with the given key.
        /// </summary>
        public override bool Insert(TKey key, TValue value)
        {
            if (key == null) return false;

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
                int cmp = comparer.Compare(_curr.nodeKey, key);
                _parent = _curr;

                if (cmp < 0)
                {
                    _curr = _curr.rightSon;
                    _isLeft = false;
                }
                else if (cmp > 0)
                {
                    _curr = _curr.leftSon;
                    _isLeft = true;
                }
                else
                {
                    // už existuje
                    return false;
                }
            }

            var _newNode = new BSTNode<TKey, TValue>(key, value)
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

            // úprava výšok a balansovanie
            while (_parent != null)
            {
                int _height = _parent.nodeHeight;

                UpdateHeight(_parent);
                _parent = Rebalance(_parent);

                if (_parent.nodeHeight == _height)
                {
                    break;
                }

                _parent = _parent.parent;
            }

            count++;
            return true;
        }

        /// <summary>
        /// Deletes a value from the stucture found by the given key.
        /// </summary>
        public override bool Delete(TKey key)
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
                    _parent = _predParent;

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

                    }
                }
            }

            // úprava výšok a balansovanie
            while (_parent != null)
            {
                int _height = _parent.nodeHeight;

                UpdateHeight(_parent);
                _parent = Rebalance(_parent);

                if (_parent.nodeHeight == _height)
                {
                    break;
                }

                _parent = _parent.parent;
            }

            count--;
            return true;
        }

        private BSTNode<TKey, TValue> RotateNodeLeft(BSTNode<TKey, TValue> node)
        {
            var _right = node.rightSon;
            var _rightLeft = _right?.leftSon;

            if (_right == null)
            {
                return node;
            }

            _right.parent = node.parent;

            if (node.parent == null)
            {
                root = _right;
            }
            else if (node.parent.leftSon == node)
            {
                node.parent.leftSon = _right;
            }
            else
            {
                node.parent.rightSon = _right;
            }

            _right.leftSon = node;
            node.parent = _right;

            node.rightSon = _rightLeft;
            if (_rightLeft != null) _rightLeft.parent = node;

            UpdateHeight(node);
            UpdateHeight(_right);

            return _right; //vráti koreň tohto podstromu
        }

        private BSTNode<TKey, TValue> RotateNodeRight(BSTNode<TKey, TValue> node)
        {
            var _left = node.leftSon;
            var _leftRight = _left?.rightSon;

            if (_left == null)
            {
                return node;
            }

            _left.parent = node.parent;
            if (node.parent == null)
            {
                root = _left;
            }
            else if (node.parent.leftSon == node)
            {
                node.parent.leftSon = _left;
            }
            else
            {
                node.parent.rightSon = _left;
            }

            _left.rightSon = node;
            node.parent = _left;

            node.leftSon = _leftRight;
            if (_leftRight != null) _leftRight.parent = node;

            UpdateHeight(node);
            UpdateHeight(_left);

            return _left; //vráti koreň tohto podstromu
        }

        private void UpdateHeight(BSTNode<TKey, TValue>? node)
        {
            if (node == null)
            {
                return;
            }

            int _height = node.leftSon?.nodeHeight ?? 0;
            int _right = node.rightSon?.nodeHeight ?? 0;

            if (_right > _height)
            {
                _height = _right;
            }

            node.nodeHeight = _height + 1;
        }

        private int GetBalanceFactor(BSTNode<TKey, TValue>? node)
        {
            if (node == null)
            {
                return 0;
            }

            int _left = node.leftSon?.nodeHeight ?? 0;
            int _right = node.rightSon?.nodeHeight ?? 0;

            return _left - _right;
        }

        private BSTNode<TKey, TValue>? Rebalance(BSTNode<TKey, TValue>? node)
        {
            if (node == null) return null;

            int _bf = GetBalanceFactor(node);

            // LL
            if (_bf > 1 && GetBalanceFactor(node.leftSon) >= 0)
                return RotateNodeRight(node);

            // LR
            if (_bf > 1 && GetBalanceFactor(node.leftSon) < 0)
            {
                RotateNodeLeft(node.leftSon);
                return RotateNodeRight(node);
            }

            // RR
            if (_bf < -1 && GetBalanceFactor(node.rightSon) <= 0)
                return RotateNodeLeft(node);

            // RL
            if (_bf < -1 && GetBalanceFactor(node.rightSon) > 0)
            {
                RotateNodeRight(node.rightSon);
                return RotateNodeLeft(node);
            }

            return node;
        }
    }
}
