using UnityEngine;
using UnityEngine.UI;

public enum PlantStage { Soil, Sprout, Leaves, Bud, AlmostBloom, Bloom }

public sealed class FlowerPot : MonoBehaviour
{
    [SerializeField] private Animator plantAnimator;
    [SerializeField] private Graphic plantGraphic;
    [SerializeField] private string soilState = "Soil";
    [SerializeField] private string growToSproutState = "GrowToSprout";
    [SerializeField] private string growToLeavesState = "GrowToLeaves";
    [SerializeField] private string growToBudState = "GrowToBud";
    [SerializeField] private string growToAlmostBloomState = "GrowToAlmostBloom";
    [SerializeField] private string growToBloomState = "GrowToBloom";
    public PlantStage CurrentStage { get; private set; } = PlantStage.Soil;
    public bool IsAnimating { get; private set; }

    private void Awake()
    {
        if (plantAnimator == null) plantAnimator = GetComponent<Animator>(); 
        ResetToSoil();
    }
    public bool GrowTo(PlantStage stage)
    {
        if (IsAnimating || (int)stage != (int)CurrentStage + 1 || stage == PlantStage.Soil) return false;
        CurrentStage = stage; 
        IsAnimating = true; 
        Play(StateFor(stage)); 
        return true;
    }

    public void ResetToSoil()
    {
        CurrentStage = PlantStage.Soil; 
        IsAnimating = false; 
        Play(soilState);
    }
    
    private string StateFor(PlantStage stage) => stage switch
    {
        PlantStage.Sprout => growToSproutState, 
        PlantStage.Leaves => growToLeavesState, 
        PlantStage.Bud => growToBudState, 
        PlantStage.AlmostBloom => growToAlmostBloomState, 
        PlantStage.Bloom => growToBloomState, 
        _ => soilState
    };
    
    private void Play(string state)
    {
        if (plantAnimator == null || plantAnimator.runtimeAnimatorController == null)
        {
            IsAnimating = false; 
            return;
        }
        int hash = Animator.StringToHash(state);
        if (!plantAnimator.HasState(0, hash))
        {
            Debug.LogError($"[FlowerPot] Animator state '{state}' is missing."); 
            IsAnimating = false; 
            return;
        }
        plantAnimator.Play(hash, 0, 0f); 
        plantAnimator.Update(0f);
    }
    public void OnGrowthAnimationFinished() => IsAnimating = false;
    public void OnBloomAnimationFinished()
    {
        IsAnimating = false;
        FlowerPuzzleManager.Instance?.OnBloomAnimationFinished();
    }
}
