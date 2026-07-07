namespace YourName.CraftingSystem
{
    public enum CraftItemType
    {
        Wood,
        Stone,
        Campfire,
        WoodBox
    }

    [System.Serializable]
    public class CraftRecipe
    {
        public CraftItemType result;
        public int resultAmount = 1;
        public CraftItemType[] ingredients;
        public int[] ingredientAmounts;
    }
}