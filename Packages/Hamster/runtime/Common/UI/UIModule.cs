using System;

namespace Hamster {

    public class UIModule {
        public class UIModuleAttribute<T> : IPool {
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
            public static void Free(UIModuleAttribute<T> obj) {
                ObjectPool<UIModuleAttribute<T>>.Free(obj);
            }
            public static UIModuleAttribute<T> Malloc() {
                return ObjectPool<UIModuleAttribute<T>>.Malloc();
            }
        }

        public virtual void Finish() {
             
        }
    }
}
