using UnityEngine;

public class ItemButton : MonoBehaviour
{
    public Sprite itemIcon;
    public string itemName;
    [TextArea] public string itemDescription;
    public int itemIndex;

    public ItemInfoDisplay infoDisplay;

    public void OnClick()
    {
        infoDisplay.ShowItemInfo(itemIcon, itemName, itemDescription, itemIndex);
    }
}
