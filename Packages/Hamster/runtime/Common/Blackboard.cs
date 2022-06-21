using System;
using System.Collections.Generic;
using Hamster;

public interface IBackboardVar {
    Type GetValueType();
    string GetName();
    void SetName(string name);

    void Free();

#if UNITY_EDITOR
    void SetValue(object value);
    object GetValueObject();
#endif
}

public class BackboardVar<T> : IBackboardVar, IPool {
    protected T _value = default;
    protected string _name = string.Empty;

    public T GetValue() {
        return _value;
    }

    public void SetValue(T value) {
        _value = value;
    }

    public void SetName(string name) {
        _name = name;
    }

    public string GetName() {
        return _name;
    }

    public Type GetValueType() {
        return typeof(T);
    }

    public void Reset() {
        _value = default;
        _name = string.Empty;
    }

    public void Free() {
        Free(this);
    }

    public static BackboardVar<T> Malloc() {
        return ObjectPool<BackboardVar<T>>.Malloc();
    }

    public static void Free(IBackboardVar inst) {
        ObjectPool<BackboardVar<T>>.Free(inst as BackboardVar<T>);
    }

#if UNITY_EDITOR

    public void SetValue(object value) {
        _value = (T)value;
    }

    public object GetValueObject() {
        return _value;
    }

#endif
}

public class Blackboard {
    protected Dictionary<int, int> _data = new Dictionary<int, int>(new Int32Comparer());
    protected Dictionary<string, IBackboardVar> _vars = new Dictionary<string, IBackboardVar>();

    private event Action<int, int> _onModifyValue;
    private event Action<string, IBackboardVar> _onModifyVar;

    public void BindModifyValue(Action<int, int> onModifyValue) {
        _onModifyValue += onModifyValue;
    }

    public void BindModifyValue(Action<string, IBackboardVar> onModifyVar) {
        _onModifyVar += onModifyVar;
    }

    public void UnbindModifyValue(Action<int, int> onModifyValue) {
        _onModifyValue -= onModifyValue;
    }

    public void UnbindModifyValue(Action<string, IBackboardVar> onModifyVar) {
        _onModifyVar -= onModifyVar;
    }

    public bool TryGetValue(int key, out int value) {
        return _data.TryGetValue(key, out value);
    }

    public bool TryGetValue<T>(string name, out T value) {
        value = default;
        if (_vars.TryGetValue(name, out IBackboardVar bbVar)) {
            value = (bbVar as BackboardVar<T>).GetValue();
            return true;
        }
        return false;
    }

    public void SetValue(int key, int value) {
        _data[key] = value;
        _onModifyValue?.Invoke(key, value);
    }

    public void SetValue<T>(string name, T value) {
        if (_vars.TryGetValue(name, out IBackboardVar bbVar)) {
            BackboardVar<T> varInst = bbVar as BackboardVar<T>;
            varInst.SetName(name);
            varInst.SetValue(value);
            _onModifyVar?.Invoke(name, bbVar);
        }
        else {
            BackboardVar<T> varInst = BackboardVar<T>.Malloc();
            varInst.SetValue(value);
            varInst.SetName(name);
            _vars.Add(name, varInst);
            _onModifyVar?.Invoke(name, bbVar);
        }
    }

    public bool HasValue(int key) {
        return _data.ContainsKey(key);
    }

    public bool HasValue(string name) {
        return _vars.ContainsKey(name);
    }

    public void DelValue(int key) {
        if (_data.TryGetValue(key, out int value)) {
            _data.Remove(key);
            _onModifyValue?.Invoke(key, value);
        }
    }

    public void DelValue(string name) {
        if (_vars.TryGetValue(name, out IBackboardVar value)) {
            _vars.Remove(name);
            value.Free();
            _onModifyVar?.Invoke(name, value);
        }
    }

    public void Clean() {
        var it = _vars.GetEnumerator();
        while (it.MoveNext()) {
            it.Current.Value.Free();
        }

        _data.Clear();
        _vars.Clear();
    }
}
