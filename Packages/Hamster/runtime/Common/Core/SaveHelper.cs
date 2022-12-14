using System;
using System.IO;

namespace Hamster {
    public static class BinaryHelper {
        public static void Write(this BinaryWriter binaryWrite, UnityEngine.Vector2 value) {
            binaryWrite.Write(value.x);
            binaryWrite.Write(value.y);
        }

        public static void Write(this BinaryWriter binaryWrite, UnityEngine.Vector3 value) {
            binaryWrite.Write(value.x);
            binaryWrite.Write(value.y);
            binaryWrite.Write(value.z);
        }

        public static UnityEngine.Vector2 ReadVector2(this BinaryReader binaryWrite) {
            return new UnityEngine.Vector2(binaryWrite.ReadSingle(), binaryWrite.ReadSingle());
        }

        public static UnityEngine.Vector3 ReadVector3(this BinaryReader binaryWrite) {
            return new UnityEngine.Vector3(binaryWrite.ReadSingle(), binaryWrite.ReadSingle(), binaryWrite.ReadSingle());
        }

    }

    public class SaveHelper {
        private int _version = 0;
        private string _savePath = string.Empty;
        private bool _isDirty = false;

        public int SaveCount = 0;

        private Action _onLoadComplete;
        private Action _onSaveComplete;

        public SaveHelper(string path, int version) {
            _savePath = path;
            _version = version;
        }

        protected virtual void SaveData(BinaryWriter binaryWriter) {
            
        }

        protected virtual void LoadData(BinaryReader binaryReader) {
        }

        public void SetDirty() {
            _isDirty = true;
        }


        public void Save() {
            if (!_isDirty)
                return;
            _isDirty = false;

            BinaryWriter binaryWriter = new BinaryWriter(new FileStream(_savePath, FileMode.OpenOrCreate));

            // 写入版本号和数据
            binaryWriter.Write(_version);
            SaveData(binaryWriter);

            binaryWriter.Close();

            _onSaveComplete?.Invoke();
        }

        public void Delete() {
            if (HasSaveData()) {
                File.Delete(_savePath);
            }
        }

        public void Load() {
            if (!HasSaveData())
                return;

            BinaryReader binaryReader = new BinaryReader(new FileStream(_savePath, FileMode.Open));

            int version = binaryReader.ReadInt32();
            if (version != _version)
                throw new System.Exception("The version of save data is not match!!");
            LoadData(binaryReader);

            binaryReader.Close();

            _onLoadComplete?.Invoke();
        }

        public bool HasSaveData() {
            return File.Exists(_savePath);
        }

        public void BindLoadCompleteCallback(Action action) {
            _onLoadComplete += action;
        }

        public void BindSaveCompleteCallback(Action action) {
            _onSaveComplete += action;
        }

        public void UnBindLoadCompleteCallback(Action action) {
            _onLoadComplete -= action;
        }

        public void UnBindSaveCompleteCallback(Action action) {
            _onSaveComplete -= action;
        }
    }
}
