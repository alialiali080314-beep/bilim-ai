using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Patrol, Alert, Chase, Attack, Dead }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float sightRange = 20f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private float hearingRange = 10f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 14f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] [Range(0f, 1f)] private float accuracy = 0.65f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("FX")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip alertSound;
    [SerializeField] private AudioClip deathSound;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform player;
    private EnemyState currentState = EnemyState.Patrol;
    private int patrolIndex;
    private float lastAttackTime;

    public EnemyState CurrentState => currentState;

    public event System.Action OnDied;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (currentState != EnemyState.Dead)
        {
            switch (currentState)
            {
                case EnemyState.Patrol: yield return StartCoroutine(Patrol()); break;
                case EnemyState.Alert:  yield return StartCoroutine(Alert());  break;
                case EnemyState.Chase:  yield return StartCoroutine(Chase());  break;
                case EnemyState.Attack: yield return StartCoroutine(AttackLoop()); break;
            }
            yield return null;
        }
    }

    private IEnumerator Patrol()
    {
        agent.speed = 2.5f;
        agent.isStopped = false;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);

        while (currentState == EnemyState.Patrol)
        {
            if (CanSeePlayer()) { SetState(EnemyState.Alert); yield break; }

            if (patrolPoints.Length > 0 && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                yield return new WaitForSeconds(patrolWaitTime);
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }

            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator Alert()
    {
        agent.isStopped = true;
        if (alertSound) audioSource.PlayOneShot(alertSound);
        yield return new WaitForSeconds(0.4f);
        SetState(EnemyState.Chase);
    }

    private IEnumerator Chase()
    {
        agent.speed = 5.5f;
        agent.isStopped = false;

        while (currentState == EnemyState.Chase)
        {
            if (player == null) { SetState(EnemyState.Patrol); yield break; }

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= attackRange && CanSeePlayer()) { SetState(EnemyState.Attack); yield break; }
            if (dist > sightRange * 1.8f)              { SetState(EnemyState.Patrol); yield break; }

            agent.SetDestination(player.position);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator AttackLoop()
    {
        agent.isStopped = true;

        while (currentState == EnemyState.Attack)
        {
            if (player == null) { SetState(EnemyState.Patrol); yield break; }

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > attackRange || !CanSeePlayer()) { SetState(EnemyState.Chase); yield break; }

            // Face player on Y axis
            Vector3 lookDir = (player.position - transform.position);
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(lookDir), 8f * Time.deltaTime);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }

            yield return null;
        }
    }

    private void Shoot()
    {
        muzzleFlash?.Play();
        if (shootSound) audioSource.PlayOneShot(shootSound);

        if (Random.value <= accuracy)
            player.GetComponent<IDamageable>()?.TakeDamage(attackDamage, (player.position - transform.position).normalized);
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > sightRange) return false;
        if (Vector3.Angle(transform.forward, toPlayer) > fieldOfView * 0.5f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        if (Physics.Raycast(eyePos, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return true;
    }

    public void SetState(EnemyState state) => currentState = state;

    public void Die()
    {
        SetState(EnemyState.Dead);
        agent.isStopped = true;
        agent.enabled = false;

        if (deathSound) audioSource.PlayOneShot(deathSound);

        GetComponent<Collider>()?.gameObject.SetActive(false);
        OnDied?.Invoke();

        Destroy(gameObject, 3f);
    }
}
