using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum EAbilityIndex {
        Fire = 0,
        Ultimate = 1
    }

    [ExecuteInEditMode]
    public class LocalAbilityComponent : MonoBehaviour {

        private Dictionary<int, List<WeaponComponent>> _weapons = new Dictionary<int, List<WeaponComponent>>(new Int32Comparer());

        public void Awake() {
            WeaponComponent[] weaponComponents = GetComponentsInChildren<WeaponComponent>();
            foreach (var item in weaponComponents) {
                int type = (int)item.Type;
                if (!_weapons.TryGetValue(type, out List<WeaponComponent> weapons)) {
                    weapons = new List<WeaponComponent>();
                    _weapons[type] = weapons;
                }
                weapons.Add(item);
            }
        }

        public void Cast(EAbilityIndex abilityIndex, float cdGain) {
            if (_weapons.TryGetValue((int)abilityIndex, out List<WeaponComponent> weapons)) {
                foreach (var item in weapons) {
                    item.Spawn(cdGain);
                }
            }
        }

        public void Tick(float dt) {
            var it = _weapons.GetEnumerator();
            while (it.MoveNext()) {
                List<WeaponComponent> weapons = it.Current.Value;
                foreach (var item in weapons) {
                    item.Tick(dt);
                }
            }
        }

    }
}
