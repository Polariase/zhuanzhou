using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("地面检测")]
    public LayerMask groundLayerMask = 1 << 0;
    public float maxPlacementAngle = 15f;
    public float maxPlacementDistance = 20f;

    [Header("预览设置")]
    public float previewYOffset = 0.1f;
    public Color validColor = new Color(0f, 1f, 0f, 0.6f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.6f);

    [Header("物品预制体映射")]
    public PlaceableItem[] placeableItems;

    public bool IsPlacing { get; private set; }

    private GameObject previewObject;
    private string currentItemType;
    private Vector3 targetPosition;
    private Vector3 groundPosition;
    private bool isPositionValid;

    private LineRenderer lineRenderer;
    private GameObject wireframeObject;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!IsPlacing || previewObject == null) return;

        UpdatePreview();

        if (Input.GetMouseButtonDown(0) && isPositionValid)
            PlaceObject();

        if (Input.GetMouseButtonDown(1))
            CancelPlacement();
    }

    public void StartPlacement(string itemType)
    {
        Debug.Log($"[PlacementManager] StartPlacement 被调用，物品：{itemType}");

        if (IsPlacing) CancelPlacement();

        currentItemType = itemType;
        GameObject prefab = GetPrefabForItem(itemType);
        if (prefab == null)
        {
            Debug.LogError($"未找到物品 {itemType} 的预制体！");
            return;
        }

        Debug.Log($"[PlacementManager] 找到预制体：{prefab.name}");

        previewObject = Instantiate(prefab);
        previewObject.name = "Preview_篝火";

        previewObject.transform.SetParent(null);
        previewObject.hideFlags = HideFlags.None;
        previewObject.layer = 0;
        previewObject.SetActive(true);

        foreach (Transform child in previewObject.transform)
        {
            child.gameObject.layer = 0;
            child.gameObject.SetActive(true);
            child.gameObject.hideFlags = HideFlags.None;
        }

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Camera.main 为 null！请确保 Main Camera 的 Tag 为 MainCamera。");
            Destroy(previewObject);
            previewObject = null;
            return;
        }

        Vector3 camPos = mainCam.transform.position;
        Vector3 camForward = mainCam.transform.forward;
        Vector3 targetPos = camPos + camForward * 5f;
        targetPos.y += 0.5f;

        previewObject.transform.position = targetPos;
        previewObject.transform.rotation = Quaternion.identity;
        previewObject.transform.localScale = Vector3.one;

        Renderer[] rends = previewObject.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"Renderer 数量：{rends.Length}");
        foreach (var rend in rends)
        {
            rend.enabled = true;
            if (rend.sharedMaterial == null || rend.sharedMaterial.shader.name == "Hidden/InternalErrorShader")
            {
                rend.material = new Material(Shader.Find("Standard"));
                rend.material.color = Color.red;
            }
            else
            {
                Color c = rend.material.color;
                c.a = 1f;
                rend.material.color = c;
            }
        }

        // 创建独立的地面线框（父物体为 null）
        wireframeObject = new GameObject("Wireframe");
        wireframeObject.transform.SetParent(null);
        wireframeObject.transform.position = Vector3.zero;
        wireframeObject.transform.rotation = Quaternion.identity;
        wireframeObject.transform.localScale = Vector3.one;

        lineRenderer = wireframeObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        lineRenderer.material = mat;
        lineRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineRenderer.material.renderQueue = 3000;
        lineRenderer.positionCount = 0;

        SetPreviewTransparent(previewObject);
        IsPlacing = true;
        Cursor.visible = false;
    }

    public void CancelPlacement()
    {
        if (previewObject != null) Destroy(previewObject);
        if (wireframeObject != null) Destroy(wireframeObject);
        IsPlacing = false;
        Cursor.visible = true;
    }

    void UpdatePreview()
    {
        if (previewObject == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxPlacementDistance, groundLayerMask))
        {
            previewObject.SetActive(true);
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            isPositionValid = angle <= maxPlacementAngle;

            groundPosition = hit.point;
            targetPosition = hit.point + Vector3.up * previewYOffset;
            previewObject.transform.position = targetPosition;
            previewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            UpdateWireframe();
        }
        else
        {
            previewObject.SetActive(false);
            isPositionValid = false;
            if (lineRenderer != null) lineRenderer.positionCount = 0;
        }
    }

    void UpdateWireframe()
    {
        if (lineRenderer == null || previewObject == null) return;

        // 合并所有 Renderer 的包围盒
        Bounds bounds = new Bounds();
        Renderer[] rends = previewObject.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            bounds = rends[0].bounds;
            foreach (var r in rends)
                bounds.Encapsulate(r.bounds);
        }
        else
        {
            bounds = new Bounds(previewObject.transform.position, Vector3.one * 0.5f);
        }

        // 地面高度（抬高一点避免闪烁）
        float groundY = groundPosition.y + 0.01f;

        // 取包围盒的 XZ 尺寸
        float halfX = bounds.size.x / 2f;
        float halfZ = bounds.size.z / 2f;

        // ===== 关键：使用包围盒中心作为线框中心（XZ） =====
        Vector3 center = new Vector3(bounds.center.x, groundY, bounds.center.z);

        // 四个角（顺时针）
        Vector3 p1 = center + new Vector3(-halfX, 0, -halfZ);
        Vector3 p2 = center + new Vector3(halfX, 0, -halfZ);
        Vector3 p3 = center + new Vector3(halfX, 0, halfZ);
        Vector3 p4 = center + new Vector3(-halfX, 0, halfZ);

        Vector3[] positions = new Vector3[8];
        positions[0] = p1; positions[1] = p2;
        positions[2] = p2; positions[3] = p3;
        positions[4] = p3; positions[5] = p4;
        positions[6] = p4; positions[7] = p1;

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        Color wireColor = isPositionValid ? validColor : invalidColor;
        lineRenderer.startColor = wireColor;
        lineRenderer.endColor = wireColor;
    }

    void PlaceObject()
    {
        Vector3 pos = groundPosition;
        Quaternion rot = Quaternion.identity;

        Debug.Log($"PlaceObject 放置位置：{pos}");

        GameObject prefab = GetPrefabForItem(currentItemType);
        if (prefab != null)
        {
            GameObject placed = Instantiate(prefab, pos, rot);
            placed.transform.SetParent(null);
            placed.transform.position = pos;
            placed.transform.localScale = Vector3.one;
        }

        CancelPlacement();
    }

    GameObject GetPrefabForItem(string itemType)
    {
        foreach (var item in placeableItems)
            if (item.itemType == itemType) return item.prefab;
        return null;
    }

    void SetPreviewTransparent(GameObject obj)
    {
        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
        {
            Color c = rend.material.color;
            c.a = 0.5f;
            rend.material.color = c;
        }
    }

    [System.Serializable]
    public class PlaceableItem
    {
        public string itemType;
        public GameObject prefab;
    }
}