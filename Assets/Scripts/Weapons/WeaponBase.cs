using System.Collections;
using UnityEngine;

public enum WeaponType { Rifle, Pistol, Sniper, Shotgun, Knife }

[System.Serializable]
public class WeaponData
{
    public string weaponName = "Weapon";
    public WeaponType weaponType = WeaponType.Rifle;
    public int damage = 25;
    public float fireRate = 600f;        // rounds/min
    public int magazineSize = 30;
    public int totalAmmo = 90;
    public float reloadTime = 2.5f;
    public float hipSpread = 2f;         // degrees
    public float adsSpread = 0.4f;
    public float range = 120f;
    public Vector2 recoilAmount = new Vector2(1.5f, 0.3f);
    public int price = 2700;
    public bool isAutomatic = true;
    public int pelletsPerShot = 1;       // shotgun = 8+
}

[RequireComponent(typeof(AudioSource))]
public class WeaponBase : MonoBehaviour
{
    [Header("Data")]
    public WeaponData weaponData;

    [Header("Visual")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private TrailRenderer bulletTrailPrefab;

    [Header("ADS")]
    [SerializeField] private Vector3 adsPosition = new Vector3(0, -0.05f, 0.1f);
    [SerializeField] private float adsSpeed = 15f;
    [SerializeField] private float adsFOV = 45f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private AudioClip drawSound;

    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;
    private bool isADS;
    private float nextFireTime;
    private Camera mainCamera;
    private PlayerCamera playerCamera;
    private AudioSource audioSource;
    private Vector3 defaultPosition;

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
    public bool IsADS => isADS;

    public event System.Action<int, int> OnAmmoChanged;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerCamera = mainCamera?.GetComponent<PlayerCamera>();
        audioSource = GetComponent<AudioSource>();
        defaultPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        currentAmmo = weaponData.magazineSize;
        reserveAmmo = weaponData.totalAmmo;
        isReloading = false;
        isADS = false;
        transform.localPosition = defaultPosition;

        if (drawSound) audioSource.PlayOneShot(drawSound);
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }

    private void Update()
    {
        HandleADS();
        HandleFire();
        HandleReload();
    }

    private void HandleADS()
    {
        isADS = Input.GetMouseButton(1) && !isReloading;

        Vector3 target = isADS ? adsPosition : defaultPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, adsSpeed * Time.deltaTime);

        if (mainCamera != null)
        {
            float targetFOV = isADS ? adsFOV : 60f;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, adsSpeed * Time.deltaTime);
        }

        playerCamera?.SetADS(isADS);
    }

    private void HandleFire()
    {
        if (isReloading) return;

        bool fireInput = weaponData.isAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (!fireInput || Time.time < nextFireTime) return;

        if (currentAmmo > 0)
        {
            Fire();
            nextFireTime = Time.time + 60f / weaponData.fireRate;
        }
        else
        {
            if (emptySound) audioSource.PlayOneShot(emptySound);
            if (reserveAmmo > 0) StartReload();
        }
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading
            && currentAmmo < weaponData.magazineSize && reserveAmmo > 0)
            StartReload();
    }

    private void Fire()
    {
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);

        muzzleFlash?.Play();
        if (shootSound) audioSource.PlayOneShot(shootSound);
        playerCamera?.AddRecoil(weaponData.recoilAmount.x, weaponData.recoilAmount.y);

        for (int i = 0; i < weaponData.pelletsPerShot; i++)
            CastBullet();
    }

    private void CastBullet()
    {
        float spread = isADS ? weaponData.adsSpread : weaponData.hipSpread;
        Vector3 dir = mainCamera.transform.forward
            + mainCamera.transform.right   * Random.Range(-spread, spread) * 0.01f
            + mainCamera.transform.up      * Random.Range(-spread, spread) * 0.01f;
        dir.Normalize();

        Ray ray = new Ray(mainCamera.transform.position, dir);
        Vector3 endPoint = ray.origin + ray.direction * weaponData.range;

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
        {
            endPoint = hit.point;
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(weaponData.damage, dir);

            if (bulletHolePrefab && !hit.collider.CompareTag("Enemy"))
            {
                var hole = Instantiate(bulletHolePrefab,
                    hit.point + hit.normal * 0.002f,
                    Quaternion.LookRotation(hit.normal));
                Destroy(hole, 10f);
            }
        }

        if (bulletTrailPrefab && muzzlePoint)
            StartCoroutine(SpawnTrail(muzzlePoint.position, endPoint));
    }

    private IEnumerator SpawnTrail(Vector3 start, Vector3 end)
    {
        var trail = Instantiate(bulletTrailPrefab, start, Quaternion.identity);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.08f;
            trail.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        Destroy(trail.gameObject, trail.time);
    }

    private void StartReload()
    {
        if (!isReloading) StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        if (reloadSound) audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = weaponData.magazineSize - currentAmmo;
        int fill = Mathf.Min(needed, reserveAmmo);
        currentAmmo += fill;
        reserveAmmo -= fill;

        isReloading = false;
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }

    public void AddAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(reserveAmmo + amount, weaponData.totalAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
}
