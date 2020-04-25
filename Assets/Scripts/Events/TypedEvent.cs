﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Dispatch(T parameter)
    {
        for (int i = 0; i < _listeners.Count; ++i)
        {
            _listeners[i](parameter);
        }
    }
}
