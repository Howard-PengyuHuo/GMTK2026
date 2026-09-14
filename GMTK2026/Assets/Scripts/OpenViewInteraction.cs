using UnityEngine;
using UnityEngine.EventSystems;

public sealed class OpenViewInteraction : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private PuzzleViewId targetView;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button==PointerEventData.InputButton.Left) viewManager?.OpenView(targetView);
    }
}
