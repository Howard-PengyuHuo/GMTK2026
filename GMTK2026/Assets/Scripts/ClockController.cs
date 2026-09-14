using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ClockController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform clockCenter;
    [SerializeField] private RectTransform clockHand;
    [SerializeField] private Graphic dragGraphic;
    [SerializeField] private float[] snapAngles = { -30f, -48f, -66f, -84f, -90f };
    [SerializeField] private float spriteAngleOffset = -180f;
    public bool IsInstalled { get; private set; }
    public int CurrentStageIndex { get; private set; }
    private bool dragging;

    private void Awake()
    {
        if (dragGraphic == null) dragGraphic = GetComponent<Graphic>(); 
        SetInput(false); 
        SetHand(0);
    }

    public void InstallClockHand()
    {
        if (IsInstalled) return; 
        
        IsInstalled = true; 
        if (clockHand != null) clockHand.gameObject.SetActive(true); 
        SetInput(true); 
        ResetToStartTime();
    }
    public void ResetToStartTime() => SetHand(0);

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CanDrag()) dragging = true;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || clockCenter == null || clockHand == null) return;
        
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clockCenter, eventData.position, eventData.pressEventCamera, out Vector2 point)) return;
        float angle = Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg + spriteAngleOffset;
        clockHand.localEulerAngles = new Vector3(0, 0, angle);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return; dragging = false;
        int nearest = NearestIndex(Normalize(clockHand.localEulerAngles.z));
        if (FlowerPuzzleManager.Instance != null && FlowerPuzzleManager.Instance.OnClockStageChanged(nearest)) SetHand(nearest);
        else SetHand(CurrentStageIndex);
    }
    private bool CanDrag() => IsInstalled && (FlowerPuzzleManager.Instance == null || !FlowerPuzzleManager.Instance.HasBloomed) && (FlowerPuzzleManager.Instance == null || FlowerPuzzleManager.Instance.CanAdvanceClock(CurrentStageIndex + 1));

    private int NearestIndex(float angle)
    {
        int best = 0; 
        float delta = float.MaxValue;
        for (int i = 0; i < snapAngles.Length; i++)
        {
            float d=Mathf.Abs(Mathf.DeltaAngle(angle,snapAngles[i]));
            if (d < delta)
            {
                delta=d;best=i;
            }
        } return best;
    }

    private void SetHand(int index)
    {
        CurrentStageIndex = Mathf.Clamp(index, 0, snapAngles.Length-1); 
        if (clockHand != null) clockHand.localEulerAngles = new Vector3(0,0,snapAngles[CurrentStageIndex]);
    }

    private void SetInput(bool value)
    {
        if (dragGraphic != null) dragGraphic.raycastTarget = value; 
        if (!value && clockHand != null) clockHand.gameObject.SetActive(false);
    }
    private static float Normalize(float angle) => angle > 180f ? angle - 360f : angle;

    public bool TryAdvanceToIndexForTests(int index)
    {
        if (!IsInstalled || FlowerPuzzleManager.Instance == null || !FlowerPuzzleManager.Instance.OnClockStageChanged(index)) 
            return false; 
        
        SetHand(index); 
        return true;
    }
}
