using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YourName.CraftingSystem
{
    public class CraftingModule_UI : MonoBehaviour
    {
        // 单例
        public static CraftingModule_UI Instance { get; private set; }

        // ---------- 在 Inspector 中手动拖拽绑定的 UI 引用 ----------
        [Header("主面板")]
        public GameObject craftingPanel;              // 整个合成界面

        [Header("左侧 - 配方按钮")]
        public Transform recipeButtonParent;          // 按钮放在哪个容器下
        public GameObject recipeButtonPrefab;         // 按钮长什么样（预制体）

        [Header("右侧 - 信息显示")]
        public TMP_Text selectedRecipeNameText;       // 显示“篝火”
        public TMP_Text woodRequirementText;          // 显示“木材: 5 (拥有: 20)”
        public TMP_Text stoneRequirementText;         // 显示“石材: 3 (拥有: 20)”

        [Header("右侧 - 打造按钮")]
        public Button craftButton;                    // 打造按钮

        [Header("库存显示")]
        public TMP_Text inventoryWoodText;            // 左下角“木材: 20”
        public TMP_Text inventoryStoneText;           // 左下角“石材: 20”

        // 当前选中的配方索引（默认第0个）
        private int currentSelectedRecipe = 0;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // 绑定按钮点击事件
            craftButton.onClick.AddListener(OnCraft);

            // 生成左侧配方按钮列表（篝火、木箱）
            GenerateRecipeButtons();

            // 默认选中第一个配方
            SelectRecipe(0);

            // 刷新界面显示
            RefreshUI();

            // 默认关闭合成界面（按C键打开）
            craftingPanel.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                // 开关合成界面
                craftingPanel.SetActive(!craftingPanel.activeSelf);
                if (craftingPanel.activeSelf)
                    RefreshUI(); // 每次打开时刷新数据
            }
        }

        // 动态生成左侧的配方按钮
        void GenerateRecipeButtons()
        {
            var logic = CraftingModule_Logic.Instance;
            if (logic == null || logic.recipes.Count == 0) return;

            for (int i = 0; i < logic.recipes.Count; i++)
            {
                // 从预制体克隆一个新按钮
                GameObject btnObj = Instantiate(recipeButtonPrefab, recipeButtonParent);
                Button btn = btnObj.GetComponent<Button>();
                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
                var recipe = logic.recipes[i];

                // 设置按钮文字（例如 "篝火 (5木+3石)"）
                btnText.text = $"{GetDisplayName(recipe.result)} ({recipe.ingredientAmounts[0]}木+{recipe.ingredientAmounts[1]}石)";

                // 点击按钮时，选中对应配方
                int index = i;
                btn.onClick.AddListener(() => SelectRecipe(index));
            }
        }

        // 选中某个配方
        void SelectRecipe(int index)
        {
            currentSelectedRecipe = index;
            RefreshUI(); // 立即刷新右侧显示
        }

        // ---------- 最重要的刷新方法：从逻辑层获取数据，更新 UI ----------
        public void RefreshUI()
        {
            var logic = CraftingModule_Logic.Instance;
            if (logic == null || logic.recipes.Count == 0) return;
            if (currentSelectedRecipe >= logic.recipes.Count)
                currentSelectedRecipe = 0;

            var recipe = logic.recipes[currentSelectedRecipe];

            // 1. 更新配方名称
            selectedRecipeNameText.text = GetDisplayName(recipe.result);

            // 2. 从逻辑层获取“背包中的真实数量”（目前是模拟数据，以后会变成真实数据）
            int woodHave = logic.GetMaterialCount(CraftItemType.Wood);
            int stoneHave = logic.GetMaterialCount(CraftItemType.Stone);

            // 3. 获取当前配方需要的数量
            int woodNeed = logic.GetRecipeRequirement(currentSelectedRecipe, CraftItemType.Wood);
            int stoneNeed = logic.GetRecipeRequirement(currentSelectedRecipe, CraftItemType.Stone);

            // 4. 更新右侧显示
            woodRequirementText.text = $"木材: {woodNeed}  (拥有: {woodHave})";
            stoneRequirementText.text = $"石材: {stoneNeed}  (拥有: {stoneHave})";

            // 5. 更新左下角库存总数
            inventoryWoodText.text = $"木材: {woodHave}";
            inventoryStoneText.text = $"石材: {stoneHave}";

            // 6. 判断能否打造，控制按钮颜色/可点击状态
            bool canCraft = logic.CanCraft(currentSelectedRecipe);
            craftButton.interactable = canCraft;

            // 改变按钮颜色：材料充足时绿色，不足时灰色
            ColorBlock colors = craftButton.colors;
            colors.disabledColor = canCraft ? Color.green : Color.gray;
            craftButton.colors = colors;
        }

        // 点击“打造”按钮时执行
        void OnCraft()
        {
            if (CraftingModule_Logic.Instance.TryCraft(currentSelectedRecipe))
            {
                Debug.Log("打造成功！");
                RefreshUI(); // 打造成功后刷新数据
            }
            else
            {
                Debug.Log("打造失败！材料不足。");
            }
        }

        // 工具方法：将枚举转成中文名称
        string GetDisplayName(CraftItemType type)
        {
            switch (type)
            {
                case CraftItemType.Wood: return "木材";
                case CraftItemType.Stone: return "石材";
                case CraftItemType.Campfire: return "篝火";
                case CraftItemType.WoodBox: return "木箱";
                default: return type.ToString();
            }
        }
    }
}