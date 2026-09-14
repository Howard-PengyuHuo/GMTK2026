using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Puzzle Game/Item Data")]
public sealed class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [TextArea, SerializeField] private string description;
    [SerializeField] private bool consumedOnUse;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;
    public bool ConsumedOnUse => consumedOnUse;
}
