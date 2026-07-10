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
    private readonly Dictionary<string, MagicSeg> _magicSegCache = new();
    private AsyncOperationHandle<IList<ItemData>> _itemLibHandle;
    private AsyncOperationHandle<IList<MagicSeg>> _magicSegLibHandle;
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

    private async void Initialize()
    {
        await LoadAllData();
    }

    private async Task LoadAllData()
    {
        _itemLibHandle = Addressables.LoadAssetsAsync<ItemData>("ItemData", null);
        _magicSegLibHandle = Addressables.LoadAssetsAsync<MagicSeg>("MagicSeg", null);
        await Task.WhenAll(_itemLibHandle.Task, _magicSegLibHandle.Task);

        bool itemLoadSuccess = _itemLibHandle.Status == AsyncOperationStatus.Succeeded;
        bool magicLoadSuccess = _magicSegLibHandle.Status == AsyncOperationStatus.Succeeded;

        if (itemLoadSuccess && magicLoadSuccess)
        {
            _itemCache.Clear();
            _magicSegCache.Clear();

            List<Task> iconLoadingTasks = new List<Task>();
            HashSet<string> processingKeys = new HashSet<string>();

            foreach (var data in _itemLibHandle.Result)
            {
                if (data != null && !_itemCache.ContainsKey(data.itemID))
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

            processingKeys.Add("法术道具Icon");
            iconLoadingTasks.Add(PreloadIconAsync("法术道具Icon"));

            foreach (var seg in _magicSegLibHandle.Result)
            {
                if (seg != null)
                {
                    if (!string.IsNullOrEmpty(seg.keyword))
                    {
                        if (!_magicSegCache.ContainsKey(seg.keyword))
                        {
                            _magicSegCache.Add(seg.keyword, seg);
                        }
                        else
                        {
                            Debug.LogWarning($"[DataManager] 发现了重复的 MagicSeg Keyword: {seg.keyword}");
                        }
                    }
                }
            }

            await Task.WhenAll(iconLoadingTasks);

            IsInitialized = true;
            Debug.Log($"[DataManager] 初始化成功。缓存了 {_itemCache.Count} 个道具，{_magicSegCache.Count} 个魔法片段。");
        }
        else
        {
            Debug.LogError($"[DataManager] 配置加载失败！ItemData状态: {_itemLibHandle.Status}, MagicSeg状态: {_magicSegLibHandle.Status}");
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

    public MagicSeg GetMagicSeg(string keyword)
    {
        if (_magicSegCache.TryGetValue(keyword, out var seg))
        {
            return seg;
        }
        return null;
    }

    public T GetMagicSeg<T>(string keyword) where T : MagicSeg
    {
        var seg = GetMagicSeg(keyword);
        if (seg != null && seg is T targetSeg)
        {
            return targetSeg;
        }
        Debug.Log("fail to get");
        return null;
    }

    public List<string> GetAllMagicSegKeywords()
    {
        return new List<string>(_magicSegCache.Keys);
    }
}