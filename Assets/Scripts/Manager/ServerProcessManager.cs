using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {

    public interface IServerTicker {
        void Tick();

        void Kill();

        bool IsPendingKill();
    }

    public class ServerProcessManager {
        private List<IServerTicker> _tickers = new List<IServerTicker>();

        public void AddTicker(IServerTicker serverTicker) {
            _tickers.Add(serverTicker);
        }

        public void RemoveTicker(IServerTicker serverTicker) {
            serverTicker.Kill();
        }

        public void Update() {
            for (int i = _tickers.Count - 1; i >= 0; i--) {
                IServerTicker serverTicker = _tickers[i];
                if (serverTicker.IsPendingKill()) {
                    _tickers.RemoveAt(i);
                }
                else {
                    serverTicker.Tick();
                }
            }
        }

    }
}
