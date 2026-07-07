namespace YourName.CraftingSystem
{
    // 1. 定义所有物品类型（枚举）
    public enum CraftItemType
    {
        Wood,       // 木材
        Stone,      // 石材
        Campfire,   // 篝火
        WoodBox     // 木箱
    }

    // 2. 定义“合成配方”的数据结构（一个配方包含：产物、所需材料、所需数量）
    [System.Serializable]
    public class CraftRecipe
    {
        public CraftItemType result;          // 产出什么（例如 Campfire）
        public int resultAmount = 1;          // 产出多少个（例如 1）
        public CraftItemType[] ingredients;   // 需要哪些材料（例如 [Wood, Stone]）
        public int[] ingredientAmounts;       // 对应需要多少个（例如 [5, 3]）
    }
}