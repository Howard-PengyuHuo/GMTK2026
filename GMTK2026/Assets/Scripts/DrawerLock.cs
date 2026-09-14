using UnityEngine;

public sealed class DrawerLock : MonoBehaviour
{
    [SerializeField] private NumberDial firstDial;
    [SerializeField] private NumberDial secondDial;
    [SerializeField] private NumberDial thirdDial;
    [SerializeField] private int correctFirstValue = 2;
    [SerializeField] private int correctSecondValue = 4;
    [SerializeField] private int correctThirdValue = 5;
    [SerializeField] private ItemPickup wateringCanPickup;
    [SerializeField] private ItemPickup clockHandPickup;
    [SerializeField] private GameObject closedDrawerObject;
    [SerializeField] private GameObject openedDrawerObject;
    
    public bool IsOpened { get; private set; }
    private void Awake()
    {
        SetDrawerVisual(IsOpened);
        if (!IsOpened)
        {
            if (wateringCanPickup != null) wateringCanPickup.gameObject.SetActive(false);
            if (clockHandPickup != null) clockHandPickup.gameObject.SetActive(false);
        }
    }
    public void TryOpenDrawer()
    {
        if (IsOpened || firstDial == null || secondDial == null || thirdDial == null) return;
        if (firstDial.CurrentValue != correctFirstValue || secondDial.CurrentValue != correctSecondValue ||
            thirdDial.CurrentValue != correctThirdValue)
        {
            Debug.Log("[Drawer] Combination is incorrect."); 
            return;
        }
        IsOpened = true; 
        SetDrawerVisual(true);
        if (wateringCanPickup != null)
        {
            wateringCanPickup.gameObject.SetActive(true); 
            wateringCanPickup.SetCanPickup(true);
        }

        if (clockHandPickup != null)
        {
            clockHandPickup.gameObject.SetActive(true); 
            clockHandPickup.SetCanPickup(true);
        }
    }

    private void SetDrawerVisual(bool open)
    {
        if (closedDrawerObject != null) closedDrawerObject.SetActive(!open); 
        if (openedDrawerObject != null) openedDrawerObject.SetActive(open);
    }
}
