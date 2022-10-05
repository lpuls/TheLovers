﻿using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public class NetPlayerCommand : IPool {

        public int FrameIndex = 0;
        public Vector3 Location = Vector3.zero;
        public int Operate = 0;

        public void Reset() {
            FrameIndex = 0;
            Location = Vector3.zero;
            Operate = 0;
        }
    }

    public class SimulateComponent : MonoBehaviour {

        public float MaxDistanceWithServer = 1.0f;

        // 模拟相关
        protected float _simulateTime = 0;
        public Vector3 PreLocation {
            get;
            private set;
        }
        public Vector3 CurrentLocation {
            get;
            private set;
        }

        private Vector3 _serverPreLocation = Vector3.zero;
        private Vector3 _serverCurrentLocation = Vector3.zero;

        // 預測相关
        private int _predictionIndex = 0;
        private List<NetPlayerCommand> _predicationCommands = new List<NetPlayerCommand>(32);

        private NetSyncComponent _netSyncComponent = null;
        private MovementComponent _movementComponent = null;


        private void Awake() {
            _netSyncComponent = GetComponent<NetSyncComponent>();
            _movementComponent = GetComponent<MovementComponent>();

            PreLocation = transform.position;
            CurrentLocation = transform.position;

            _serverPreLocation = transform.position;
            _serverCurrentLocation = transform.position;
        }

        public void Update() {
            // 主端需要对預測结果进么比对并根据逻辑结果模拟后面的操作
            if (_netSyncComponent.IsAutonomousProxy()) {
                if (_predictionIndex > 0 && TryGetTopPredictionCommand(out NetPlayerCommand command)) {
                    // 如果逻辑已经超过預測帧了，说明前面的帧都已经没用了，直接移除
                    if (command.FrameIndex < _predictionIndex) {
                        CleanPredicationLocations(_predictionIndex);
                        TryGetTopPredictionCommand(out command);
                    }

                    // 检查預測值是否与逻辑值相同
                    if (null != command && command.FrameIndex == _predictionIndex) {
                       


                        // 逻辑值与預測值不一致时，以逻辑值为准，并重新模拟操作
                        if (command.Location != _serverCurrentLocation) {
                            Debug.Log(string.Format("=====>{3} Predict: {0}, Server: {1}, FrameIndex: {2}-{3}",
                                command.Location, _serverCurrentLocation, command.FrameIndex, _predictionIndex));

                            PreLocation = _serverPreLocation;
                            CurrentLocation = _serverCurrentLocation;
                            _simulateTime = BaseFrameDataManager.LOGIC_FRAME_TIME;

                            // 移除最顶上的操作并重新模拟
                            RemoveTopPredictionCommand();
                            SimulateAfter();
                        }
                        else {
                            RemoveTopPredictionCommand();
                        }
                    }
                }
            }

            _simulateTime += Time.deltaTime;
            if (!CurrentLocation.Equals(Vector3.zero)) {
                transform.position = Vector3.Lerp(PreLocation, CurrentLocation, _simulateTime / BaseFrameDataManager.LOGIC_FRAME_TIME);
            }
        }

        public void UpdateSimulateInfo(Vector3 preLocation, Vector3 currentLocation, int predictionIndex) {
            if (predictionIndex >= 0) {
                _predictionIndex = predictionIndex;
                _serverPreLocation = preLocation;
                _serverCurrentLocation = currentLocation;
            }
            else {
                PreLocation = preLocation;
                CurrentLocation = currentLocation;
                _simulateTime = 0;
            }

        }

        public void AddPredictionCommand(int index, Vector3 location, int input) {
            NetPlayerCommand command = ObjectPool<NetPlayerCommand>.Malloc();
            command.FrameIndex = index;
            command.Location = location;
            command.Operate = input;
            _predicationCommands.Add(command);
        }

        public bool TryGetTopPredictionCommand(out NetPlayerCommand command) {
            command = null;
            if (_predicationCommands.Count > 0) {
                command = _predicationCommands[0];
                return true;
            }
            return false;
        }

        public void RemoveTopPredictionCommand() {
            if (_predicationCommands.Count > 0) {
                NetPlayerCommand command = _predicationCommands[0];
                _predicationCommands.RemoveAt(0);
                ObjectPool<NetPlayerCommand>.Free(command);
            }
        }

        public void CleanPredicationLocations(int index) {
            for (int i = _predicationCommands.Count - 1; i >= 0; i--) {
                NetPlayerCommand command = _predicationCommands[0];
                if (command.FrameIndex < index) {
                    _predicationCommands.RemoveAt(0);
                    ObjectPool<NetPlayerCommand>.Free(command);
                }
            }
        }

        public void SimulateAfter() {
            Debug.Log(string.Format("=====>SimulateAfter {0}", _predicationCommands.Count));
            Vector3 lastLocation = CurrentLocation;
            foreach (var item in _predicationCommands) {
                NetPlayerCommand command = item;
                GameLogicUtility.GetOperateFromInput(transform, command.Operate, out Vector3 moveDirection, out bool _);
                command.Location = GetSimulateComponent().MoveTick(lastLocation, BaseFrameDataManager.LOGIC_FRAME_TIME, command.FrameIndex);
                lastLocation = command.Location;
            }
            CurrentLocation = lastLocation;
        }

        private MovementComponent GetSimulateComponent() {
            if (null == _movementComponent)
                _movementComponent = GetComponent<MovementComponent>();
            return _movementComponent;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(PreLocation, 1.0f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(CurrentLocation, 1.0f);

            UnityEditor.Handles.Label(_serverPreLocation, "Logic Pre " + _serverPreLocation);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_serverPreLocation, 1.0f);
            UnityEditor.Handles.Label(_serverPreLocation, "Logic Current " + CurrentLocation);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(CurrentLocation, 1.0f);

            Gizmos.color = Color.blue;
            foreach (var item in _predicationCommands) {
                NetPlayerCommand command = item;
                Gizmos.DrawWireSphere(command.Location, .5f);
            }
        }
#endif

    }
}