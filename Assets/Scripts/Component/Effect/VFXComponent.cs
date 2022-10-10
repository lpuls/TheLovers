using UnityEngine;

namespace Hamster.SpaceWar {
    public class VFXComponent : MonoBehaviour {

        private float _time = 0;
        [SerializeField] private float _lifeTime = 0;

#if UNITY_EDITOR
        public void SetLifeTime(float value) {
            _lifeTime = value;
        }
        public float GetLifeTime() {
            return _lifeTime;
        }
#endif

        public void Update() {
            _time += Time.deltaTime;
            if (_time >= _lifeTime) {
                AssetPool.Free(gameObject);
            }
        }

        public void OnEnable() {
            _time = 0;
        }

        public static float GetMaxVlaue(AnimationCurve curve) {
            var ret = float.MinValue;
            var frames = curve.keys;
            for (int i = 0; i < frames.Length; i++) {
                var frame = frames[i];
                var value = frame.value;
                if (value > ret)
                    ret = value;
            }
            return ret;
        }

        public static float GetMinVlaue(AnimationCurve curve) {
            var ret = float.MaxValue;
            var frames = curve.keys;
            for (int i = 0; i < frames.Length; i++) {
                var frame = frames[i];
                var value = frame.value;
                if (value < ret)
                    ret = value;
            }
            return ret;
        }

        public static float GetMaxValue(ParticleSystem.MinMaxCurve minMaxCurve) {
            switch (minMaxCurve.mode) {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.Curve:
                    return GetMaxVlaue(minMaxCurve.curve);
                case ParticleSystemCurveMode.TwoCurves:
                    return Mathf.Max(GetMaxVlaue(minMaxCurve.curveMin), GetMaxVlaue(minMaxCurve.curveMax));
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMax;
            }
            return -1;
        }

        public static float GetMinValue(ParticleSystem.MinMaxCurve minMaxCurve) {
            switch (minMaxCurve.mode) {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.Curve:
                    return GetMinVlaue(minMaxCurve.curve);
                case ParticleSystemCurveMode.TwoCurves:
                    return Mathf.Min(GetMinVlaue(minMaxCurve.curveMin), GetMinVlaue(minMaxCurve.curveMax));
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMin;
            }
            return -1;
        }

        public static float GetDuration(ParticleSystem particleSystem, bool allowLoop = false) {
            if (!particleSystem.emission.enabled)
                return 0.0f;
            if (particleSystem.main.loop && !allowLoop)
                return -1.0f;
            if (GetMinValue(particleSystem.emission.rateOverTime) <= 0)
                return GetMinValue(particleSystem.main.startDelay) + GetMinValue(particleSystem.main.startLifetime);
            else
                return GetMaxValue(particleSystem.main.startDelay) + Mathf.Max(GetMaxValue(particleSystem.main.startLifetime), particleSystem.main.duration);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(VFXComponent))]
    public class VFXComponentInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            VFXComponent component = (VFXComponent)target;

            GUILayout.Label("Duration: " + component.GetLifeTime());
            if (GUILayout.Button("Cal Duration")) {
                float lifeTime = float.MinValue;
                ParticleSystem[] particleSystems = component.gameObject.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < particleSystems.Length; i++) {
                    ParticleSystem particleSystem = particleSystems[i];
                    float duration = VFXComponent.GetDuration(particleSystem);
                    if (duration > lifeTime)
                        lifeTime = duration;
                }
                component.SetLifeTime(lifeTime);

                UnityEditor.EditorUtility.SetDirty(component.gameObject);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }

    }
#endif
}
