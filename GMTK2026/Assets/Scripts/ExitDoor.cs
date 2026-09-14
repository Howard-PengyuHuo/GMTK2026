using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class ExitDoor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ItemData exitKeyItem;
    [SerializeField] private GameObject closedDoorObject;
    [SerializeField] private GameObject openedDoorObject;
    [SerializeField] private string nextSceneName;
    public bool IsOpen { get; private set; }
    public bool PuzzleCompleted { get; private set; }
    private void Start()=>SetVisual(false);
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button!=PointerEventData.InputButton.Left)return;
        if(IsOpen){EnterDoor();return;}
        if(InventoryManager.Instance==null||!InventoryManager.Instance.TryUseSelectedItem(exitKeyItem))return;
        IsOpen = true;
        SetVisual(true);
    }

    private void EnterDoor()
    {
        PuzzleCompleted = true;
        if(!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else Debug.Log("[FlowerPuzzle] Puzzle completed.");
    }

    private void SetVisual(bool opened)
    {
        if(closedDoorObject!=null) closedDoorObject.SetActive(!opened);
        if(openedDoorObject!=null) openedDoorObject.SetActive(opened);
    }
}
