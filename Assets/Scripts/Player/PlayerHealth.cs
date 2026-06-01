using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxArmor = 100;
    [SerializeField] private float armorAbsorption = 0.5f;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent<int> OnArmorChanged;
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken;

    private int currentHealth;
    private int currentArmor;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int CurrentArmor => currentArmor;
    public bool IsDead => isDead;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        Respawn();
    }

    public void TakeDamage(int damage, Vector3 direction)
    {
        if (isDead) return;

        int actualDamage = damage;

        if (currentArmor > 0)
        {
            int armorDamage = Mathf.RoundToInt(damage * armorAbsorption);
            if (currentArmor >= armorDamage)
            {
                currentArmor -= armorDamage;
                actualDamage = damage - armorDamage;
            }
            else
            {
                actualDamage = damage - currentArmor;
                currentArmor = 0;
            }
            OnArmorChanged?.Invoke(currentArmor);
        }

        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke((float)actualDamage / maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddArmor(int amount)
    {
        currentArmor = Mathf.Min(currentArmor + amount, maxArmor);
        OnArmorChanged?.Invoke(currentArmor);
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        currentArmor = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnArmorChanged?.Invoke(currentArmor);
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
    }
}
