using System;

[Serializable]
public abstract class ItemRuntimeData
{
    public abstract int ItemID { get; }
    public abstract string ItemName { get; }
    public abstract ItemType Type { get; }
    public abstract string IconAddress { get; }
    public abstract string PrefabAddress { get; }
    public abstract int MaxStack { get; }
    public abstract string Desc { get; }

    public abstract ItemRuntimeData Clone();
}

[Serializable]
public class NormalItemRuntime : ItemRuntimeData
{
    private readonly ItemData _shared;

    public NormalItemRuntime(ItemData so) => _shared = so;

    public override int ItemID => _shared.itemID;
    public override string ItemName => _shared.itemName;
    public override ItemType Type => _shared.itemType;
    public override string IconAddress => _shared.iconAddress;
    public override string PrefabAddress => _shared.prefabAddress;
    public override string Desc => _shared.description;
    public override int MaxStack => _shared.maxStack;

    public override ItemRuntimeData Clone() => new NormalItemRuntime(_shared);
}

[Serializable]
public class FoodItemRuntime : ItemRuntimeData
{
    private readonly FoodData _shared;

    public float WindPot => _shared.windPot * PotScale;
    public float FirePot => _shared.firePot * PotScale;
    public float WaterPot => _shared.waterPot * PotScale;
    public float GrassPot => _shared.grassPot * PotScale;
    public float DarkPot => _shared.darkPot * PotScale;

    public MeatState meatState;

    public float PotScale => meatState switch
    {
        MeatState.Cooked => 3f,
        MeatState.Burnt => 0.5f,
        _ => 1f
    };

    public FoodItemRuntime(FoodData so, MeatState state = MeatState.Raw)
    {
        _shared = so;
        meatState = state;
    }

    public override int ItemID => _shared.itemID;
    public override string ItemName => _shared.itemName;
    public override ItemType Type => ItemType.Food;
    public override string Desc => _shared.description;
    public override int MaxStack => _shared.maxStack;

    public override string IconAddress => meatState switch
    {
        MeatState.Cooked when _shared is MeatData meat => meat.iconAddressCooked,
        MeatState.Burnt when _shared is MeatData meat => meat.iconAddressBurnt,
        _ => _shared.iconAddress
    };

    public override string PrefabAddress => meatState switch
    {
        MeatState.Cooked when _shared is MeatData meat => meat.prefabAddressCooked,
        MeatState.Burnt when _shared is MeatData meat => meat.prefabAddressBurnt,
        _ => _shared.prefabAddress
    };

    public override ItemRuntimeData Clone()
    {
        var clone = new FoodItemRuntime(_shared, meatState);
        return clone;
    }
}