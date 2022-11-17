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

        private BaseBehaviour _root = null;
        private Blackboard _blackboard = new Blackboard();
        public bool IsRun {
            get;
            private set;
        }

        public void Initialize(BaseBehaviour root, GameObject gameObject) {
            _root = root;
            _blackboard.Clean();
            _root.Initialize(gameObject, this, _blackboard);
        }

        public void Run() {
            IsRun = true;
        }

        public void Execute(float dt) {
            if (IsRun)
                _root.Execute(dt);
        }

        public void Reset() {
            _root.ResetBehaviour();
        }

        public void Stop() {
            IsRun = false;
        }

    }
}
