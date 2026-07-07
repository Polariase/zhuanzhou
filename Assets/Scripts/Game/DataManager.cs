using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    private readonly Dictionary<int, ItemData> _itemCache = new();
    private readonly Dictionary<string, Sprite> _iconCache = new();
    private AsyncOperationHandle<IList<ItemData>> _itemLibHandle;
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        LoadAllData();
    }

    private async void LoadAllData()
    {
        _itemLibHandle = Addressables.LoadAssetsAsync<ItemData>("ItemData", null);
        await _itemLibHandle.Task;

        if (_itemLibHandle.Status == AsyncOperationStatus.Succeeded)
        {
            _itemCache.Clear();

            List<Task> iconLoadingTasks = new List<Task>();

            HashSet<string> processingKeys = new HashSet<string>();

            foreach (var data in _itemLibHandle.Result)
            {
                if (!_itemCache.ContainsKey(data.itemID))
                {
                    _itemCache.Add(data.itemID, data);

                    if (!string.IsNullOrEmpty(data.iconAddress) &&
                        !_iconCache.ContainsKey(data.iconAddress) &&
                        processingKeys.Add(data.iconAddress))
                    {
                        iconLoadingTasks.Add(PreloadIconAsync(data.iconAddress));
                    }

                    if (data is MeatData meatData)
                    {
                        if (!string.IsNullOrEmpty(meatData.iconAddressCooked) &&
                            !_iconCache.ContainsKey(meatData.iconAddressCooked) &&
                            processingKeys.Add(meatData.iconAddressCooked))
                        {
                            iconLoadingTasks.Add(PreloadIconAsync(meatData.iconAddressCooked));
                        }

                        if (!string.IsNullOrEmpty(meatData.iconAddressBurnt) &&
                            !_iconCache.ContainsKey(meatData.iconAddressBurnt) &&
                            processingKeys.Add(meatData.iconAddressBurnt))
                        {
                            iconLoadingTasks.Add(PreloadIconAsync(meatData.iconAddressBurnt));
                        }
                    }
                }
            }

            await Task.WhenAll(iconLoadingTasks);

            IsInitialized = true;
            Debug.Log($"[DataManager] 成功加载了 {_itemCache.Count} 个配置及其所有独立图标。");
        }
        else
        {
            Debug.LogError("[DataManager] 道具配置加载失败！");
        }
    }

    private async Task PreloadIconAsync(string address)
    {
        if (string.IsNullOrEmpty(address) || _iconCache.ContainsKey(address)) return;

        var iconHandle = Addressables.LoadAssetAsync<Sprite>(address);
        await iconHandle.Task;

        if (iconHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // 双重检查防重名 Key 冲突
            if (!_iconCache.ContainsKey(address))
            {
                _iconCache.Add(address, iconHandle.Result);
            }
        }
        else
        {
            Debug.LogWarning($"[DataManager] 找不到图标资源，Addressable Key: {address}");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            foreach (var sprite in _iconCache.Values)
            {
                if (sprite != null) Addressables.Release(sprite);
            }
            _iconCache.Clear();

            if (_itemLibHandle.IsValid())
            {
                Addressables.Release(_itemLibHandle);
            }
            _itemCache.Clear();

            Instance = null;
        }
    }

    public ItemData GetItemData(int id)
    {
        if (_itemCache.TryGetValue(id, out var data))
        {
            return data;
        }
        return null;
    }

    public Sprite GetIcon(string addr)
    {
        if(_iconCache.TryGetValue(addr, out var sprite))
        {
            return sprite;
        }
        return null;
    }
}