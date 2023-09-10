using System.Collections;
using SPSL.Language.Parsing.AST;

namespace SPSL.Language.Utils;

public sealed class OrderedSet<T> : ICollection<T>, IEquatable<OrderedSet<T>> where T : INode
{
    private readonly IDictionary<T, LinkedListNode<T>?> _dictionary;
    private readonly LinkedList<T> _linkedList;

    public OrderedSet(IEnumerable<T>? items = null)
        : this(EqualityComparer<T>.Default, items)
    {
    }

    public OrderedSet(IEqualityComparer<T> comparer, IEnumerable<T>? items = null)
    {
        _dictionary = new Dictionary<T, LinkedListNode<T>?>(comparer);
        _linkedList = new LinkedList<T>();

        if (items == null) return;

        AddRange(items);
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => _dictionary.IsReadOnly;

    public override int GetHashCode()
    {
        return _linkedList.Aggregate(0, (current, item) => HashCode.Combine(current, item.GetHashCode()));
    }

    public int GetSemanticHashCode<TNode>() where TNode : T, ISemanticallyEquatable<T>
    {
        return _linkedList.Aggregate(0,
            (current, item) => HashCode.Combine(current, ((TNode)item).GetSemanticHashCode()));
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (T item in items)
            Add(item);
    }

    public bool Add(T item)
    {
        if (_dictionary.ContainsKey(item)) return false;
        var node = _linkedList.AddLast(item);
        _dictionary.Add(item, node);
        return true;
    }

    public bool Prepend(T item)
    {
        if (_dictionary.ContainsKey(item)) return false;
        var node = _linkedList.AddFirst(item);
        _dictionary.Add(item, node);
        return true;
    }

    public bool Append(T item)
    {
        return Add(item);
    }

    public void Clear()
    {
        _linkedList.Clear();
        _dictionary.Clear();
    }

    public bool Remove(T item)
    {
        bool found = _dictionary.TryGetValue(item, out LinkedListNode<T>? node);
        if (!found || node == null) return false;
        _dictionary.Remove(item);
        _linkedList.Remove(node);
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _linkedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(T item)
    {
        return _dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _linkedList.CopyTo(array, arrayIndex);
    }

    public T this[uint i]
    {
        get
        {
            uint v = 0;
            foreach (T item in _linkedList)
            {
                if (v == i) return item;
                v++;
            }

            throw new ArgumentOutOfRangeException(nameof(i), i, "Invalid index.");
        }
    }

    public bool Equals(OrderedSet<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        for (int i = _linkedList.Count - 1; i >= 0; i--)
            if (!_linkedList.ElementAt(i).Equals(other._linkedList.ElementAt(i)))
                return false;

        return true;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is OrderedSet<T> other && Equals(other);
    }
}