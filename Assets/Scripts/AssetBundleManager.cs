using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AssetBundleManager : SingletonDontDestroyed<AssetBundleManager>
{
    private readonly Dictionary<AssetReference, AsyncOperationHandle> dicLoadedAssetBundle = new();

    #region async await.
    public async Task<T> LoadAssetBundle_Async<T>(string path) where T : class
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(path);

        return await handle.Task as T;
    }
    public async Task<T> LoadAssetBundle_Async<T>(AssetReference assetReference) where T : class
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(assetReference);

        return await handle.Task as T;
    }

    public async Task<GameObject> InstantiateAssetBundle_Async(AssetReference assetReference)
    {
        if (dicLoadedAssetBundle.ContainsKey(assetReference))
        {
            return dicLoadedAssetBundle[assetReference].Result as GameObject;
        }
        else
        {
            AsyncOperationHandle handle = Addressables.InstantiateAsync(assetReference);

            return await handle.Task as GameObject;
        }
    }
    #endregion

    #region Load.
    public T LoadAssetBundle<T>(string path) where T : class
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(path);

        return handle.Result as T;
    }
    public T LoadAssetBundle<T>(AssetReference assetReference) where T : class
    {
        if (dicLoadedAssetBundle.ContainsKey(assetReference))
        {
            return dicLoadedAssetBundle[assetReference].Result as T;
        }
        else
        {
            AsyncOperationHandle handle = assetReference.LoadAssetAsync<T>();

            return handle.Result as T;
        }
    }
    public IEnumerator CoLoadAssetBundleAll<T>(string path) where T : class
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(path);

        while (!handle.IsDone)
            yield return null;
    }
    public IEnumerator CoLoadAssetBundle<T>(string path, Action<T> action) where T : class
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(path);

        while (!handle.IsDone)
            yield return null;

        action?.Invoke(handle.Result as T);
    }
    public IEnumerator CoLoadAssetBundle<T>(AssetReference assetReference, Action<T> action) where T : class
    {
        if (dicLoadedAssetBundle.ContainsKey(assetReference))
        {
            action?.Invoke(dicLoadedAssetBundle[assetReference].Result as T);
        }
        else
        {
            AsyncOperationHandle handle = assetReference.LoadAssetAsync<T>();

            while (!handle.IsDone)
                yield return null;

            dicLoadedAssetBundle.Add(assetReference, handle);

            action?.Invoke(handle.Result as T);
        }
    }
    #endregion

    #region Instantiate.
    public T InstantiateAssetBundle<T>(string path) where T : class
    {
        AsyncOperationHandle handle = Addressables.InstantiateAsync(path);

        return handle.Result as T;
    }
    public T InstantiateAssetBundle<T>(AssetReference assetReference) where T : class
    {
        AsyncOperationHandle handle = assetReference.InstantiateAsync();

        return handle.Result as T;
    }
    public IEnumerator CoInstantiateAssetBundle<T>(string path, Action<T> action) where T : class
    {
        AsyncOperationHandle handle = Addressables.InstantiateAsync(path);

        while (!handle.IsDone)
            yield return null;

        action?.Invoke(handle.Result as T);
    }
    public IEnumerator CoInstantiateAssetBundle<T>(AssetReference assetReference, Action<T> action) where T : class
    {
        AsyncOperationHandle handle = assetReference.InstantiateAsync();

        while (!handle.IsDone)
            yield return null;

        action?.Invoke(handle.Result as T);
    }
    #endregion

    #region Complete Callback
    private void OnLoadAssetBundleComplete(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> handle)
    {
        GameObject result = handle.Result;
    }
    #endregion

    #region UnLoad.
    public void UnLoadInstantiateAssetBundle(GameObject gameObject)
    {
        Addressables.ReleaseInstance(gameObject);
    }

    public void UnLoadAssetBundle<T>(object param) where T : class
    {
        Addressables.Release(param as T);
    }
    public void UnLoadAssetBundle(AssetReference assetReference)
    {
        Addressables.Release(assetReference);
    }
    public void UnLoadAssetBundle(GameObject gameObject)
    {
        Addressables.Release(gameObject);
    }
    public void UnLoadAssetBundle(string path)
    {
        Addressables.Release(path);
    }
    #endregion
}