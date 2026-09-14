using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public sealed class ItemPickup : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private Graphic interactionGraphic;
    [SerializeField] private bool canPickup = true;
    [SerializeField] private bool hideAfterPickup = true;
    public bool HasBeenPickedUp { get; private set; }

    private void Awake()
    {
        if (interactionGraphic == null) interactionGraphic = GetComponent<Graphic>(); 
        if (interactionGraphic != null) interactionGraphic.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) TryPickup();
    }
    public bool TryPickup()
    {
        if (!canPickup || HasBeenPickedUp || itemData == null || InventoryManager.Instance == null) return false;
        if (!InventoryManager.Instance.AddItem(itemData)) return false;
        
        HasBeenPickedUp = true; 
        canPickup = false;
        
        if (interactionGraphic != null) interactionGraphic.raycastTarget = false;
        if (hideAfterPickup) gameObject.SetActive(false);
        return true;
    }

    public void SetCanPickup(bool value)
    {
        if (HasBeenPickedUp) return; 
        
        canPickup = value; 
        if (interactionGraphic != null) interactionGraphic.raycastTarget = value;
    }
}
