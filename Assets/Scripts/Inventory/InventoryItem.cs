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
    public ItemRuntimeData runtimeData;

    public InventoryItem(ItemData itemData, int amount, MeatState defaultMeatState = MeatState.Raw)
    {
        count = amount;

        if (itemData is FoodData foodData)
        {
            runtimeData = new FoodItemRuntime(foodData, defaultMeatState);
        }
        else
        {
            runtimeData = new NormalItemRuntime(itemData);
        }
    }

    public InventoryItem(ItemRuntimeData customRuntimeData, int amount)
    {
        count = amount;
        runtimeData = customRuntimeData;
    }

    public void AddCount(int amount) => count += amount;

    public string GetCurrentIconAddress() => runtimeData.IconAddress;
    public string GetCurrentPrefabAddress() => runtimeData.PrefabAddress;

    public InventoryItem Clone(int newCount)
    {
        InventoryItem newItem = new(null, newCount);
        newItem.runtimeData = runtimeData.Clone();
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
            if (runtimeData == null || other.runtimeData == null) return false;

            if (runtimeData.ItemID != other.runtimeData.ItemID) return false;

            if (runtimeData is FoodItemRuntime thisFood && other.runtimeData is FoodItemRuntime otherFood)
            {
                return thisFood.meatState == otherFood.meatState;
            }

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
        if (runtimeData is FoodItemRuntime food)
        {
            return HashCode.Combine(runtimeData.ItemID, food.meatState);
        }
        return HashCode.Combine(runtimeData.ItemID);
    }
}
