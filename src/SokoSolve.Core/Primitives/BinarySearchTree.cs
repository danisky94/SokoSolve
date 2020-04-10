using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Primitives
{
    public class BinarySearchTree<T> : IReadOnlyCollection<T>
    {
        public class Node
        {
            public Node(T value)
            {
                Value = value;
            }

            public T Value { get; }
            public Node? Parent { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }
            public EqualNode? Equal { get; set; }

            public override string ToString()
            {
                var c = 0;
                var eq = Equal;
                while (eq != null)
                {
                    c++;
                    eq = eq.Next;
                }
                return new FluentStringBuilder(" ")
                    .IfNotNull(Left, x=>$"({Left}").Sep()
                    .Append(Value.ToString()).Sep()
                    .If(c > 0, $"={c}").Sep()
                    .IfNotNull(Right, x=>$"({Right}")
                    ;
            }
        }

        public class EqualNode
        {
            public EqualNode(T value)
            {
                Value = value;
            }

            public T Value { get; }
            public EqualNode? Next { get; set; }
        }

        private volatile int count;
        private volatile int countEqual;
        private readonly IComparer<T> compare;

        public BinarySearchTree(IComparer<T> compare)
        {
            this.compare = compare;
        }

        public Node? Root { get; private set; }
        public int Count => count;

        public IEnumerable<Node> GetNodes()
        {
            var x = new List<Node>();
            Recurse(x, Root);
            return x;
            
            void Recurse(List<Node> res, Node r)
            {
                if (r == null) return;
                Recurse(res, r.Left);
                res.Add(r);
                Recurse(res, r.Right);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var x = new List<T>();
            Recurse(x, Root);
            return x.GetEnumerator();
            
            void Recurse(List<T> res, Node r)
            {
                if (r == null) return;
                Recurse(res, r.Left);
                res.Add(r.Value);
                var ee = r.Equal;
                while (ee != null)
                {
                    res.Add(ee.Value);
                    ee = ee.Next;
                }
                Recurse(res, r.Right);
            }
        }  
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public Node Add(T item)
        {
            Debug.Assert(item != null);
            Interlocked.Increment(ref count);
            return Root == null 
                ? Root = new Node(item)
                : AddInner(Root, item);
        }

        Node AddInner(Node n, T value)
        {
            var c = compare.Compare(value, n.Value);
            if (c > 0)
            {
                if (n.Right == null)
                {
                    n.Right = new Node(value)
                    {
                        Parent = n
                    };
                    return n.Right;
                }
                else
                {
                    return AddInner(n.Right, value);
                }
            }
            else if (c < 0)
            {
                if (n.Left == null)
                {
                    n.Left = new Node(value)
                    {
                        Parent = n
                    };
                    return n.Left;
                }
                else
                {
                    return AddInner(n.Left, value);
                }
            }
            else // equal
            {
                Interlocked.Increment(ref countEqual);
                if (n.Equal == null) n.Equal = new EqualNode(value);
                else
                {
                    var nn = n.Equal;
                    while (nn.Next != null) nn = nn.Next;
                    nn.Next = new EqualNode(value);
                }

                return n;
            }
        }
        
        public void AddRange(IOrderedEnumerable<T> item)
        {
            foreach (var i in item)
            {
                Add(i);
            }
        }

        public Node GetMin()
        {
            if (Root == null) return null;
            var n = Root;
            while (n.Left != null)
            {
                n = n.Left;
            }
            return n;
        } 
        
        public Node GetMax()
        {
            if (Root == null) return null;
            var n = Root;
            while (n.Right != null)
            {
                n = n.Right;
            }
            return n;
        } 


    }
}