using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Color normalTint = Color.white;
    [SerializeField] private Color selectedTint = new(1f, .75f, .1f, 1f);
    private ItemData itemData;
    public ItemData ItemData => itemData;
    public void Setup(ItemData item)
    {
        itemData = item;
        if (itemIcon != null)
        {
            itemIcon.sprite = item.Icon; 
            itemIcon.enabled = item.Icon != null; 
            itemIcon.raycastTarget = false;
        }

        if (itemNameText != null)
        {
            itemNameText.text = item.DisplayName; 
            itemNameText.raycastTarget = false;
        }
        
        SetSelected(false);
    }

    public void OnClicked()
    {
        if (itemData != null && InventoryManager.Instance != null) InventoryManager.Instance.SelectItem(itemData);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) OnClicked();
    }
    public void SetSelected(bool value)
    {
        if (selectedFrame != null)
        {
            selectedFrame.enabled = value; 
            selectedFrame.color = value ? selectedTint : normalTint; 
            selectedFrame.raycastTarget = false;
        }
    }
}
