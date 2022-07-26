namespace Hamster {

    public class LoopQueueNode<T> : IPool {
        public LoopQueueNode<T> Next;
        public T Value;

        public void Reset() {
            Next = null;
            Value = default(T);
        }
    }

    public class LoopQueue<T> {

        private int _dataSize = 0;
        private LoopQueueNode<T> _fron = null;
        private LoopQueueNode<T> _tail = null;


        public void Reset() {
            LoopQueueNode<T> temp = _fron;
            while (null != temp) {
                LoopQueueNode<T> value = temp;
                temp = temp.Next;
                ObjectPool<LoopQueueNode<T>>.Free(temp);
            }

            _fron = null;
            _tail = null;
            _dataSize = 0;
        }

        public void Push(T value) {
            LoopQueueNode<T> node = _tail;
            if (null == _tail) {
                node = ObjectPool<LoopQueueNode<T>>.Malloc();
                _fron = node;
                _tail = node;
            }
            else {
                node = ObjectPool<LoopQueueNode<T>>.Malloc();
                _tail.Next = node;
                _tail = node;
            }
            node.Value = value;
            _dataSize++;
        }

        public bool Pop(out T value) {
            value = default;
            if (null == _fron)
                return false;

            value = _fron.Value;

            LoopQueueNode<T> node = _fron;
            _fron = _fron.Next;
            ObjectPool<LoopQueueNode<T>>.Free(node);

            return true;
        }

        public int GetDataSize() {
            return _dataSize;
        }
    }
}
