using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // Slots: 0 = primary, 1 = pistol, 2 = knife
    [SerializeField] private WeaponBase[] weaponSlots = new WeaponBase[3];
    [SerializeField] private int startSlot = 2;

    private int currentSlot;
    private WeaponBase currentWeapon;

    public WeaponBase CurrentWeapon => currentWeapon;
    public int CurrentSlot => currentSlot;

    public event System.Action<int, int> OnAmmoChanged;
    public event System.Action<WeaponBase> OnWeaponChanged;

    private void Start()
    {
        foreach (var w in weaponSlots)
            if (w != null) w.gameObject.SetActive(false);

        Equip(startSlot);
    }

    private void Update()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && weaponSlots[i] != null)
                Equip(i);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) CycleWeapon(1);
        else if (scroll < 0f) CycleWeapon(-1);
    }

    private void CycleWeapon(int dir)
    {
        int next = currentSlot;
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            next = (next + dir + weaponSlots.Length) % weaponSlots.Length;
            if (weaponSlots[next] != null) { Equip(next); return; }
        }
    }

    public void Equip(int slot)
    {
        if (slot < 0 || slot >= weaponSlots.Length || weaponSlots[slot] == null) return;

        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= PropagateAmmo;
            currentWeapon.gameObject.SetActive(false);
        }

        currentSlot = slot;
        currentWeapon = weaponSlots[slot];
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.OnAmmoChanged += PropagateAmmo;

        OnWeaponChanged?.Invoke(currentWeapon);
        OnAmmoChanged?.Invoke(currentWeapon.CurrentAmmo, currentWeapon.ReserveAmmo);
    }

    private void PropagateAmmo(int cur, int res) => OnAmmoChanged?.Invoke(cur, res);

    public bool HasWeaponInSlot(int slot) => slot >= 0 && slot < weaponSlots.Length && weaponSlots[slot] != null;

    public void GiveWeapon(int slot, WeaponBase weapon)
    {
        if (slot < 0 || slot >= weaponSlots.Length) return;
        if (weaponSlots[slot] != null) weaponSlots[slot].gameObject.SetActive(false);
        weaponSlots[slot] = weapon;
    }
}
