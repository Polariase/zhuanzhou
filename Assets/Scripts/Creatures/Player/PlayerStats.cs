using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ElementStats
{
    public ElementType type;
    public int lvl;
    public float exp;
    public float pot;

    public ElementStats(ElementType type, int lvl, float pot)
    {
        this.type = type;
        this.lvl = lvl;
        exp = 0;
        this.pot = pot;
    }

    public static int ExpReq(int lv)
    {
        return Mathf.RoundToInt(10f * Mathf.Pow(1.1f, lv));
    }

    public void GainExp(float value)
    {
        exp += value * pot/100f;
        while (TryUpgrade()) ;
    }

    public bool TryUpgrade()
    {
        if(exp > ExpReq(lvl))
        {
            exp -= ExpReq(lvl);
            lvl++;
            pot *= 0.8f;
            return true;
        }
        return false;
    }
}

[Serializable]
public class PlayerStats
{
    public int currentSelectedIndex = 0;
    public InventoryItem currentSelectedItem;
    public Action<int, InventoryItem> OnSelectedChanged;

    public void ClearAllSubscribers()
    {
        OnSelectedChanged = null;
        OnHpChanged = null;
        OnManaChanged = null;
        OnStaminaChanged = null;
    }

    public void ResetStatus(bool heal = false)
    {
        if (heal)
        {
            hp = maxHp;
            mana = maxMana;
            stamina = maxStamina;
        }

        _hpRecTimer = 0f;
        _manaRecTimer = 0f;
        _stRecTimer = 0f;

        currentSelectedIndex = 0;
        currentSelectedItem = null;
    }

    public void UpdateSelection(int index,InventoryItem newItem)
    {
        currentSelectedIndex = index;
        currentSelectedItem = newItem;
        OnSelectedChanged?.Invoke(currentSelectedIndex, newItem);
    }

    //½ÇÉ«ÊôÐÔ
    public float maxHp = 100f;
    public float hp = 100f;
    public Action<float, float> OnHpChanged;

    public float maxMana = 100f;
    public float mana = 100f;
    public Action<float, float> OnManaChanged;

    public float maxStamina = 100f;
    public float stamina = 100f;
    public Action<float, float> OnStaminaChanged;

    public float stRec = 20f;
    public float hpRec = 2f;
    public float manaRec = 20f;
    public float recCooldown = 1f;
    private float _hpRecTimer;
    private float _manaRecTimer;
    private float _stRecTimer;

    public Dictionary<ElementType, ElementStats> elementStats = new Dictionary<ElementType, ElementStats>();

    public PlayerStats()
    {
        elementStats.Add(ElementType.Wind, new ElementStats(ElementType.Wind, 1, 100f));
        elementStats.Add(ElementType.Water, new ElementStats(ElementType.Water, 1, 100f));
        elementStats.Add(ElementType.Fire, new ElementStats(ElementType.Fire, 1, 100f));
        elementStats.Add(ElementType.Grass, new ElementStats(ElementType.Grass, 1, 100f));
        elementStats.Add(ElementType.Dark, new ElementStats(ElementType.Dark, 0, 0f));
    }

    public List<ElementStats> GetSortedElementStats()
    {
        if (elementStats == null || elementStats.Count == 0)
        {
            return new List<ElementStats>();
        }

        return elementStats.Values                     
            .OrderByDescending(element => element.lvl) 
            .ThenByDescending(element => element.exp)  
            .ToList();                                 
    }

    public void RecoverHp(float value)
    {
        if (_hpRecTimer <= 0f)
            Heal(value * hpRec);
        else
            _hpRecTimer -= value;
    }

    public void RecoverMana(float value)
    {
        if (_manaRecTimer <= 0f)
            Regene(value * manaRec);
        else
            _manaRecTimer -= value;
    }

    public void RecoverStamina(float value)
    {
        if (_stRecTimer <= 0f)
            Rest(value * stRec);
        else
            _stRecTimer -= value;
    }

    public void Heal(float amount)
    {
        if (amount < 0) _hpRecTimer = recCooldown;
        hp = Mathf.Clamp(hp + amount,0f, maxHp);
        OnHpChanged?.Invoke(hp, maxHp);
    }

    public void Regene(float amount)
    {
        if(amount < 0) _manaRecTimer = recCooldown;
        mana = Mathf.Clamp(mana + amount,0f, maxMana);
        OnManaChanged?.Invoke(mana, maxMana);
    }

    public void Rest(float amout)
    {
        if (amout < 0) _stRecTimer = recCooldown;
        stamina = Mathf.Clamp(stamina + amout,0f, maxStamina);
        OnStaminaChanged?.Invoke(stamina, maxStamina);
    }

    public void ModifyMaxHp(float newMaxHp, bool setToMax = false)
    {
        float oldMax = maxHp;
        maxHp = Mathf.Max(newMaxHp, 1);

        if (setToMax)
        {
            hp = maxHp;
        }
        else
        {
            float pct = hp / oldMax;
            hp = maxHp * pct;
        }
        OnHpChanged?.Invoke(hp, maxHp);
    }

    public void ModifyMaxMana(float newMaxMana, bool setToMax = false)
    {
        float oldMax = maxMana;
        maxMana = Mathf.Max(newMaxMana, 1);

        if (setToMax)
        {
            mana = maxMana;
        }
        else
        {
            float pct = mana / oldMax;
            mana = maxMana * pct;
        }
        OnManaChanged?.Invoke(mana, maxMana);
    }

    public void ModifyMaxStamina(int newMaxStamina, bool setToMax = false)
    {
        float oldMax = maxStamina;
        maxStamina = Mathf.Max(newMaxStamina, 1);

        if (setToMax)
        {
            stamina = maxStamina;
        }
        else
        {
            float pct = stamina / oldMax;
            stamina = maxStamina * pct;
        }
        OnStaminaChanged?.Invoke(stamina, maxStamina);
    }


}
