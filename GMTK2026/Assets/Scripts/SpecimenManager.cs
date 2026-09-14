using UnityEngine;

public sealed class SpecimenManager : MonoBehaviour
{
    [SerializeField] private SpecimenFrame sproutFrame;
    [SerializeField] private SpecimenFrame leavesFrame;
    [SerializeField] private SpecimenFrame budFrame;
    [SerializeField] private RectTransform[] positions;
    [SerializeField] private GameObject selectionMarker;
    
    private SpecimenFrame selectedFrame;
    private bool initialized;
    private void Start() => InitializeIfNeeded();
    private void InitializeIfNeeded()
    {
        if (initialized || positions == null || positions.Length < 3 || sproutFrame == null || leavesFrame == null || budFrame == null) return;
        sproutFrame.Initialize(this,0,positions[0]);
        leavesFrame.Initialize(this,1,positions[1]); 
        budFrame.Initialize(this,2,positions[2]); 
        initialized=true; HideMarker();
    }
    public void SelectFrame(SpecimenFrame clicked)
    {
        InitializeIfNeeded(); if (clicked == null) return;
        
        if (selectedFrame == null)
        {
            selectedFrame=clicked; ShowMarker(clicked); return;
        }

        if (selectedFrame == clicked)
        {
            selectedFrame=null; HideMarker(); return;
        }
        
        int a = selectedFrame.CurrentPositionIndex, b = clicked.CurrentPositionIndex;
        selectedFrame.SetPosition(b,positions[b]); 
        clicked.SetPosition(a,positions[a]); 
        selectedFrame=null; HideMarker();
    }

    public bool IsCorrectUnbinderOrder()
    {
        InitializeIfNeeded(); return
            TypeAt(0) == SpecimenType.Bud && TypeAt(1) == SpecimenType.Sprout && TypeAt(2) == SpecimenType.Leaves;
    }

    private SpecimenType TypeAt(int index)
    {
        if(sproutFrame.CurrentPositionIndex == index) return SpecimenType.Sprout; 
        if(leavesFrame.CurrentPositionIndex == index) return SpecimenType.Leaves; 
        return SpecimenType.Bud;
    }

    private void ShowMarker(SpecimenFrame frame)
    {
        if(selectionMarker == null)return; 
        
        selectionMarker.SetActive(true); 
        var marker = selectionMarker.transform as RectTransform; 
        var source = frame.transform as RectTransform; 
        
        if(marker!=null&&source !=null) marker.anchoredPosition = source.anchoredPosition;
    }

    private void HideMarker()
    {
        if(selectionMarker !=null)selectionMarker.SetActive(false);
    }
}
