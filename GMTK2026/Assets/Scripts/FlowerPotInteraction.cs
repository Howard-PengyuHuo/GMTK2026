using UnityEngine;
using UnityEngine.EventSystems;

public sealed class FlowerPotInteraction : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ItemData wateringCanItem;
    [SerializeField] private WateringAnimationPlayer wateringAnimation;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || InventoryManager.Instance == null || FlowerPuzzleManager.Instance == null) return;
        
        if (!InventoryManager.Instance.IsSelected(wateringCanItem))
        {
            Debug.Log("[FlowerPot] The soil is dry."); 
            return;
        }
        
        if (FlowerPuzzleManager.Instance.WaterPlant())
        {
            wateringAnimation?.PlayWatering();
            InventoryManager.Instance.TryUseSelectedItem(wateringCanItem, true);
        }
    }
}
