using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntypedEvent
{
    public delegate void SignalListener();

    private readonly List<SignalListener> _listeners = new List<SignalListener>(1);

    public void AddListener(SignalListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    public void RemoveListener(SignalListener listener)
    {
        _listeners.Remove(listener);
    }

    public void Clear()
    {
        _listeners.Clear();
    }

    public void Dispatch()
    {
        for (int i = 0; i < _listeners.Count; ++i)
        {
            _listeners[i]();
        }
    }
}

public class TypedEvent<T>
{
    public delegate void SignalListener(T parameter);

    private readonly List<SignalListener> _listeners = new List<SignalListener>(1);

    public void AddListener(SignalListener listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    public void RemoveListener(SignalListener listener)
    {
        _listeners.Remove(listener);
    }

    public void Clear()
    {
        _listeners.Clear();
    }

    public void Dispatch(T parameter)
    {
        for (int i = 0; i < _listeners.Count; ++i)
        {
            _listeners[i](parameter);
        }
    }
}
