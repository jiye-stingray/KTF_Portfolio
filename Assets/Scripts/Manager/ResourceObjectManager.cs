using Cysharp.Threading.Tasks;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResourceObjectManager : MonoBehaviour
{
    private Dictionary<string, Object> _dicResource = new Dictionary<string, Object>();

    public GameObject Instantiate(string key, Transform parent = null)
    {
        GameObject tPrefab = Load<GameObject>($"{key}");
        if (tPrefab == null)
        {
            Debug.LogError($"Error Load {key}");
            return null;
        }
        
        GameObject tGo = Instantiate(tPrefab, parent);
        tGo.name = tPrefab.name;
        
        return tGo;
    }
    
    public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
    {
        GameObject tPrefab = await AddressableLoader.LoadCachedAssetAsync<GameObject>(key);
        if (tPrefab == null)
        {
            Debug.LogError($"Error Load {key}");
            return null;
        }
        
        GameObject tGo = Instantiate(tPrefab, parent);
        tGo.name = tPrefab.name;
        
#if UNITY_EDITOR
        foreach (Renderer smr in tGo.GetComponentsInChildren<Renderer>(true))
        {
            if (smr.sharedMaterial == null)
                continue;

            smr.sharedMaterial.shader = Shader.Find(smr.sharedMaterial.shader.name);
        }
#endif
        
        return tGo;
    }
    
    public async UniTask<T> LoadAsync<T>(string key)
    {
        T asset = await AddressableLoader.LoadCachedAssetAsync<T>(key);
        if (asset == null)
        {
            Debug.LogError($"Error Load {key}");
            return default;
        }
        
        return asset;
    }
    
    
    public T Load<T>(string key) where T : Object
    {
        if (_dicResource.TryGetValue(key, out Object cached))
        {
            return cached as T;
        }

        Object result = null;
        Type t = typeof(T);
        if (t == typeof(Sprite))
        {
            result = Resources.Load<Sprite>(key);
        }
        else if (t == typeof(AudioClip))
        {
            result = Resources.Load<AudioClip>(key);
        }
        else if (t == typeof(Material))
        {
            result = Resources.Load<Material>(key);
        }
        else if (t == typeof(SkeletonDataAsset))
        {
            result = Resources.Load<SkeletonDataAsset>(key);
        }
        else if (t == typeof(GameObject))
        {
            result = Resources.Load<GameObject>(key);
        }
        else
        {
            GameObject go = Resources.Load<GameObject>(key);
            if (go != null)
            {
                result = go.GetComponent<T>();
            }
        }

        if (result != null)
        {
            _dicResource[key] = result;
            return result as T;
        }

        MyLogger.LogWarning($"Resource not found for key: {key} and type: {typeof(T)}");
        return null;
    }
    
    public void Destroy(GameObject go)
    {
        if(go == null) return;
        
        Object.Destroy(go);
    }
}
