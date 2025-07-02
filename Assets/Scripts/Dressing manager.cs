using UnityEngine;

public class DressingManager : MonoBehaviour
{
    [Header("Target Sprite Renderers")]
    public SpriteRenderer dressSlot;
    public SpriteRenderer weaponSlot;
    public SpriteRenderer helmetSlot;

    [Header("Dress Options")]
    public Sprite[] dressSprites;
    public Sprite[] weaponSprites;
    public Sprite[] helmetSprites;

    [Header("UI Panels")]
    public GameObject helmetPanel;
    public GameObject weaponPanel;
    public GameObject dressPanel;

    void Start()
    {
        helmetPanel.SetActive(false);
        weaponPanel.SetActive(false);
        dressPanel.SetActive(false);
    }

    public void SetDress(int index)
    {
        if (index >= 0 && index < dressSprites.Length)
        {
            dressSlot.sprite = dressSprites[index];
        }
    }

    public void SetWeapon(int index)
    {
        if (index >= 0 && index < weaponSprites.Length)
        {
            weaponSlot.sprite = weaponSprites[index];
        }
    }

    public void SetHelmet(int index)
    {
        if (index >= 0 && index < helmetSprites.Length)
        {
            helmetSlot.sprite = helmetSprites[index];
        }
    }

    public void ShowHelmetPanel()
    {
        helmetPanel.SetActive(true);
        weaponPanel.SetActive(false);
        dressPanel.SetActive(false);
    }

    public void ShowWeaponPanel()
    {
        helmetPanel.SetActive(false);
        weaponPanel.SetActive(true);
        dressPanel.SetActive(false);
    }

    public void ShowDressPanel()
    {
        helmetPanel.SetActive(false);
        weaponPanel.SetActive(false);
        dressPanel.SetActive(true);
    }
    public void CloseHelmetPanel()
    {
        helmetPanel.SetActive(false);
    }

    public void CloseWeaponPanel()
    {
        weaponPanel.SetActive(false);
    }

    public void CloseDressPanel()
    {
        dressPanel.SetActive(false);
    }
}
