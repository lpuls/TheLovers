using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar
{
    public class LocalPlayerController : MonoBehaviour
    {
        public InputKeyMapValue InputKeyToValue = null;

        private FrameDataManager _frameDataManager = null;
        private GameLogicSyncModule _gameLogicSyncModule = null;

        public void Awake()
        {
            if (null == InputKeyToValue)
            {
                Debug.LogError("=====>Local LocalPlayerController Input Key To Value ");

                InputKeyToValue = Asset.Load<InputKeyMapValue>("Res/ScriptObject/LocalInputKeyMapValue");
                if (null == InputKeyToValue) {
                    Debug.LogError("=====>Local LocalPlayerController Input Key To Value is null");
                    return;
                }
            }

            NetDevice netDeivce = World.GetWorld().GetManager<NetDevice>();
            if (null == netDeivce || !netDeivce.IsValid) {
                Debug.LogError("=====>Local LocalPlayerController Has not NetDevice ");
                return;
            }

            _gameLogicSyncModule = netDeivce.GetModule(GameLogicSyncModule.NET_GAME_LOGIC_SYNC_ID) as GameLogicSyncModule;
            if (null == _gameLogicSyncModule)
            {
                Debug.LogError("=====>Local LocalPlayerController Has not GameLogicSyncModule ");
                return;
            }

            _frameDataManager = World.GetWorld().GetManager<FrameDataManager>();
        }

        public void Update()
        {
            int operat = 0;
            for (int i = 0; i < InputKeyToValue.InputKeys.Count; i++)
            {
                KeyCode keyCode = InputKeyToValue.InputKeys[i];
                if (Input.GetKey(keyCode)) {
                    operat |= (int)InputKeyToValue.InputValues[i];
                }
            }

            // 有操作的情况发送操作
            if (0 != operat && null != _gameLogicSyncModule) {
                _gameLogicSyncModule.SendOperator(operat);
            }
            
        }
    }
}
