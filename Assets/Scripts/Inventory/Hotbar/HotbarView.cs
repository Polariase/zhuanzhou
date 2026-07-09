using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HotbarView : MonoBehaviour
{
    [SerializeField] private InventoryDisplay _display;

    private InventoryData _data;
    private IInventoryHandler _handler;
    private PlayerStats _states;

    public void Initialize(InventoryData data, IInventoryHandler handler, PlayerStats stateData)
    {
        Cleanup();
        _data = data;
        _handler = handler;
        _states = stateData;
        _display.Setup(_data, _handler, _data.CurrentCapacity, 0);
        if (_states != null)
        {
            _states.OnSelectedChanged += UpdateHighlight;
            UpdateHighlight(_states.currentSelectedIndex, _states.currentSelectedItem);
        }
    }

    public void Cleanup()
    {
        if (_states != null)
        {
            _states.OnSelectedChanged -= UpdateHighlight;
            _states = null;
        }
    }

    public void OnDestroy()
    {
        Cleanup();
    }

    public void UpdateHighlight(int index, InventoryItem item)
    {
        for (int i = 0; i < _display.slots.Count; i++)
        {
            bool isSelected = (i == index - 1);
            _display.slots[i].SetHighlight(isSelected);
        }
    }
}