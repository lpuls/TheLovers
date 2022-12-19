using UnityEngine;
using UnityEngine.UI;

namespace Hamster {
    public class LoadingUI : MonoBehaviour {

        public Text ProgressText = null;
        public Slider ProgressBar = null;

        protected float _targetProgress = 0;
        protected float _currentProgress = 0;

        public void OnEnable() {
            _targetProgress = 0;
            _currentProgress = 0; 
        }

        public void SetProgress(float value) {
            _targetProgress = Mathf.Clamp01(value);
        }

        protected virtual void Update() {
            _currentProgress = Mathf.MoveTowards(_currentProgress,  _targetProgress, 0.01f);
            if (null != ProgressText) {
                ProgressText.text = ((int)(_currentProgress * 100)).ToString();
            }
            if (null != ProgressBar) {
                ProgressBar.value = _currentProgress;
            }
        }
    }
}