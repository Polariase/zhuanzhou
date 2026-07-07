using UnityEngine;
using System.Collections.Generic;

namespace YourName.CraftingSystem
{
    public class CraftingModule_Logic : MonoBehaviour
    {
        // 单例模式，方便 UI 脚本调用
        public static CraftingModule_Logic Instance { get; private set; }

        // 存储所有配方的列表（在 Awake 中初始化）
        public List<CraftRecipe> recipes = new List<CraftRecipe>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // 在这里定义具体的配方数据
            recipes.Add(new CraftRecipe()
            {
                result = CraftItemType.Campfire,
                resultAmount = 1,
                ingredients = new CraftItemType[] { CraftItemType.Wood, CraftItemType.Stone },
                ingredientAmounts = new int[] { 5, 3 }
            });
            recipes.Add(new CraftRecipe()
            {
                result = CraftItemType.WoodBox,
                resultAmount = 1,
                ingredients = new CraftItemType[] { CraftItemType.Wood, CraftItemType.Stone },
                ingredientAmounts = new int[] { 8, 4 }
            });
        }

        // ============================================================
        //   ?? ?? ?? 这里是“背包数量同步”的核心接口（你需要修改的地方）
        // ============================================================

        // 1. 检查背包里是否够用
        private bool HasEnoughMaterial(CraftItemType type, int amount)
        {
            // ?? 目前是模拟：永远返回 true（表示材料充足）
            // ?? 未来改为： return TeamInventory.Instance.GetCount(type) >= amount;
            Debug.Log($"[模拟] 检查 {type} 是否 >= {amount} → true");
            return true;
        }

        // 2. 从背包扣除材料
        private void RemoveFromBackpack(CraftItemType type, int amount)
        {
            // ?? 目前是模拟：只打印日志
            // ?? 未来改为： TeamInventory.Instance.Remove(type, amount);
            Debug.Log($"[模拟] 扣除 {type} x{amount}");
        }

        // 3. 添加产物到背包
        private void AddToBackpack(CraftItemType type, int amount)
        {
            // ?? 目前是模拟：只打印日志
            // ?? 未来改为： TeamInventory.Instance.Add(type, amount);
            Debug.Log($"[模拟] 添加 {type} x{amount}");
        }

        // 4. 获取背包中某材料的当前数量（UI显示“拥有: X”时调用）
        public int GetMaterialCount(CraftItemType type)
        {
            // ?? 目前是模拟：永远返回 20
            // ?? 未来改为： return TeamInventory.Instance.GetCount(type);
            return 20;
        }

        // ============================================================
        //  ?? 以上就是所有需要对接背包的地方，只有这 4 个方法
        // ============================================================

        // 判断某个配方是否能打造（材料是否足够）
        public bool CanCraft(int recipeIndex)
        {
            if (recipeIndex < 0 || recipeIndex >= recipes.Count) return false;
            var recipe = recipes[recipeIndex];
            for (int i = 0; i < recipe.ingredients.Length; i++)
            {
                // 只要有一种材料不够，就返回 false
                if (!HasEnoughMaterial(recipe.ingredients[i], recipe.ingredientAmounts[i]))
                    return false;
            }
            return true;
        }

        // 执行打造
        public bool TryCraft(int recipeIndex)
        {
            if (!CanCraft(recipeIndex)) return false;

            var recipe = recipes[recipeIndex];
            // 1. 扣除所有材料
            for (int i = 0; i < recipe.ingredients.Length; i++)
                RemoveFromBackpack(recipe.ingredients[i], recipe.ingredientAmounts[i]);
            // 2. 添加产物
            AddToBackpack(recipe.result, recipe.resultAmount);

            // 3. 刷新 UI（告诉 UI 重新读取数据并显示）
            //CraftingModule_UI.Instance?.RefreshUI();
            return true;
        }

        // 获取某个配方对某材料的需求数量（供 UI 显示“需要 X 个”）
        public int GetRecipeRequirement(int recipeIndex, CraftItemType type)
        {
            if (recipeIndex < 0 || recipeIndex >= recipes.Count) return 0;
            var recipe = recipes[recipeIndex];
            for (int i = 0; i < recipe.ingredients.Length; i++)
            {
                if (recipe.ingredients[i] == type)
                    return recipe.ingredientAmounts[i];
            }
            return 0;
        }
    }
}