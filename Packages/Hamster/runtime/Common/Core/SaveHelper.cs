using System;
using System.Collections.Generic;
using System.IO;

namespace Hamster {

    public interface ISaver {
        int GetSaveID();
        void Save(BinaryWriter binaryWriter);
        void Load(BinaryReader binaryReader);
        void Reset();
    }

    public class SaveHelper {
        private int _version = 0;
        private string _savePath = string.Empty;
        private Dictionary<int, ISaver> _savers = new Dictionary<int, ISaver>(new Int32Comparer());
        private Action _onLoadComplete;
        private Action _onSaveComplete;

        public SaveHelper(string path, int version, params ISaver[] savers) {
            _savePath = path;
            _version = version;
            for (int i = 0; i < savers.Length; i++) {
                _savers.Add(savers[i].GetSaveID(), savers[i]);
            }
        }

        public void Save() {
            BinaryWriter binaryWriter = new BinaryWriter(new FileStream(_savePath, FileMode.OpenOrCreate));

            // 写入版本号和数据数量
            binaryWriter.Write(_version);
            binaryWriter.Write(_savers.Count);

            // 写入数据
            var it = _savers.GetEnumerator();
            while (it.MoveNext()) {
                ISaver saver = it.Current.Value;
                binaryWriter.Write(saver.GetSaveID());
                saver.Save(binaryWriter);
            }

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

            int count = binaryReader.ReadInt32();
            for (int i = 0; i < count; i++) {
                int saverID = binaryReader.ReadInt32();
                if (!_savers.TryGetValue(saverID, out ISaver saver))
                    throw new System.Exception("Invalid Saver ID " + saverID);
                saver.Load(binaryReader);
            }

            binaryReader.Close();

            _onLoadComplete?.Invoke();
        }

        public void Reset() {
            var it = _savers.GetEnumerator();
            while (it.MoveNext()) {
                it.Current.Value.Reset();
            }
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
