using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

namespace Hamster.SpaceWar {

    public class LevelItem : UIViewScrollItem {
        public Text LevelName = null;

        public override void Init(UIViewScrollData data) {
            base.Init(data);
            LevelName = GetComponentInChildren<Text>();
            LevelName.text = (data as LevelItemData).LevelName;
        }
    }

    public class LevelItemData : UIViewScrollData {
        public string LevelName = string.Empty;
    }

    public class LevelSelectUI : UIView {

        private UIViewScroll _levelItemScroll = null;

        public override void Initialize() {
            base.Initialize();

            _levelItemScroll = GetMonoComponentFromChild<UIViewScroll>("LevelBoxs");
            _levelItemScroll.Init("Res/UI/LevelSelect/LevelItem");
        }

        public void AddItem(int id, string levelName) {
            LevelItemData levelItemData = ObjectPool<LevelItemData>.Malloc();
            levelItemData.ID = id;
            levelItemData.LevelName = levelName;
            _levelItemScroll.AddItem<LevelItem>(levelItemData);
            ObjectPool<LevelItemData>.Free(levelItemData);
        }

        public void CleanItems() {
            _levelItemScroll.Clear();
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
                _view.AddItem(item.Key, (item.Value as Config.Mission).Title);
            }
        }

        protected override void OnFinish() {
            base.OnFinish();
        }

    }
}
