using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JList<T>
{
    public Action OnListChanged;
    [SerializeField] List<T> list = new List<T>();

    public void Add(T item)
    {
        list.Add(item);
        OnListChanged?.Invoke();
    }

    public void Remove(T item)
    {
        list.Remove(item);
        OnListChanged?.Invoke();
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
        OnListChanged?.Invoke();
    }

    public void Clear()
    {
        list.Clear();
        OnListChanged?.Invoke();
    }

    public bool Contains(T item) => list.Contains(item);

    public int Count() => list.Count;
}
