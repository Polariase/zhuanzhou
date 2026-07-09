using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EasyTextEffects;
using Unity.VisualScripting;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;
using UnityEditor;

public class WordOrbitController : MonoBehaviour
{
    private class ActiveSeg
    {
        public GameObject go;
        public string poolKey;
    }

    private TextMeshProUGUI _chantTextMesh;
    private string _inputBuffer = "";
    private Camera _mainCamera;
    private PlayerController _pc;

    [Header("Orbit Settings")]
    public float baseRadius = 200f;    
    public float rotateSpeed = 30f;

    public float layerHeightStep = 50f;
    public float layerRadiusStep = 20f;
    public float layerAngleOffset = 45f;

    [Header("Fade Settings")]
    public float fadeSpeed = 3f;

    private CanvasGroup _canvasGroup;
    private Dictionary<string, List<ActiveSeg>> _layerDictionary = new Dictionary<string, List<ActiveSeg>>()
    {
        { "Arcana", new List<ActiveSeg>() },
        { "Element", new List<ActiveSeg>() },
        { "Emitter", new List<ActiveSeg>() },
        { "Modifier", new List<ActiveSeg>() }
    };
    private List<ActiveSeg> _allSpawnedTexts = new List<ActiveSeg>();

    private float _currentAngle = 0f;
    private bool _isFadingOut = false;
    private float _startTime = 0f;

    private Transform _trackTarget;
    private Vector3 _positionOffset;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (_chantTextMesh == null) _chantTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void InitAndSpawn(List<string> keywords, Transform target, Vector3 offset)
    {
        _trackTarget = target;
        _positionOffset = offset;
        _mainCamera = Camera.main;
        _pc = PlayerController.Instance;
        _inputBuffer = "";
        _startTime = Time.time;
        UpdateTextDisplay();

        UnbindInput();
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnChantTyping;
        }

        if (_trackTarget != null)
        {
            transform.position = _trackTarget.position + _positionOffset;
        }

        _isFadingOut = false;
        _canvasGroup.alpha = 0f;

        ClearActiveTexts();

        if (keywords == null || keywords.Count == 0) return;

        int count = keywords.Count;
        for (int i = 0; i < count; i++)
        {
            string kw = keywords[i];
            var segData = DataManager.Instance.GetMagicSeg(kw);
            if (segData == null) continue;

            string poolKey = segData.segStr;

            GameObject txtGo = PopupManager.Instance.SpawnMagicSegText(poolKey, transform.position, transform);

            if (txtGo != null)
            {
                txtGo.transform.DOKill();
                txtGo.transform.localScale = Vector3.one;

                txtGo.GetComponent<TextMeshProUGUI>().SetText(segData.segName);
                txtGo.GetComponent<TextEffect>().Refresh();

                ActiveSeg newSeg = new ActiveSeg { go = txtGo, poolKey = poolKey };
                _layerDictionary[poolKey].Add(newSeg);
                _allSpawnedTexts.Add(newSeg);
            }
        }
    }

    public void AbortChant()
    {
        _inputBuffer = "";
        UpdateTextDisplay();
        Dismiss();
    }

    public void Dismiss()
    {
        _isFadingOut = true;
        UnbindInput();
    }

    public void CompleteChant()
    {
        if (!string.IsNullOrWhiteSpace(_inputBuffer))
        {
            ParseAndAddToBackpack(_inputBuffer);
        }

        _inputBuffer = "";
        UpdateTextDisplay();

        Dismiss();
    }

    private void UpdateTextDisplay()
    {
        if (_chantTextMesh == null) return;

        _chantTextMesh.text = _inputBuffer;
    }

    private void ParseAndAddToBackpack(string rawText)
    {
        string[] rawTokens = rawText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (rawTokens.Length == 0) return;

        ArcanaSeg arcana = null;
        ElementSeg ele = null;
        EmitterSeg em = null;
        ModifierSeg mod = null;

        foreach (var token in rawTokens)
        {
            var baseSeg = DataManager.Instance.GetMagicSeg(token);
            if (baseSeg == null) continue;

            if (baseSeg is ArcanaSeg a) arcana = a;
            else if (baseSeg is ElementSeg el) ele = el;
            else if (baseSeg is EmitterSeg emi) em = emi;
            else if (baseSeg is ModifierSeg m) mod = m;
        }

        if (arcana != null)
        {
            MagicItemRuntime magicItem = new MagicItemRuntime(arcana, ele, em, mod);

            BackpackController backpack = InventoryManager.Instance.backpack;
            backpack.GetData().AddItem(new InventoryItem(magicItem, 1));
            Debug.Log($"成功合成法术道具并存入背包: {arcana.segName}");
        }
        else
        {
            Debug.Log("未输入核心词");
        }
    }

    void Update()
    {
        float targetAlpha = _isFadingOut ? 0f : 1f;
        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        if (_isFadingOut && _canvasGroup.alpha <= 0.01f)
        {
            ClearActiveTexts();
            gameObject.SetActive(false);
            return;
        }

        if (_trackTarget != null && !_isFadingOut)
        {
            transform.position = _trackTarget.position + _positionOffset;
        }

        if (_chantTextMesh != null && _mainCamera != null)
        {
            _chantTextMesh.transform.localPosition = new Vector3(0f, 200f, 0f);
            _chantTextMesh.transform.rotation = Quaternion.LookRotation(_mainCamera.transform.forward);
        }

        _currentAngle += rotateSpeed * Time.deltaTime;

        string[] layersOrder = { "Modifier", "Emitter", "Arcana", "Element" };

        for (int layerIndex = 0; layerIndex < layersOrder.Length; layerIndex++)
        {
            string layerKey = layersOrder[layerIndex];
            List<ActiveSeg> layerTexts = _layerDictionary[layerKey];
            int countInLayer = layerTexts.Count;
            if (countInLayer == 0) continue;

            float angleStep = 360f / countInLayer;
            float currentLayerHeight = layerIndex * layerHeightStep;
            float currentLayerStartAngle = layerIndex * layerAngleOffset;
            float currentRadius = baseRadius + (layerIndex * layerRadiusStep);

            for (int i = 0; i < countInLayer; i++)
            {
                if (layerTexts[i].go == null) continue;

                float angle = _currentAngle + currentLayerStartAngle + (i * angleStep);
                float radians = angle * Mathf.Deg2Rad;

                float x = Mathf.Cos(radians) * currentRadius;
                float z = Mathf.Sin(radians) * currentRadius;

                layerTexts[i].go.transform.localPosition = new Vector3(x, currentLayerHeight, z);

                if (_trackTarget != null)
                {
                    Vector3 directionToPlayer = layerTexts[i].go.transform.position - _trackTarget.position - _positionOffset;
                    directionToPlayer.y = 0f;

                    if (directionToPlayer.sqrMagnitude > 0.001f)
                    {
                        layerTexts[i].go.transform.rotation = Quaternion.LookRotation(directionToPlayer);
                    }
                }
            }
        }
    }

    private void ClearActiveTexts()
    {
        foreach (var seg in _allSpawnedTexts)
        {
            if (seg.go != null)
            {
                PopupManager.Instance.HideMagicSegText(seg.go, seg.poolKey);
            }
        }
        _allSpawnedTexts.Clear();

        foreach (var list in _layerDictionary.Values)
        {
            list.Clear();
        }
    }

    private void OnChantTyping(char ch)
    {
        if (!_pc.isChanting || !_pc.CanAct || Time.time - _startTime < 0.1f) return;
        if (ch == '\b')
        {
            if (_inputBuffer.Length > 0)
            {
                _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1);
            }
        }
        else
        {
            _inputBuffer += ch;
        }

        UpdateTextDisplay();
    }

    private void UnbindInput()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnChantTyping;
        }
    }

    private void OnDisable()
    {
        UnbindInput();
    }
}
