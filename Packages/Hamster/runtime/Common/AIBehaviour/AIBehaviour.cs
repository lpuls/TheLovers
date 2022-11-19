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
    }

    [SerializeField]
    public class BaseBehaviour : ScriptableObject {
        public GameObject Owner {
            get;
            protected set;
        }

        public Blackboard Blackboard {
            get;
            protected set;
        }

        public IAIBehaviour AIBehaviour {
            get;
            protected set;
        }

        public virtual void Initialize(GameObject gameObject, IAIBehaviour behaviour, Blackboard blackboard) {
            Owner = gameObject;
            Blackboard = blackboard;
            AIBehaviour = behaviour;
        }

        public virtual EBehavourExecuteResult Execute(float dt) {
            return EBehavourExecuteResult.Done;
        }

        public virtual void ResetBehaviour() {
        }

    }

    public class AIBehaviour : IAIBehaviour {

        private AIBehaviourScript _root = null;
        private Blackboard _blackboard = new Blackboard();
        public bool IsRun {
            get;
            private set;
        }

        public void Initialize(AIBehaviourScript root, GameObject gameObject) {
            _root = root;
            _blackboard.Clean();

            _root.Initialize.Initialize(gameObject, this, _blackboard);
            _root.Executor.Initialize(gameObject, this, _blackboard);
            _root.Finish.Initialize(gameObject, this, _blackboard);

            _root.Initialize.Execute(0);
        }

        public void Run() {
            IsRun = true;
        }

        public void Execute(float dt) {
            if (IsRun)
                _root.Executor.Execute(dt);
        }

        public void Reset() {
            _root.Initialize.ResetBehaviour();
            _root.Executor.ResetBehaviour();
            _root.Finish.ResetBehaviour();
        }

        public void Stop() {
            IsRun = false;
            _root.Finish.Execute(0);
        }

    }
}
