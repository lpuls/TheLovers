using Hamster;
using UnityEngine;
using UnityEngine.UI;

public class HPTip : MonoBehaviour {

    public Text Tiptext = null;
    public Color BeginColor = Color.white;
    public Color EndColor = Color.white;
    public float ShowTime = 1.0f;
    public AnimationCurve MoveCurveX = null;
    public AnimationCurve MoveCurveY = null;

    private bool _update = false;
    private float _currentTime = 0;
    private Vector3 _originPostion = Vector3.zero;

    public void Show(string text) {
        _update = true;
        _currentTime = 0;
        _originPostion = transform.position;
        Tiptext.text = text;
    }

    private void Update() {
        if (!_update)
            return;

        float t = _currentTime / ShowTime;
        if (t > 1.0f) {
            _update = false;
            AssetPool.Free(gameObject); 
        }
        Tiptext.color = Color.Lerp(BeginColor, EndColor, t);
        transform.position = _originPostion + new Vector3(MoveCurveX.Evaluate(t), MoveCurveY.Evaluate(t));
        _currentTime += Time.deltaTime;
    }


}
