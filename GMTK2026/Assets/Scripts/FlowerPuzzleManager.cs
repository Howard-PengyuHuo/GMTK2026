using UnityEngine;

public sealed class FlowerPuzzleManager : MonoBehaviour
{
    public static FlowerPuzzleManager Instance { get; private set; }
    [SerializeField] private FlowerPot flowerPot;
    [SerializeField] private ClockController clockController;
    [SerializeField] private SpecimenManager specimenManager;
    [SerializeField] private ExitKeyController exitKeyController;
    
    public bool HasBeenWatered { get; private set; }
    public bool HasBloomed { get; private set; }
    public bool HasExperiencedFirstReset { get; private set; }
    
    private bool waitingForBloom;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        } 
        
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    public bool WaterPlant()
    {
        if (HasBloomed || HasBeenWatered || flowerPot == null 
            || flowerPot.IsAnimating || flowerPot.CurrentStage != PlantStage.Soil) return false;
        
        if (!flowerPot.GrowTo(PlantStage.Sprout)) return false;
        HasBeenWatered = true; 
        return true;
    }
    public bool CanAdvanceClock(int requestedIndex)
    {
        if (!HasBeenWatered || HasBloomed || waitingForBloom || flowerPot == null || flowerPot.IsAnimating) return false;
        return requestedIndex == clockController.CurrentStageIndex + 1;
    }
    public bool OnClockStageChanged(int index)
    {
        if (!CanAdvanceClock(index)) return false;
        
        if (index >= 1 && index <= 3) return flowerPot.GrowTo((PlantStage)(index + 1));
        
        if (index == 4)
        {
            bool succeeds = specimenManager != null && specimenManager.IsCorrectUnbinderOrder();
            ReachMidnight();
            return succeeds;
        }
        return false;
    }
    private void ReachMidnight()
    {
        if (specimenManager != null && specimenManager.IsCorrectUnbinderOrder())
        {
            waitingForBloom = flowerPot.GrowTo(PlantStage.Bloom);
            HasBloomed = waitingForBloom;
        }
        else ResetPlantCycle();
    }
    private void ResetPlantCycle()
    {
        HasExperiencedFirstReset = true; 
        HasBeenWatered = false; 
        waitingForBloom = false;
        flowerPot.ResetToSoil(); 
        clockController.ResetToStartTime();
    }
    public void OnBloomAnimationFinished()
    {
        if (!waitingForBloom || flowerPot.CurrentStage != PlantStage.Bloom) return;
        waitingForBloom = false; 
        exitKeyController?.ShowKey();
    }
}
