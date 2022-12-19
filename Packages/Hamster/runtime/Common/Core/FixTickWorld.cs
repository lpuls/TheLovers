using System.Collections.Generic;
using UnityEngine;

namespace Hamster {

    public interface IServerTicker {
        int GetPriority();
        void Tick(float dt);
        bool IsEnable();
    }

    public enum EServerTickLayers {
        PreTick = 0,
        Tick = 1,
        LateTick = 2,
        Max
    }

    public class FixTickWorld : World {
        protected List<IServerTicker> _pendingAddTickers = new List<IServerTicker>(128);
        protected HashSet<IServerTicker>[] _tickers = new HashSet<IServerTicker>[(int)EServerTickLayers.Max];

        public float LogicTime {
            get;
            protected set;
        }

        public float LogicFrameTime {
            get;
            protected set;
        }

        public int FrameIndex {
            get;
            protected set;
        }

        public FixTickWorld() {
            LogicTime = 0;
            LogicFrameTime = 1 / 15.0f;
            FrameIndex = 0;

            for (int i = 0; i < _tickers.Length; i++) {
                _tickers[i] = new HashSet<IServerTicker>();
            }
        }

        public void Tick() {
            LogicTime += Time.deltaTime;
            while (LogicTime >= LogicFrameTime) {
                FrameIndex++;
                UpdateTickers();
                FixTick();
                LogicTime -= LogicFrameTime;
            }
        }

        protected virtual void FixTick() {
        }

        protected virtual void UpdateTickers() {
            var pendingIt = _pendingAddTickers.GetEnumerator();
            while (pendingIt.MoveNext()) {
                int index = pendingIt.Current.GetPriority();
                Debug.Assert(index >= 0 && index < (int)EServerTickLayers.Max, "Server tick priority out of range");
                _tickers[index].Add(pendingIt.Current);
            }
            _pendingAddTickers.Clear();

            for (int i = 0; i < _tickers.Length; i++) {
                var it = _tickers[i].GetEnumerator();
                while (it.MoveNext()) {
                    IServerTicker ticker = it.Current;
                    if (ticker.IsEnable())
                        ticker.Tick(LogicFrameTime);
                }
            }

        }

        public void AddTicker(IServerTicker serverTicker) {
            _pendingAddTickers.Add(serverTicker);
        }

        public void RemoveTicker(IServerTicker serverTicker) {
            int index = serverTicker.GetPriority();
            Debug.Assert(index >= 0 && index < (int)EServerTickLayers.Max, "Server tick priority out of range");
            _tickers[index].Remove(serverTicker);
        }
    }
}
