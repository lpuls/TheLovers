using Hamster;
using System;
using System.Collections.Generic;
using UnityEngine;


public class MapProcessor {
    protected int _currentMapID = 0;
    protected Dictionary<int, Portal> _portals = new Dictionary<int, Portal>(new Int32Comparer());
    protected Action _onInitSceneCallBack = null;

    public int GetCurrentMapID() {
        return _currentMapID;
    }

    protected void SetCurrentMapID(int mapID) {
        _currentMapID = mapID;
    }

    public bool TryGetPortal(int portalID, out Portal portal) {
        return _portals.TryGetValue(portalID, out portal);
    }

    protected void SetInitCompleteCallback(Action callback) {
        _onInitSceneCallBack = callback;
    }

    public bool TryGetMapData(int mapID, out string mapPath, out string sceneName) {
        mapPath = "";
        sceneName = "";
        return false;
    }

    public void UnloadCurrentScene() {
        throw new NotImplementedException();
    }

    public void InitMap(Map map) {
        throw new NotImplementedException();
    }

    public void TeleportTo(GameObject teleportTarget, int targetMapID, int targetPortalID) {
        // 目标ID与当前ID一致，不用加载地图
        if (targetMapID == GetCurrentMapID()) {
            // todo 显示转场UI
            if (TryGetPortal(targetPortalID, out Portal portal)) {
                teleportTarget.transform.position = portal.GetTargetLocation();
            }
        }
        else {  // 不一致，加载新地图
            SetInitCompleteCallback(()=> {
                if (TryGetPortal(targetPortalID, out Portal portal)) {
                    teleportTarget.transform.position = portal.GetTargetLocation();
                }
                SetInitCompleteCallback(null);
            });
            LoadMap(targetMapID);
        }
    }

    public AsyncOperation LoadMap(int mapID) {
        UnloadCurrentScene();
        if (!TryGetMapData(mapID, out string path, out string sceneName))
            return null;
        // mapManagerComponent.PreMapID = mapManagerComponent.MapID;
        // mapManagerComponent.MapID = mapInfo.ID;
        SetCurrentMapID(mapID);
        AsyncOperation asyncOperation = Asset.LoadScene(path, sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        asyncOperation.completed += (AsyncOperation obj)=>{
            Map map = GameObject.FindObjectOfType<Map>();
            if (null == map) {
                Debug.LogError("Can't Find Map");
                return;
            }
            InitMap(map);
        };
        return asyncOperation;
    }

    [GM("LoadMap")]
    public static void GMLoadMap(string[] args) {
        if (int.TryParse(args[1], out int id)) {
            // LoadMap(Hamster.ECS.ECSContext.ActiveContext, id);
        }
        else {
            Debug.LogError("Can't Load Map " + args[1]); 
        }
    }

}
