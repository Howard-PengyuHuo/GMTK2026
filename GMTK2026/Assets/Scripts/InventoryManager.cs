using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    private readonly List<ItemData> items = new();
    public IReadOnlyList<ItemData> Items => items;
    public ItemData SelectedItem { get; private set; }
    public event Action InventoryChanged;
    public event Action<ItemData> SelectedItemChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    
    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Inventory] Missing ItemData reference."); 
            return false;
        }
        
        if (items.Contains(item)) return false;
        items.Add(item); InventoryChanged?.Invoke(); 
        return true;
    }
    public bool HasItem(ItemData item) => item != null && items.Contains(item);
    public bool IsSelected(ItemData item) => item != null && SelectedItem == item;
    public void SelectItem(ItemData item)
    {
        if (!HasItem(item)) return;
        
        if (SelectedItem == item)
        {
            DeselectItem(); 
            return;
        }
        
        SelectedItem = item; SelectedItemChanged?.Invoke(item);
    }
    public void DeselectItem()
    {
        if (SelectedItem == null) return;
        SelectedItem = null; 
        SelectedItemChanged?.Invoke(null);
    }
    public bool TryUseSelectedItem(ItemData requiredItem, bool deselectRetainedItem = true)
    {
        if (!IsSelected(requiredItem)) return false;
        
        if (requiredItem.ConsumedOnUse) RemoveItem(requiredItem);
        else if (deselectRetainedItem) DeselectItem();
        
        return true;
    }
    public bool RemoveItem(ItemData item)
    {
        if (item == null || !items.Remove(item)) return false;
        if (SelectedItem == item)
        {
            SelectedItem = null; 
            SelectedItemChanged?.Invoke(null);
        }
        
        InventoryChanged?.Invoke(); return true;
    }
    public void ClearInventory()
    {
        items.Clear(); 
        SelectedItem = null; 
        InventoryChanged?.Invoke(); 
        SelectedItemChanged?.Invoke(null);
    }
}
