using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ItemUseContext
{
    public PlayerController player;
    public Camera mainCamera;
}

public interface IItemUseHandler
{
    void OnUseStart(ItemUseContext context);

    void OnUseTick(ItemUseContext context);

    void OnUseEnd(ItemUseContext context);
}