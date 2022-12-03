namespace Hamster.SpaceWar {
    public class SpaceWarSwapData : WorldSwapData {
        public Config.GameSetting Setting = null;
        public Config.GameModel GameModel = Config.GameModel.None;

        public string IP = "0.0.0.0";
        public int Port = 8888;
        public int LevelID = 0;
    }
}