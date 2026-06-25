using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using static Define;

public class AtlasManager
{
    private List<string> _patchAtlasTypes = new List<string>();
    // Atlas cache
    private Dictionary<EAtlasType, SpriteAtlas> _atlasesCache = new Dictionary<EAtlasType, SpriteAtlas>();
    // sprite cache
    private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

#if ADDRESSABLES_ENABLED
    private Dictionary<EAtlasType, AsyncOperationHandle<SpriteAtlas>> _loadHandles = new();
#endif
    
    #if ADDRESSABLES_ENABLED
    public void InitializeFromBundle(SpriteAtlas atlas)
    {
        if (atlas == null) return;

        if (Enum.TryParse<EAtlasType>(atlas.name, out var atlasType))
            _atlasesCache.TryAdd(atlasType, atlas);
        else
            MyLogger.LogWarning($"[AtlasManager] 알 수 없는 Atlas 이름: {atlas.name}");
    }

    public void InitializeFromBundle(List<SpriteAtlas> loadedAtlases)
    {
        foreach (var atlas in loadedAtlases)
            InitializeFromBundle(atlas);

        MyLogger.Log($"[AtlasManager] 번들로부터 SpriteAtlas {loadedAtlases.Count}개 초기화 완료");
    }

    #endif

    public Sprite GetSprite(EAtlasType atlasType, string spriteName)
    {
        string fullkey = $"{atlasType.ToString()}:{spriteName}";

        // 이미 sprite 캐싱 된 경우
        if(_spriteCache.TryGetValue(fullkey, out Sprite atlasSprite))
        {
            return atlasSprite;
        }

        // atlas 캐싱 확인
        if(!_atlasesCache.TryGetValue(atlasType, out SpriteAtlas atlas))
        {
            atlas = Resources.Load<SpriteAtlas>($"Atlas/{atlasType.ToString()}");     // 추후 addressable로 관리 가능
            if(atlas == null )
            {
                Debug.LogError($"atlas null !! {atlasType.ToString()} ");
                return null;
            }

            _atlasesCache[atlasType] = atlas;
        }

        // sprite 추출 
        Sprite sprite = atlas.GetSprite(spriteName);
        if (sprite == null)
        {
            Debug.LogError($"sprite null !! {atlasType.ToString()} in {spriteName}");
            return null;    
        }

        // setting
        _spriteCache[fullkey] = sprite;
        return sprite;
    }

    public Sprite GetCharacterIcon(EUnitType unitType, string iconName)
    {
        EAtlasType atlasType = unitType == EUnitType.PlayerCharacter ? EAtlasType.CharacterIconAtlas : EAtlasType.MonsterIconAtlas;
        string spriteName = unitType == EUnitType.PlayerCharacter ? $"Thum_Face_{iconName}" : $"Thumbnail_{iconName}";

        return GetSprite(atlasType, spriteName);
    }
    
    public void UnloadAtlas(EAtlasType atlasType)
    {

   #if ADDRESSABLES_ENABLED
        if (_loadHandles.TryGetValue(atlasType, out var handle))
        {
            Addressables.Release(handle);
            _loadHandles.Remove(atlasType);
        }
    #endif
        
        if(_atlasesCache.ContainsKey(atlasType))
        {
            _atlasesCache.Remove(atlasType);
        }

        // sprite 캐시 중 해당 atlas 관련된 것 제거
        List<string> keysToRemove = new List<string>();
        foreach (string key in _spriteCache.Keys)
        {
            if(key.StartsWith($"{atlasType.ToString()}"))
                keysToRemove.Add(key);
        }

        foreach (string key in keysToRemove)
        {
            _spriteCache.Remove(key);
        }

        Resources.UnloadUnusedAssets();     // 메모리 해제
    }

    public void UnloadAll()
    {
    #if ADDRESSABLES_ENABLED
        foreach (var handle in _loadHandles.Values)
            Addressables.Release(handle);

        _loadHandles.Clear();
    #endif
        
        _atlasesCache.Clear();
        _spriteCache.Clear();

        Resources.UnloadUnusedAssets();     // 메모리 해제
    }
}
