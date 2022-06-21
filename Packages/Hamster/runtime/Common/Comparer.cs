using System.Collections.Generic;

namespace Hamster {
    public class Int32Comparer : System.Collections.Generic.IEqualityComparer<int> {
        bool IEqualityComparer<int>.Equals(int x, int y) {
            return x == y;
        }

        int IEqualityComparer<int>.GetHashCode(int obj) {
            return obj;
        }
    }

}
