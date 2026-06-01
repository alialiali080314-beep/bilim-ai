using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ShopItem
{
    public string name;
    public int price;
    public Sprite icon;
    public bool isArmor;
    public int armorAmount = 100;
    public int weaponSlot = -1;           // -1 = not a weapon
    public WeaponBase weaponPrefab;
}

public class BuyMenu : MonoBehaviour
{
    [SerializeField] private ShopItem[] items;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private int startingMoney = 800;

    private int money;
    private PlayerHealth playerHealth;
    private WeaponManager weaponManager;

    public int Money => money;

    private void Start()
    {
        money = startingMoney;
        var player = GameObject.FindGameObjectWithTag("Player");
        playerHealth  = player?.GetComponent<PlayerHealth>();
        weaponManager = player?.GetComponentInChildren<WeaponManager>();

        BuildGrid();
        UpdateMoney();
    }

    private void BuildGrid()
    {
        foreach (Transform c in itemGrid) Destroy(c.gameObject);

        foreach (var item in items)
        {
            var btn = Instantiate(itemButtonPrefab, itemGrid);

            var labels = btn.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length > 0) labels[0].text = item.name;
            if (labels.Length > 1) labels[1].text = $"${item.price}";

            var img = btn.GetComponentInChildren<Image>();
            if (img != null && item.icon != null) img.sprite = item.icon;

            var captured = item;
            btn.GetComponent<Button>()?.onClick.AddListener(() => Buy(captured));
        }
    }

    private void Buy(ShopItem item)
    {
        if (money < item.price) { Debug.Log("Недостаточно денег"); return; }

        money -= item.price;
        UpdateMoney();

        if (item.isArmor)
            playerHealth?.AddArmor(item.armorAmount);
        else if (item.weaponPrefab != null && item.weaponSlot >= 0)
            weaponManager?.GiveWeapon(item.weaponSlot, item.weaponPrefab);
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoney();
    }

    private void UpdateMoney()
    {
        if (moneyText) moneyText.text = $"${money}";
    }
}
