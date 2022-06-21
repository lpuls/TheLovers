using Hamster;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasManager {
    private Dictionary<string, Dictionary<string, Sprite>> _sprites = new Dictionary<string, Dictionary<string, Sprite>>();
    private Sprite[] _spriteArray = new Sprite[256];

    public void Init() {
        SpriteAtlasManager.atlasRequested += SpriteAtlasManagerAtlasRequested;
    }

    public void Finish() {
        SpriteAtlasManager.atlasRequested -= SpriteAtlasManagerAtlasRequested;
    }

    private void SpriteAtlasManagerAtlasRequested(string atlasName, System.Action<SpriteAtlas> callback) {
        string atlasPath = "Res/SpriteAtlas/" + atlasName;
        Asset.LoadSync(atlasPath, (Object asset) => {
            SpriteAtlas spriteAtlas = asset as SpriteAtlas;
            callback?.Invoke(spriteAtlas);
        });
    }

    public SpriteAtlas LoadAtlas(string path) {
        if (_sprites.TryGetValue(path, out Dictionary<string, Sprite> sprites)) {
            return null;
        }

        SpriteAtlas spriteAtlas = Asset.Load<SpriteAtlas>(path);
        if (null == spriteAtlas) {
            Debug.LogError("Can't Get Sprite Atlas " + path);
            return null;
        }

        sprites = new Dictionary<string, Sprite>();
        _sprites[path] = sprites;

        int count = spriteAtlas.GetSprites(_spriteArray);
        if (count < spriteAtlas.spriteCount) {
            Debug.LogError("Can't Get All Sprites " + path); 
        }
        for (int i = 0; i < count; i++) {
            Sprite sprite = _spriteArray[i];
            sprites.Add(sprite.name.Replace("(Clone)", ""), sprite);
        }

        return spriteAtlas;
    }

    public Sprite GetSprite(string atlasPath, string spritepath) {
        if (!_sprites.TryGetValue(atlasPath, out Dictionary<string, Sprite> sprites)) {
            LoadAtlas(atlasPath);
        }
        if (!_sprites.TryGetValue(atlasPath, out sprites)) {
            Debug.LogError("Can't Get Sprite Atlas " + atlasPath);
            return null;
        }

        if (!sprites.TryGetValue(spritepath, out Sprite sprite)) {
            Debug.LogError("Can't Get Sprite " + atlasPath + ", " + spritepath);
        }
        return sprite;
    }

    public void UnloadAtlas(string path) {
        if (_sprites.TryGetValue(path, out Dictionary<string, Sprite> sprites)) {
            _sprites.Remove(path);
        }
        Asset.Unload(path);
    }
}
