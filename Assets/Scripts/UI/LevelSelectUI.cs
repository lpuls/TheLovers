using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

namespace Hamster.SpaceWar {

    public class LevelItem : MonoBehaviour {
        public int ID = 0;
        public Image Icon = null;
        public Text Title = null;
        public Text Context = null;
        public Button ItemButton = null;
        public Button SinglePlayButton = null;
        public Button CreateRoomButton = null;
        public InputField IpInputField = null;
        public InputField PortInputField = null;
        public Button JoinRoomButton = null;
        public GameObject Info = null;

        public ILevelSelectUI LevelSelectUI = null;

        public void Init(UIViewScrollData data) {
            ID = data.ID;
            LevelItemData levelItemData = data as LevelItemData;
            GetComponent<RectTransform>().anchoredPosition = new Vector3(levelItemData.X, levelItemData.Y);

            if (null == ItemButton)
                ItemButton = gameObject.GetComponentFromeChild<Button>("Icon");
            ItemButton.onClick.AddListener(OnClickIcon);

            if (null == Icon)
                Icon = gameObject.GetComponentFromeChild<Image>("Icon");
            Icon.sprite = Single<AtlasManager>.GetInstance().GetSprite("Res/SpriteAtlas/LevelSelectIcons", levelItemData.Icon);

            if (null == Info)
                Info = transform.Find("Info").gameObject;
            Info.SetActive(false);

            if (null == Title)
                Title = gameObject.GetComponentFromeChild<Text>("Info/Title");
            Title.text = (data as LevelItemData).Title;

            if (null == Context)
                Context = gameObject.GetComponentFromeChild<Text>("Info/Context");
            Context.text = (data as LevelItemData).Context;

            if (null == IpInputField)
                IpInputField = gameObject.GetComponentFromeChild<InputField>("Info/Ip");
            if (null == PortInputField)
                PortInputField = gameObject.GetComponentFromeChild<InputField>("Info/Port");
            if (null == SinglePlayButton) {
                SinglePlayButton = gameObject.GetComponentFromeChild<Button>("Info/SinglePlay");
                SinglePlayButton.onClick.AddListener(OnClickSinglePlay);
            }
            if (null == CreateRoomButton) {
                CreateRoomButton = gameObject.GetComponentFromeChild<Button>("Info/Hoster");
                CreateRoomButton.onClick.AddListener(OnClickCreateRoom);
            }
            if (null == JoinRoomButton) {
                JoinRoomButton = gameObject.GetComponentFromeChild<Button>("Info/Join");
                JoinRoomButton.onClick.AddListener(OnClickJoinRoom);
            }

            if (!CheckLevelValid()) {
                Title.text = "这关其实没有做";
                Context.text = "开发者已经失去耐心了，准备开新坑了";
            }

            // 初始化ip和端口
            GameOutsideWorld world = World.GetWorld<GameOutsideWorld>();
            if (null != world) {
                if (world.TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData)) {
                    IpInputField.text = swapData.IP;
                    PortInputField.text = swapData.Port.ToString();
                }
            }
        }

        private void OnClickIcon() {
            if (null != Info) {
                Info.SetActive(!Info.activeSelf);
                if (null != LevelSelectUI) {
                    if (Info.activeSelf)
                        LevelSelectUI.OnSelectItem(this, false);
                    else
                        LevelSelectUI.OnSelectItem(null, true);
                }
            }
        }

        private void OnClickSinglePlay() {
            if (!CheckLevelValid())
                return;

            GameOutsideWorld world = World.GetWorld<GameOutsideWorld>();
            if (null != world) {
                if (world.TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData)) {
                    swapData.LevelID = ID;
                    world.GoToSinglePlay();
                }
            }
        }

        private void OnClickCreateRoom() {
            if (!CheckLevelValid())
                return;

            GameOutsideWorld world = World.GetWorld<GameOutsideWorld>();
            if (null != world && TryGetIpAndPort(out string ip, out int port)) {
                if (world.TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData)) {
                    swapData.IP = ip;
                    swapData.Port = port;
                    swapData.LevelID = ID;
                    world.GoToMultipleAsHoster();
                }
            }
        }

        private void OnClickJoinRoom() {
            if (!CheckLevelValid())
                return;

            GameOutsideWorld world = World.GetWorld<GameOutsideWorld>();
            if (null != world && TryGetIpAndPort(out string ip, out int port)) {
                if (world.TryGetWorldSwapData<SpaceWarSwapData>(out SpaceWarSwapData swapData)) {
                    swapData.IP = ip;
                    swapData.Port = port;
                    swapData.LevelID = ID;
                    world.GoToMultipleAsGuess();
                }
            }
        }

        private bool CheckLevelValid() {
            if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Mission>(ID, out Config.Mission mission)) {
                return !string.IsNullOrEmpty(mission.Path);
            }
            return false;
        }

        private bool TryGetIpAndPort(out string ip, out int port) {
            ip = IpInputField.text;
            return int.TryParse(PortInputField.text, out port);
        }
    }

    public class LevelItemData : UIViewScrollData {
        public string Title = string.Empty;
        public string Context = string.Empty;
        public string Icon = string.Empty;
        public string LevelPath = string.Empty;
        public float X = 0;
        public float Y = 0;
    }

    public interface ILevelSelectUI {
        void OnSelectItem(LevelItem item, bool enable);
    }

    public class LevelSelectUI : UIView, ILevelSelectUI {

        private List<LevelItem> _items = new List<LevelItem>();

        public void AddItem(int id, string title, string context, string icon, float x, float y) {
            LevelItemData levelItemData = ObjectPool<LevelItemData>.Malloc();
            levelItemData.ID = id;
            levelItemData.Title = title;
            levelItemData.Context = context;
            levelItemData.Icon = icon;
            levelItemData.X = x / 100.0F;
            levelItemData.Y = y / 100.0F;

            GameObject item = Asset.Load("Res/UI/LevelSelect/LevelItem");
            if (null != item) {
                LevelItem levelItem = item.TryGetOrAdd<LevelItem>();
                if (null != levelItem) {
                    _items.Add(levelItem);
                    levelItem.transform.SetParent(transform, false);
                    levelItem.LevelSelectUI = this;
                    levelItem.Init(levelItemData);
                }
            }

            ObjectPool<LevelItemData>.Free(levelItemData);
        }

        public void CleanItems() {
            foreach (var item in _items) {
                AssetPool.Free(item.gameObject);
            }
            _items.Clear();
        }

        public void OnSelectItem(LevelItem levelItem, bool enable) {
            foreach (var item in _items) {
                if (item != levelItem) {
                    item.gameObject.SetActive(enable);
                }
            }
        }

    }

    public class LevelSelectModule : UIModule {
    }

    [UIInfo("Res/UI/LevelSelect/LevelSelectUI", typeof(LevelSelectUI), typeof(LevelSelectModule))]
    public class LevelSelectController : UIController<LevelSelectUI, LevelSelectModule> {
        protected override void OnInitialize() {
            base.OnInitialize();

            Dictionary<int, Google.Protobuf.IMessage> dictionary = Single<ConfigHelper>.GetInstance().GetConfigs<Config.Mission>();
            foreach (var item in dictionary) {
                Config.Mission mission = item.Value as Config.Mission;
                _view.AddItem(item.Key, mission.Title, mission.Context, mission.Icon, mission.X, mission.Y);
            }
        }

        protected override void OnFinish() {
            base.OnFinish();
        }

    }
}
