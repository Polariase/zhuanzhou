using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using MyPool;
using System.Collections.Generic;

public class ItemObjectPool : MyObjectPool
{
    private Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();

    public async Task SpawnItemAsync(InventoryItem item, Vector3 position, Quaternion rotation = default)
    {
        string key = item.GetCurrentPrefabAddress();

        if (!pool.ContainsKey(key))
        {
            await CreatePoolFromAddressable(key);
        }

        GameObject obj = Get(key);
        obj.transform.SetPositionAndRotation(position, rotation);

        if (obj.TryGetComponent<ItemObject>(out var itemObj))
        {
            itemObj.Setup(item);
        }
    }

    public async Task SpawnAndThrowItemAsync(InventoryItem item, Vector3 position, Vector3 targetDirection, float upwardForce = 8f, float forwardForce = 2f)
    {
        string key = item.GetCurrentPrefabAddress();
        if (!pool.ContainsKey(key)) await CreatePoolFromAddressable(key);

        GameObject obj = Get(key);
        obj.transform.SetPositionAndRotation(position, Quaternion.identity);

        if (obj.TryGetComponent<ItemObject>(out var itemObj))
        {
            itemObj.Setup(item);
        }

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            targetDirection.y = 0;
            Vector3 throwDir = targetDirection.normalized;
            Vector3 forceVector = (throwDir * forwardForce) + (Vector3.up * upwardForce);
            rb.AddForce(forceVector, ForceMode.Impulse);
        }
    }

    private async Task CreatePoolFromAddressable(string key)
    {
        if (pool.ContainsKey(key)) return;

        if (_loadedPrefabs.ContainsKey(key))
        {
            while (!pool.ContainsKey(key))
            {
                await Task.Yield();
            }
            return;
        }

        _loadedPrefabs.Add(key, null);

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedPrefabs[key] = handle.Result;

            pool.Add(key, CreatePool(handle.Result));
        }
    }

    public void ReleaseItem(GameObject obj)
    {
        if (obj.TryGetComponent<PoolItem>(out var poolItem))
        {
            string key = poolItem.key;

            if (string.IsNullOrEmpty(key) || !pool.ContainsKey(key))
            {
                Debug.LogWarning($"[ItemObjectPool] 找不到对应的对象池 Key: '{key}'，该物体将被直接销毁。");

                if (obj.TryGetComponent<ItemObject>(out var itemObj))
                {
                    itemObj.Setup(null);
                }
                Destroy(obj);
                return;
            }
        }
        else
        {
            Debug.LogWarning($"[ItemObjectPool] 回收的对象 {obj.name} 身上缺少 PoolItem 组件，将直接销毁。");
            Destroy(obj);
            return;
        }

        obj.GetComponent<ItemObject>().Setup(null);
        Release(obj, obj.GetComponent<PoolItem>().key);
    }
}