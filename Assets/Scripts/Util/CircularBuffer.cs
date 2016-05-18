using System.Collections.Generic;
using UnityEngine;

public class CircularQueue<T>
{
	Queue<T> _queue;
	int _size;

	public CircularQueue(int size)
	{
		_queue = new Queue<T>(size);
		_size = size;
	}

	public void Add(T obj)
	{
		if (_queue.Count == _size)
		{
			_queue.Dequeue();
			_queue.Enqueue(obj);
		}
		else
			_queue.Enqueue(obj);
	}

	public T Read()
	{
		return _queue.Dequeue();
	}

	public T Peek()
	{
		return _queue.Peek();
	}
}

public class CircularList<T>
{
	List<T> _list;
	int _capacity;
	int _start;
	int _pos;

	public CircularList(int capacity)
	{
		_list = new List<T>(capacity);
		_capacity = capacity;
		_start = 0;
		_pos = 0;
	}

	public void Add(T obj)
	{
		if (_list.Count == _capacity) {
			_list [_pos] = obj;
			_start++; _start %= _list.Count;
			_pos++; _pos %= _list.Count;
		} else {
            _list.Add(obj);
        }
	}

	public T Get(int index)
	{
		if (_list.Count == 0)
			return default(T);
		int idx = _start + index;
		idx %= _list.Count;
		return _list [idx];
    }

    public T GetFromEnd(int indexFromEnd) {
        if (_list.Count == 0)
            return default(T);
        int idx = _start + _list.Count - indexFromEnd - 1;
        idx %= _list.Count;
        return _list[idx];
    }

    public int Capacity {
		get {
			return _list.Capacity;
		}
	}

    public T[] Slice(int start, int end) {
        T[] sliced = new T[end - start];
        int copyCount = 0;
        for (int i = start; i < end && i < _list.Count; i++) {
            sliced[i - start] = _list[i];
            copyCount++;
        }
        if (copyCount < sliced.Length) {
            T[] slicedSmaller = new T[copyCount];
            for (int i = 0; i < copyCount; i++) {
                slicedSmaller[i] = sliced[i];
            }
            sliced = slicedSmaller;
        }
        return sliced;
    }
    /// <summary> Returns a right-justified slice window on the array of length (end-start) </summary>
    public T[] TailSlice(int start, int end) {
        int offset = _list.Count - end;
        return Slice(start + offset, end + offset);
    }
}