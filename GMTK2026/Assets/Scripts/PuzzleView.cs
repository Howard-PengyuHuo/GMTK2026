using UnityEngine;

public enum PuzzleViewId { Room, Drawer, Clock, Specimens, FlowerPot, Door }
public sealed class PuzzleView : MonoBehaviour
{
    [SerializeField] private PuzzleViewId viewId;
    [SerializeField] private CanvasGroup canvasGroup;
    public PuzzleViewId ViewId=>viewId;

    private void Awake()
    {
        if(canvasGroup==null)canvasGroup=GetComponent<CanvasGroup>();
    }
    public void SetVisible(bool value){if(canvasGroup==null)return;canvasGroup.alpha=value?1:0;canvasGroup.interactable=value;canvasGroup.blocksRaycasts=value;}
}
