using UnityEngine;
using UnityEngine.EventSystems;

public enum SpecimenType { Sprout, Leaves, Bud }

public sealed class SpecimenFrame : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpecimenType specimenType;
    [SerializeField] private RectTransform visualRoot;
    public SpecimenType Type => specimenType;
    public int CurrentPositionIndex { get; private set; }
    private SpecimenManager manager;
    public void Initialize(SpecimenManager owner, int index, RectTransform position) { manager = owner; SetPosition(index, position); }
    public void SetPosition(int index, RectTransform position)
    {
        CurrentPositionIndex = index;
        RectTransform target = visualRoot != null ? visualRoot : transform as RectTransform;
        if (target != null && position != null) target.anchoredPosition = position.anchoredPosition;
    }
    public void OnPointerClick(PointerEventData eventData) { if (eventData.button == PointerEventData.InputButton.Left) manager?.SelectFrame(this); }
}
