using System;
using System.Collections.Generic;

namespace Hamster {
    [Serializable]
    public class PlatformConfig {
        public bool UseAssetBundle = true;
        public string Platform = string.Empty;
        public string AssetBundlePath = string.Empty;
        public string Manifast = string.Empty;
        public string BuildAssetBundlePath = string.Empty;
        public string BuildPlayerPath = string.Empty;
        public bool IsRelease = false;
    }

    [Serializable]
    public class GameConfig {
        public string GameName = "Game Create By Hamster In Donut't Hole";
        public List<string> BuildScenes = new();
        public List<PlatformConfig> Platforms = new();

        public bool FindPlatformConfig(string platform, out PlatformConfig config) {
            config = null;
            foreach (var item in Platforms) {
                if (item.Platform.Equals(platform)) {
                    config = item;
                    return true;
                }
            }
            return false;
        }
    }
}
