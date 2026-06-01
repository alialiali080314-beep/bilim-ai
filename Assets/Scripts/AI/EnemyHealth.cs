using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private ParticleSystem hitEffect;

    private int currentHealth;
    private EnemyAI ai;
    private bool isDead;

    public bool IsDead => isDead;

    public event System.Action<int> OnDamaged;
    public event System.Action OnKilled;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector3 direction)
    {
        if (isDead) return;

        currentHealth -= damage;
        OnDamaged?.Invoke(damage);
        hitEffect?.Play();

        // Wake up the AI
        if (ai != null && ai.CurrentState == EnemyState.Patrol)
            ai.SetState(EnemyState.Chase);

        if (currentHealth <= 0)
        {
            isDead = true;
            OnKilled?.Invoke();
            ai?.Die();
        }
    }
}
