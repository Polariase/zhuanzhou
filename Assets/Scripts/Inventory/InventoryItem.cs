using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum MeatState
{
    Raw,
    Cooked,
    Burnt
}

[Serializable]
public class InventoryItem
{
    public int count;
    public ItemData data;
    public MeatState meatState = MeatState.Raw;

    public InventoryItem(ItemData itemData, int amount)
    {
        data = itemData;
        count = amount;
    }

    public void AddCount(int amount) => count += amount;

    public string GetCurrentIconAddress()
    {
        if (data is MeatData meat)
        {
            return meatState switch
            {
                MeatState.Cooked => meat.iconAddressCooked,
                MeatState.Burnt => meat.iconAddressBurnt,
                _ => meat.iconAddress
            };
        }

        return data.iconAddress;
    }

    public string GetCurrentPrefabAddress()
    {
        if (data is MeatData meat)
        {
            return meatState switch
            {
                MeatState.Cooked => meat.prefabAddressCooked,
                MeatState.Burnt => meat.prefabAddressBurnt,
                _ => meat.prefabAddress
            };
        }
        return data.prefabAddress;
    }

    public InventoryItem Clone(int newCount)
    {
        InventoryItem newItem = new(data, newCount);
        newItem.meatState = meatState;
        return newItem;
    }

    public InventoryItem Split(int splitCount)
    {
        if (splitCount <= 0 || splitCount > count) return null;

        count -= splitCount;
        return Clone(splitCount);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is InventoryItem other)
        {
            if (data == other.data && meatState == other.meatState)
                return true;
        }
        return false;
    }

    public static bool operator ==(InventoryItem left, InventoryItem right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(InventoryItem left, InventoryItem right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(data.itemID);
    }
}
