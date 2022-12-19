using Hamster;
using UnityEngine;

public class A : UnityEngine.ScriptableObject {
    public B b = null;
}

public class B : UnityEngine.ScriptableObject {
    public int a = 0;
}

[ExecuteInEditMode]
public class TestScript : MonoBehaviour {
    public AIBehaviourScript Script = null;
    public AIBehaviourRunner AIBehaviour = new AIBehaviourRunner();

    public void Awake() {
        AIBehaviour.Initialize(Script, gameObject);
    }

    public void Update() {
        AIBehaviour.Execute(Time.deltaTime);
    }
}
