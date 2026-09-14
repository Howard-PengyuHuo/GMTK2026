using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class WateringAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Graphic visual;
    [SerializeField] private string waterState = "Water";

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (visual == null) visual = GetComponent<Graphic>();
        SetVisible(false);
    }

    public void PlayWatering()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[WateringAnimation] Animator or controller is missing.");
            return;
        }

        int stateHash = Animator.StringToHash(waterState);
        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning($"[WateringAnimation] State '{waterState}' is missing.");
            return;
        }

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        SetVisible(true);
        animator.Play(stateHash, 0, 0f);
        animator.Update(0f);
        hideRoutine = StartCoroutine(HideWhenFinished());
    }

    private IEnumerator HideWhenFinished()
    {
        yield return null;
        float duration = Mathf.Max(0.05f, animator.GetCurrentAnimatorStateInfo(0).length);
        yield return new WaitForSeconds(duration);
        SetVisible(false);
        hideRoutine = null;
    }

    private void SetVisible(bool visible)
    {
        if (visual != null) visual.enabled = visible;
    }
}
