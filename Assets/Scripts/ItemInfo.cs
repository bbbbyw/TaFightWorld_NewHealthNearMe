using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Button unlockButton;

    private int currentItemIndex;
    private bool[] unlockedItems;

    void Start()
    {
        infoPanel.SetActive(false);
        unlockedItems = new bool[100];
    }

    public void ShowItemInfo(Sprite icon, string name, string description, int index)
    {
        infoPanel.SetActive(true);
        itemIcon.sprite = icon;
        itemNameText.text = name;
        itemDescriptionText.text = description;

        currentItemIndex = index;

        bool isUnlocked = unlockedItems[index];
        unlockButton.gameObject.SetActive(!isUnlocked);
    }

    public void UnlockItem()
    {
        unlockedItems[currentItemIndex] = true;
        unlockButton.gameObject.SetActive(false);
    }


    public void HideItemInfo()
    {
        infoPanel.SetActive(false);
    }
}
