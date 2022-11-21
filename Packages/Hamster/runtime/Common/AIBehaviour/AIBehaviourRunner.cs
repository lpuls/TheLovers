using System.Collections.Generic;
using UnityEngine;

namespace Hamster {

    public enum EBehavourExecuteResult {
        Continue,
        Wait,
        Stop,
        Error,
        Done
    }

    public interface IAIBehaviour {
        void Run();
        void Stop();
        void Reset();

        GameObject GetOwner();
        Blackboard GetBlackboard();
    }

    [SerializeField]
    public class BaseBehaviour : ScriptableObject {

        public IAIBehaviour AIBehaviour {
            get;
            protected set;
        }

        public virtual void Initialize(IAIBehaviour behaviour) {
        }

        public virtual EBehavourExecuteResult Execute(IAIBehaviour behaviour, float dt) {
            return EBehavourExecuteResult.Done;
        }
        public virtual void ResetBehaviour(IAIBehaviour behaviour) {
        }

    }

    public class AIBehaviourRunner : IAIBehaviour {

        private AIBehaviourScript _root = null;
        private Blackboard _blackboard = new Blackboard();
        private GameObject _owner = null;

        public bool IsRun {
            get;
            private set;
        }

        public void Initialize(AIBehaviourScript root, GameObject gameObject) {
            _root = root;
            _blackboard.Clean();
            _owner = gameObject;

            _root.Executor.Initialize(this);
        }

        public void Run() {
            IsRun = true;
        }

        public void Execute( float dt) {
            if (IsRun)
                _root.Executor.Execute(this, dt);
        }

        public void Reset() {
            _root.Executor.ResetBehaviour(this);
        }

        public void Stop() {
            IsRun = false;
        }

        public GameObject GetOwner() {
            return _owner;
        }

        public Blackboard GetBlackboard() {
            return _blackboard;
        }
    }
}
