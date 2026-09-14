using UnityEngine;
using UnityEngine.UI;

public sealed class RoomViewSynchronizer : MonoBehaviour
{
    [Header("State Sources")]
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private ClockController closeUpClock;
    [SerializeField] private SpecimenManager specimenManager;
    [SerializeField] private FlowerPot closeUpFlower;

    [Header("Room Clock")]
    [SerializeField] private RectTransform roomClockHand;

    [Header("Room Specimens - fixed display slots 0, 1, 2")]
    [SerializeField] private RawImage[] roomSpecimenSlots;
    [SerializeField] private Texture sproutSpecimenTexture;
    [SerializeField] private Texture leavesSpecimenTexture;
    [SerializeField] private Texture budSpecimenTexture;

    [Header("Room Flower")]
    [SerializeField] private Animator roomPlantAnimator;
    [SerializeField] private RawImage roomPlantGraphic;
    [SerializeField] private GameObject[] unusedRoomStageVisuals;

    private PlantStage presentedStage = PlantStage.Soil;
    private bool subscribed;

    private void Start()
    {
        Subscribe();
        if (viewManager != null && viewManager.CurrentView == PuzzleViewId.Room)
            SynchronizeRoom();
    }

    private void OnEnable() => Subscribe();
    private void OnDisable()
    {
        if (!subscribed || viewManager == null) return;
        viewManager.ViewChanged -= OnViewChanged;
        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed || viewManager == null) return;
        viewManager.ViewChanged += OnViewChanged;
        subscribed = true;
    }

    private void OnViewChanged(PuzzleViewId view)
    {
        if (view == PuzzleViewId.Room) SynchronizeRoom();
    }

    public void SynchronizeRoom()
    {
        SynchronizeClock();
        SynchronizeSpecimens();
        SynchronizeFlower();
    }

    private void SynchronizeClock()
    {
        if (roomClockHand == null || closeUpClock == null) return;
        roomClockHand.gameObject.SetActive(closeUpClock.IsInstalled);
        if (closeUpClock.IsInstalled)
            roomClockHand.localEulerAngles = new Vector3(0f, 0f, closeUpClock.CurrentHandAngle);
    }

    private void SynchronizeSpecimens()
    {
        if (specimenManager == null || roomSpecimenSlots == null) return;
        for (int i = 0; i < Mathf.Min(3, roomSpecimenSlots.Length); i++)
        {
            if (roomSpecimenSlots[i] == null) continue;
            roomSpecimenSlots[i].texture = TextureFor(specimenManager.GetTypeAtPosition(i));
        }
    }

    private Texture TextureFor(SpecimenType type)
    {
        return type switch
        {
            SpecimenType.Leaves => leavesSpecimenTexture,
            SpecimenType.Bud => budSpecimenTexture,
            _ => sproutSpecimenTexture
        };
    }

    private void SynchronizeFlower()
    {
        if (closeUpFlower == null || roomPlantGraphic == null) return;
        foreach (GameObject visual in unusedRoomStageVisuals)
            if (visual != null) visual.SetActive(false);

        PlantStage stage = closeUpFlower.CurrentStage;
        if (stage == PlantStage.Soil)
        {
            roomPlantGraphic.enabled = false;
            presentedStage = PlantStage.Soil;
            return;
        }

        roomPlantGraphic.enabled = true;
        if (roomPlantAnimator != null && presentedStage != stage)
        {
            string state = StateFor(stage);
            int hash = Animator.StringToHash(state);
            if (roomPlantAnimator.HasState(0, hash))
            {
                roomPlantAnimator.Play(hash, 0, 0f);
                roomPlantAnimator.Update(0f);
            }
            else Debug.LogWarning($"[RoomSync] Room plant state '{state}' is missing.");
        }

        presentedStage = stage;
    }

    private static string StateFor(PlantStage stage) => stage switch
    {
        PlantStage.Sprout => "GrowToSprout",
        PlantStage.Leaves => "GrowToLeaves",
        PlantStage.Bud => "GrowToBud",
        PlantStage.AlmostBloom => "GrowToAlmostBloom",
        PlantStage.Bloom => "GrowToBloom",
        _ => "Soil"
    };

    // The shared clips contain these events; the room presentation does not own puzzle state.
    public void OnGrowthAnimationFinished() { }
    public void OnBloomAnimationFinished() { }
}
