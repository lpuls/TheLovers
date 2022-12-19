using System.Collections;
using UnityEngine;

namespace Hamster {
    public class TransitionUI : MonoBehaviour {

        public float HideTime = 1.5f;

        private Animator _animator = null;
        private WaitForSeconds _waitHideTime = null;

        private void Awake() {
            _animator = GetComponent<Animator>();
            _waitHideTime = new WaitForSeconds(HideTime);
        }

        public virtual void ShowTransition() {
            gameObject.SetActive(true);
            _animator.Play("Show");
        }

        public virtual void HideTransition() {
            StartCoroutine(WaitHide());
            _animator.Play("Hide");

        }

        private IEnumerator WaitHide() {
            yield return _waitHideTime;
            gameObject.SetActive(false);
        }
    }
}
