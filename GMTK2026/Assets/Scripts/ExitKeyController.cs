using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ExitKeyController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ItemData exitKeyItem;
    [SerializeField] private Graphic keyGraphic;
    public bool IsAvailable { get; private set; }
    public bool HasBeenCollected { get; private set; }

    private void Awake()
    {
        if(keyGraphic ==null)
            keyGraphic=GetComponent<Graphic>(); 
        
        SetVisible(false);
    }

    public void ShowKey()
    {
        if(HasBeenCollected)return; 
        IsAvailable = true; 
        SetVisible(true);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left ||!IsAvailable || HasBeenCollected || InventoryManager.Instance ==null)return;
        if(!InventoryManager.Instance.AddItem(exitKeyItem)) return;
        HasBeenCollected=true; IsAvailable=false; SetVisible(false);
    }

    private void SetVisible(bool value)
    {
        if (keyGraphic != null)
        {
            keyGraphic.enabled=value;
            keyGraphic.raycastTarget=value;
        }else gameObject.SetActive(value);
    }
}
