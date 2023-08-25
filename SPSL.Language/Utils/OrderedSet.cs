using System.Collections;

namespace SPSL.Language.Utils;

public sealed class OrderedSet<T> : ICollection<T> where T : notnull
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

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item);
    }

    public bool Add(T item)
    {
        if (_dictionary.ContainsKey(item)) return false;
        LinkedListNode<T>? node = _linkedList.AddLast(item);
        _dictionary.Add(item, node);
        return true;
    }

    public bool Prepend(T item)
    {
        if (_dictionary.ContainsKey(item)) return false;
        LinkedListNode<T>? node = _linkedList.AddFirst(item);
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
        var found = _dictionary.TryGetValue(item, out LinkedListNode<T>? node);
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
            foreach (var item in _linkedList)
            {
                if (v == i) return item;
                v++;
            }

            throw new ArgumentOutOfRangeException(nameof(i));
        }
    }
}