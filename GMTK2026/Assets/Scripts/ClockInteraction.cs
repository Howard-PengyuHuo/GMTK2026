using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class  ClockInteraction : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ItemData clockHandItem;
    [SerializeField] private ClockController clockController;
    [SerializeField] private Graphic installGraphic;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || clockController == null || clockController.IsInstalled || InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.IsSelected(clockHandItem)) return;
        if (!InventoryManager.Instance.TryUseSelectedItem(clockHandItem)) return;
        
        clockController.InstallClockHand();
        if (installGraphic != null) installGraphic.raycastTarget = false;
        enabled = false;
    }
}
