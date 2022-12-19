using System;
using System.Collections.Generic;

namespace Hamster {
    [Serializable]
    public class PlatformConfig {
        public string Platform = string.Empty;
        public string AssetBundlePath = string.Empty;
        public string Manifast = string.Empty;
        public string BuildAssetBundlePath = string.Empty;
        public string BuildPath = string.Empty;
    }

    [Serializable]
    public class GameConfig {
        public bool UseAssetBundle = false;
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
