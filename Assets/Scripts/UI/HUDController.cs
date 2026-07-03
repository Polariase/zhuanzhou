using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Image hpFill;
    public TextMeshProUGUI hpText;
    public Image hpBufferFill;
    public Image manaFill;
    public TextMeshProUGUI manaText;
    public Image manaBufferFill;
    public Image staminaFill;
    public TextMeshProUGUI staminaText;
    public Image staminaBufferFill;
    public HotbarController hotbar;

    private float bufferDelay = 0.5f;
    private float bufferShrinkSpeed = 1.0f;

    private float _hpBufferPct;
    private float _hpBufferTimer;
    private bool _isHpBuffering;

    private float _manaBufferPct;
    private float _manaBufferTimer;
    private bool _isManaBuffering;

    private float _staminaBufferPct;
    private float _staminaBufferTimer;
    private bool _isStaminaBuffering;

    private PlayerStats _stats;

    private void Start()
    {
        Initialize(PlayerController.Instance);
    }

    private void Update()
    {
        HandleHpBuffer();
        HandleManaBuffer();
        HandleStaminaBuffer();
    }

    private void HandleHpBuffer()
    {
        if (!_isHpBuffering) return;

        if (_hpBufferTimer > 0)
        {
            _hpBufferTimer -= Time.deltaTime;
        }
        else
        {
            _hpBufferPct = Mathf.MoveTowards(_hpBufferPct, hpFill.fillAmount, bufferShrinkSpeed * Time.deltaTime);
            hpBufferFill.fillAmount = _hpBufferPct;

            if (Mathf.Approximately(_hpBufferPct, hpFill.fillAmount))
            {
                _isHpBuffering = false;
            }
        }
    }

    private void HandleManaBuffer()
    {
        if (!_isManaBuffering) return;

        if (_manaBufferTimer > 0)
        {
            _manaBufferTimer -= Time.deltaTime;
        }
        else
        {
            _manaBufferPct = Mathf.MoveTowards(_manaBufferPct, manaFill.fillAmount, bufferShrinkSpeed * Time.deltaTime);
            manaBufferFill.fillAmount = _manaBufferPct;

            if (Mathf.Approximately(_manaBufferPct, manaFill.fillAmount))
            {
                _isManaBuffering = false;
            }
        }
    }

    private void HandleStaminaBuffer()
    {
        if (!_isStaminaBuffering) return;

        if (_staminaBufferTimer > 0)
        {
            _staminaBufferTimer -= Time.deltaTime;
        }
        else
        {
            _staminaBufferPct = Mathf.MoveTowards(_staminaBufferPct, staminaFill.fillAmount, bufferShrinkSpeed * Time.deltaTime);
            staminaBufferFill.fillAmount = _staminaBufferPct;

            if (Mathf.Approximately(_staminaBufferPct, staminaFill.fillAmount))
            {
                _isStaminaBuffering = false;
            }
        }
    }

    public void Initialize(PlayerController pc)
    {
        Cleanup();
        _stats = pc.stats;
        //if (_stats == null) return;
        _stats.OnHpChanged += OnHpChanged;
        _stats.OnManaChanged += OnManaChanged;
        _stats.OnStaminaChanged += OnStaminaChanged;
        OnHpChanged(_stats.hp, _stats.maxHp);
        OnManaChanged(_stats.mana, _stats.maxMana);
        OnStaminaChanged(_stats.stamina, _stats.maxStamina);
        //hotbar.Initialize(pc);
    }

    public void Cleanup()
    {
        if (_stats != null)
        {
            _stats.OnHpChanged -= OnHpChanged;
            _stats.OnManaChanged -= OnManaChanged;
            _stats.OnStaminaChanged -= OnStaminaChanged;
            _stats = null;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void OnHpChanged(float curValue, float maxValue)
    {
        if (maxValue <= 0) return;

        float pct = Mathf.Clamp01(curValue / maxValue);

        if (pct < hpFill.fillAmount)
        {
            _hpBufferTimer = bufferDelay;
            _isHpBuffering = true;       
        }
        else
        {
            if (pct >= _hpBufferPct)
            {
                _hpBufferPct = pct;
                hpBufferFill.fillAmount = _hpBufferPct;
                _isHpBuffering = false;
            }
        }

        hpFill.fillAmount = pct;

        hpText.SetText("{0}/{1}", Mathf.RoundToInt(curValue),Mathf.RoundToInt(maxValue));
    }

    private void OnManaChanged(float curValue, float maxValue)
    {
        if (maxValue <= 0) return;

        float pct = Mathf.Clamp01(curValue / maxValue);

        if (pct < manaFill.fillAmount)
        {
            _manaBufferTimer = bufferDelay;
            _isManaBuffering = true;
        }
        else
        {
            if (pct >= _manaBufferPct)
            {
                _manaBufferPct = pct;
                manaBufferFill.fillAmount = _manaBufferPct;
                _isManaBuffering = false;
            }
        }

        manaFill.fillAmount = pct;

        manaText.SetText("{0}/{1}", Mathf.RoundToInt(curValue), Mathf.RoundToInt(maxValue));
    }

    private void OnStaminaChanged(float curValue, float maxValue)
    {
        if (maxValue <= 0) return;

        float pct = Mathf.Clamp01(curValue / maxValue);
        staminaFill.fillAmount = pct;

        if (pct < staminaFill.fillAmount)
        {
            _staminaBufferTimer = bufferDelay;
            _isStaminaBuffering = true;
        }
        else
        {
            if (pct >= _staminaBufferPct)
            {
                _staminaBufferPct = pct;
                staminaBufferFill.fillAmount = _staminaBufferPct;
                _isStaminaBuffering = false;
            }
        }

        staminaText.SetText("{0}/{1}", Mathf.RoundToInt(curValue), Mathf.RoundToInt(maxValue));
    }
}
