using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Health & Armor")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider armorSlider;
    [SerializeField] private TextMeshProUGUI armorText;

    [Header("Weapon")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image reloadBar;

    [Header("Crosshair")]
    [SerializeField] private RectTransform crossTop;
    [SerializeField] private RectTransform crossBottom;
    [SerializeField] private RectTransform crossLeft;
    [SerializeField] private RectTransform crossRight;
    [SerializeField] private float baseSpread = 8f;
    [SerializeField] private float dynamicSpread = 0f;

    [Header("Round Info")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Round End")]
    [SerializeField] private GameObject roundEndPanel;
    [SerializeField] private TextMeshProUGUI roundEndText;

    [Header("Damage")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float flashDuration = 0.35f;

    [Header("Hit Marker")]
    [SerializeField] private GameObject hitMarker;
    [SerializeField] private float hitMarkerTime = 0.12f;

    [Header("Kill Feed")]
    [SerializeField] private Transform killFeedRoot;
    [SerializeField] private GameObject killFeedEntryPrefab;

    [Header("Phase Banner")]
    [SerializeField] private TextMeshProUGUI phaseBanner;

    private float flashTimer;
    private float hitTimer;

    private void Start()
    {
        WireEvents();
        if (roundEndPanel) roundEndPanel.SetActive(false);
        if (hitMarker)     hitMarker.SetActive(false);
    }

    private void WireEvents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.OnHealthChanged.AddListener(SetHealth);
            hp.OnArmorChanged.AddListener(SetArmor);
            hp.OnDamageTaken.AddListener(FlashDamage);
        }

        var wm = player.GetComponentInChildren<WeaponManager>();
        if (wm != null)
        {
            wm.OnAmmoChanged   += SetAmmo;
            wm.OnWeaponChanged += SetWeapon;
        }

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnTimerTick    += SetTimer;
            gm.OnScoreUpdated += SetScore;
            gm.OnStateChanged += HandleState;
            gm.OnRoundStarted += n => { if (roundText) roundText.text = $"Round {n}"; };
        }
    }

    private void Update()
    {
        UpdateCrosshair();

        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (damageOverlay)
            {
                var c = damageOverlay.color;
                c.a = Mathf.Clamp01(flashTimer / flashDuration) * 0.55f;
                damageOverlay.color = c;
            }
        }

        if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0 && hitMarker) hitMarker.SetActive(false);
        }
    }

    private void UpdateCrosshair()
    {
        dynamicSpread = Mathf.Lerp(dynamicSpread, 0f, 8f * Time.deltaTime);
        float s = baseSpread + dynamicSpread;
        if (crossTop)    crossTop.anchoredPosition    = new Vector2(0, s);
        if (crossBottom) crossBottom.anchoredPosition = new Vector2(0, -s);
        if (crossLeft)   crossLeft.anchoredPosition   = new Vector2(-s, 0);
        if (crossRight)  crossRight.anchoredPosition  = new Vector2(s, 0);
    }

    public void SpreadCrosshair(float amount) => dynamicSpread += amount;

    public void SetHealth(int cur, int max)
    {
        if (healthSlider) healthSlider.value = (float)cur / max;
        if (healthText)   healthText.text    = cur.ToString();
    }

    public void SetArmor(int cur)
    {
        if (armorSlider) armorSlider.value = cur / 100f;
        if (armorText)   armorText.text    = cur.ToString();
    }

    public void SetAmmo(int cur, int res)
    {
        if (ammoText) ammoText.text = $"{cur} / {res}";
    }

    public void SetWeapon(WeaponBase w)
    {
        if (weaponNameText) weaponNameText.text = w.weaponData.weaponName;
    }

    public void SetTimer(float seconds)
    {
        if (!timerText) return;
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        timerText.text = $"{m:00}:{s:00}";
    }

    public void SetScore(int p, int e)
    {
        if (scoreText) scoreText.text = $"{p}  -  {e}";
    }

    public void FlashDamage(float intensity) => flashTimer = flashDuration;

    public void ShowHitMarker()
    {
        if (!hitMarker) return;
        hitMarker.SetActive(true);
        hitTimer = hitMarkerTime;
    }

    public void AddKillFeedEntry(string killer, string victim)
    {
        if (!killFeedRoot || !killFeedEntryPrefab) return;
        var entry = Instantiate(killFeedEntryPrefab, killFeedRoot);
        var t = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (t) t.text = $"{killer}  ›  {victim}";
        Destroy(entry, 4f);
    }

    private void HandleState(GameState st)
    {
        bool showEnd = st == GameState.RoundEnd || st == GameState.MatchEnd;
        if (roundEndPanel) roundEndPanel.SetActive(showEnd);

        if (phaseBanner)
        {
            phaseBanner.text = st switch
            {
                GameState.Warmup   => "WARMUP",
                GameState.BuyPhase => "BUY PHASE  [B]",
                GameState.LivePhase => "",
                GameState.RoundEnd  => "",
                GameState.MatchEnd  => "",
                _ => ""
            };
        }

        if (st == GameState.RoundEnd && roundEndText && GameManager.Instance != null)
        {
            bool won = GameManager.Instance.PlayerScore >= GameManager.Instance.EnemyScore;
            roundEndText.text = won ? "ROUND WON" : "ROUND LOST";
        }

        if (st == GameState.MatchEnd && roundEndText && GameManager.Instance != null)
        {
            bool won = GameManager.Instance.PlayerScore > GameManager.Instance.EnemyScore;
            roundEndText.text = won ? "VICTORY!" : "DEFEAT";
        }
    }
}
