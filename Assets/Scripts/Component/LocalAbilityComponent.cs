using System.Collections.Generic;
using UnityEngine;

namespace Hamster.SpaceWar {
    public enum EAbilityIndex {
        MainWeapon = 0,
        SecondaryWeapon = 1,
        UltimateWeapon = 1
    }

    [ExecuteInEditMode]
    public class LocalAbilityComponent : MonoBehaviour {
        public const int MAX_WEAPON_ID = 300;

        private Dictionary<int, List<WeaponComponent>> _weapons = new Dictionary<int, List<WeaponComponent>>(new Int32Comparer());
        private Dictionary<int, int> _weaponEquipIDs = new Dictionary<int, int>(new Int32Comparer());

        public void Awake() {
            WeaponComponent[] weaponComponents = GetComponentsInChildren<WeaponComponent>();
            foreach (var item in weaponComponents) {
                int type = (int)item.Type;
                if (!_weapons.TryGetValue(type, out List<WeaponComponent> weapons)) {
                    weapons = new List<WeaponComponent>();
                    _weapons[type] = weapons;
                }
                item.Parent = gameObject;
                weapons.Add(item);
            }
        }

        public void ChangeWeapon(EAbilityIndex abilityIndex, int id) {
            // 判断是否升级
            int realID = id;
            if (_weaponEquipIDs.TryGetValue((int)abilityIndex, out int equipID)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Weapon>(equipID, out Config.Weapon equipWeaponInfo)) {
                    if (id == equipWeaponInfo.TypeID)
                        realID = equipWeaponInfo.NextLv;
                    // NetxtID 为0时，说明没有下一个等级了
                    if (0 == realID)
                        realID = equipID;
                }
            }
            _weaponEquipIDs[(int)abilityIndex] = realID;

            // 更换武器
            if (_weapons.TryGetValue((int)abilityIndex, out List<WeaponComponent> weapons)) {
                if (Single<ConfigHelper>.GetInstance().TryGetConfig<Config.Weapon>(realID, out Config.Weapon info)) {
                    // 通知UI变更
                    GameLogicUtility.SetMainWeaponIDDirty(gameObject);

                    // 更换生成器
                    BulletSpawner bulletSpawner = Asset.Load<BulletSpawner>(info.Path);
                    foreach (var item in weapons) {
                        item.Spawner = bulletSpawner;
                    }
                }
            }
        }

        public void Cast(EAbilityIndex abilityIndex, float cdGain) {
            if (_weapons.TryGetValue((int)abilityIndex, out List<WeaponComponent> weapons)) {
                foreach (var item in weapons) {
                    item.Spawn(cdGain);
                }
            }
        }

        public bool TryGetWeaponID(int index, out int id) {
            return _weaponEquipIDs.TryGetValue(index, out id);
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
