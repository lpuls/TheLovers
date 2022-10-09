using System;

namespace Hamster {
    public class ModityValue<T> : IPool {
        private T _value = default;
        public Action<T, T> OnValueChange = null;

        public T GetValue() {
            return _value;
        }

        public void SetValue(T v) {
            OnValueChange?.Invoke(_value, v);
            _value = v;
        }

        public void Reset() {
            _value = default;
        }
        public static void Free(ModityValue<T> obj) {
            ObjectPool<ModityValue<T>>.Free(obj);
        }
        public static ModityValue<T> Malloc() {
            return ObjectPool<ModityValue<T>>.Malloc();
        }
    }

    public class ModifyProperty<T> : IPool {
        private T _value = default;
        private T _maxValue = default;
        private float _magnification = 1.0f;

        public Action<T, T> OnValueChange = null;
        public Action<T, T> OnMaxValueChange = null;
        public Action<float, float> OnMagnificationChange = null;

        public void Init(T value, T maxValue, float magnification = 1.0f) {
            _value = value;
            _maxValue = maxValue;
            _magnification = magnification;
        }

        public T GetValue() {
            return _value;
        }

        public void SetValue(T v) {
            OnValueChange?.Invoke(_value, v);
            _value = v;
        }

        public T GetMaxValue() {
            return _maxValue;
        }

        public void SetMaxValue(T v) {
            OnMaxValueChange?.Invoke(_maxValue, v);
            _maxValue = v;
        }

        public float GetMagnification() {
            return _magnification;
        }

        public void SetMagnification(float v) {
            OnMagnificationChange?.Invoke(_magnification, v);
            _magnification = v;
        }

        public void Reset() {
            _value = default;
            _maxValue = default;
            _magnification = 1.0f;
        }
    }

}
