using Hamster;
using UnityEngine;

[ExecuteInEditMode]
public class TestScript : MonoBehaviour {

    public bool ExecuteLoopQueue = false;
    private void TestLoopQueue() {
        LoopQueue<int> loopQueue = new LoopQueue<int>();

        loopQueue.Push(1);
        loopQueue.Push(2);
        loopQueue.Push(3);
        loopQueue.Push(4);
        loopQueue.Push(5);
        loopQueue.Push(6);
        loopQueue.Push(7);
        loopQueue.Push(8);

        for (int i = 0; i < 9; i++) {
            bool result = loopQueue.Pop(out int value);
            Debug.Log("===> " + result + ", " + value);
        }
    }
    
    public void Update() {
        if (ExecuteLoopQueue) {
            TestLoopQueue();
            ExecuteLoopQueue = false;
        }
    }
}
