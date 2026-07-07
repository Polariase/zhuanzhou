using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Data;
using ExcelDataReader;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class ExcelToSOConverter
{
    private static Dictionary<string, ItemType> excelConfigs = new()
    {
        { Path.Combine(Application.dataPath, "Configs/NormalItem.xlsx"), ItemType.Normal },
        { Path.Combine(Application.dataPath, "Configs/FoodItem.xlsx"), ItemType.Food }
    };

    private static string soSavePath = "Assets/GameData/Items/";
    private static string addressableLabel = "ItemData";

    [MenuItem("Tools/Import Items From Excel")]
    public static void Import()
    {
        if (!Directory.Exists(soSavePath))
            Directory.CreateDirectory(soSavePath);

        foreach (var config in excelConfigs)
        {
            string excelPath = config.Key;
            ItemType currentTableType = config.Value;

            if (!File.Exists(excelPath))
            {
                Debug.LogWarning($"[Converter] 找不到目标 Excel 表格，跳过: {excelPath}");
                continue;
            }

            ProcessSingleExcel(excelPath, currentTableType);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green><b>[Converter] 所有 Excel 数据导入并自动标记 Addressables 成功！</b></color>");
    }

    private static void ProcessSingleExcel(string excelPath, ItemType tableType)
    {
        using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var sheet = result.Tables[0];

                if (sheet.Rows.Count < 2)
                {
                    Debug.LogError($"[Converter] 表格 {Path.GetFileName(excelPath)} 数据行数不足。");
                    return;
                }

                Dictionary<string, int> headerMap = new Dictionary<string, int>();
                DataRow headerRow = sheet.Rows[0];
                for (int col = 0; col < sheet.Columns.Count; col++)
                {
                    string headerName = headerRow[col]?.ToString().Trim();
                    if (!string.IsNullOrEmpty(headerName) && !headerMap.ContainsKey(headerName))
                    {
                        headerMap.Add(headerName, col);
                    }
                }

                // 逐行解析数据
                for (int i = 1; i < sheet.Rows.Count; i++)
                {
                    DataRow row = sheet.Rows[i];

                    string GetValue(string columnName)
                    {
                        if (headerMap.TryGetValue(columnName, out int index))
                        {
                            return row[index]?.ToString() ?? "";
                        }
                        return "";
                    }

                    string idStr = GetValue("ItemID");
                    if (string.IsNullOrEmpty(idStr)) continue;

                    int id = int.Parse(idStr);
                    string assetPath = $"{soSavePath}Item_{id}.asset";

                    ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                    System.Type targetType = typeof(ItemData);
                    if (tableType == ItemType.Food)
                    {
                        // 检查是否标记为肉类
                        string isMeatStr = GetValue("IsMeat");
                        bool isMeat = isMeatStr.ToLower() == "true";

                        targetType = isMeat ? typeof(MeatData) : typeof(FoodData);
                    }

                    if (item != null && item.GetType() != targetType)
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        item = null;
                    }

                    if (item == null)
                    {
                        item = (ItemData)ScriptableObject.CreateInstance(targetType);
                        item.itemType = tableType;
                        AssetDatabase.CreateAsset(item, assetPath);
                    }

                    item.itemID = id;
                    item.itemName = GetValue("Name");
                    item.description = GetValue("Description");

                    int.TryParse(GetValue("MaxStack"), out int maxStack);
                    item.maxStack = maxStack == 0 ? 1 : maxStack;

                    if (item is MeatData meat)
                    {
                        meat.iconAddress = "生肉Icon";
                        meat.prefabAddress = "生肉";
                        meat.iconAddressCooked = "熟肉Icon";
                        meat.prefabAddressCooked = "熟肉";
                        meat.iconAddressBurnt = "糊肉Icon";
                        meat.prefabAddressBurnt = "糊肉";
                    }
                    else
                    {
                        item.prefabAddress = item.itemName;
                        item.iconAddress = item.itemName + "Icon";
                    }

                    // 3. 如果是食物，填充特有的“潜力”属性
                    if (tableType == ItemType.Food && item is FoodData food)
                    {
                        float.TryParse(GetValue("Wind"), out float wind);
                        float.TryParse(GetValue("Fire"), out float fire);
                        float.TryParse(GetValue("Water"), out float water);
                        float.TryParse(GetValue("Grass"), out float grass);
                        float.TryParse(GetValue("Dark"), out float dark);

                        food.windPot = wind;
                        food.firePot = fire;
                        food.waterPot = water;
                        food.grassPot = grass;
                        food.darkPot = dark;
                    }

                    EditorUtility.SetDirty(item);

                    SetAddressableInfo(assetPath, addressableLabel);
                }
            }
        }
    }

    /// <summary>
    /// 自动将指定路径的资源加入 Addressable 组并设置 Label
    /// </summary>
    private static void SetAddressableInfo(string assetPath, string labelName)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[Converter] 找不到 AddressableAssetSettings，请确认你已经创建了 Addressables 组。");
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        // 寻找到指定的 Group（如果没有就用默认组）
        AddressableAssetGroup group =  settings.DefaultGroup;

        // 创建或获取对应资源的 Entry（实体）
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);

        if (entry != null)
        {
            // 将 Address 名字改为纯文件名（如 Item_1001），方便 DataManager 加载
            entry.address = Path.GetFileNameWithoutExtension(assetPath);

            // 自动加上标签（如果项目里还没有这个标签，会自动创建）
            if (!entry.labels.Contains(labelName))
            {
                settings.AddLabel(labelName);
                entry.SetLabel(labelName, true);
            }
        }
    }
}