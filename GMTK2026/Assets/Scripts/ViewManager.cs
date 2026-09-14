using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ViewManager : MonoBehaviour
{
    [SerializeField] private List<PuzzleView> views = new();
    [SerializeField] private Button backButton;
    public PuzzleViewId CurrentView { get; private set; }=PuzzleViewId.Room;

    private void Start()
    {
        if(backButton !=null) backButton.onClick.AddListener(BackToRoom);
        OpenView(PuzzleViewId.Room);
    }

    private void OnDestroy()
    {
        if(backButton !=null) backButton.onClick.RemoveListener(BackToRoom);
    }

    public void OpenView(PuzzleViewId id)
    {
        CurrentView = id;
        foreach(var view in views)
            if(view !=null)
                view.SetVisible(view.ViewId == id);
        
        if(backButton !=null)
            backButton.gameObject.SetActive(id != PuzzleViewId.Room);
    }
    public void BackToRoom()=>OpenView(PuzzleViewId.Room);
}
