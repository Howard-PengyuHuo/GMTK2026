using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;
    private readonly List<InventorySlotUI> slots = new();
    private InventoryManager subscribed;

    private void OnEnable() => TrySubscribe();
    private void Start() => TrySubscribe();
    private void Update() { if (subscribed == null) TrySubscribe(); }
    private void OnDisable() => Unsubscribe();
    private void TrySubscribe()
    {
        if (subscribed != null || InventoryManager.Instance == null) return;
        subscribed = InventoryManager.Instance;
        subscribed.InventoryChanged += Refresh;
        subscribed.SelectedItemChanged += UpdateSelection;
        Refresh();
    }
    private void Unsubscribe()
    {
        if (subscribed == null) return;
        subscribed.InventoryChanged -= Refresh;
        subscribed.SelectedItemChanged -= UpdateSelection;
        subscribed = null;
    }
    private void Refresh()
    {
        foreach (var slot in slots) if (slot != null) Destroy(slot.gameObject);
        slots.Clear();
        if (slotContainer == null || subscribed == null) return;
        foreach (var item in subscribed.Items)
        {
            InventorySlotUI slot = slotPrefab != null ? Instantiate(slotPrefab, slotContainer) : CreateFallbackSlot(slotContainer);
            slot.Setup(item); slots.Add(slot);
        }
        UpdateSelection(subscribed.SelectedItem);
    }
    private static InventorySlotUI CreateFallbackSlot(Transform parent)
    {
        var root = new GameObject("InventorySlot", typeof(RectTransform), typeof(Image), typeof(Button), typeof(InventorySlotUI));
        root.transform.SetParent(parent, false);
        ((RectTransform)root.transform).sizeDelta = new Vector2(150, 92);
        root.GetComponent<Image>().color = new Color(.12f, .1f, .08f, .92f);
        
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image)); 
        iconGo.transform.SetParent(root.transform, false);
        
        var icon = iconGo.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        ((RectTransform)iconGo.transform).anchorMin = new Vector2(0, .25f); 
        ((RectTransform)iconGo.transform).anchorMax = new Vector2(1, 1);
        ((RectTransform)iconGo.transform).offsetMin = Vector2.zero; 
        ((RectTransform)iconGo.transform).offsetMax = Vector2.zero;
        
        var textGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI)); 
        textGo.transform.SetParent(root.transform, false);
        
        var text = textGo.GetComponent<TextMeshProUGUI>(); 
        text.alignment = TextAlignmentOptions.Center; 
        text.fontSize = 18; 
        text.raycastTarget = false;
        ((RectTransform)textGo.transform).anchorMin = Vector2.zero; ((RectTransform)textGo.transform).anchorMax = new Vector2(1, .28f);
        ((RectTransform)textGo.transform).offsetMin = Vector2.zero; ((RectTransform)textGo.transform).offsetMax = Vector2.zero;
        
        var slot = root.GetComponent<InventorySlotUI>();
        typeof(InventorySlotUI).GetField("itemIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(slot, icon);
        typeof(InventorySlotUI).GetField("itemNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(slot, text);
        return slot;
    }

    private void UpdateSelection(ItemData selected)
    {
        foreach (var slot in slots) 
            slot.SetSelected(slot.ItemData == selected);
    }
}
