using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Warmup, BuyPhase, LivePhase, RoundEnd, MatchEnd }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    [SerializeField] private int totalRounds = 15;
    [SerializeField] private float warmupTime = 5f;
    [SerializeField] private float buyPhaseTime = 15f;
    [SerializeField] private float roundTime = 115f;
    [SerializeField] private float roundEndTime = 5f;

    [Header("Spawning")]
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private int baseEnemyCount = 4;
    [SerializeField] private int enemyScalePerRound = 1;

    private GameState state;
    private int round;
    private int playerScore;
    private int enemyScore;
    private int aliveEnemies;
    private float timer;
    private PlayerHealth playerHealth;

    public GameState State => state;
    public int Round => round;
    public int PlayerScore => playerScore;
    public int EnemyScore => enemyScore;
    public float Timer => timer;

    public event System.Action<GameState> OnStateChanged;
    public event System.Action<float> OnTimerTick;
    public event System.Action<int, int> OnScoreUpdated;
    public event System.Action<int> OnRoundStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnDeath.AddListener(HandlePlayerDeath);

        StartCoroutine(MatchLoop());
    }

    private IEnumerator MatchLoop()
    {
        yield return Countdown(warmupTime, GameState.Warmup);

        while (round < totalRounds)
        {
            round++;
            OnRoundStarted?.Invoke(round);
            yield return StartCoroutine(RunRound());
        }

        ChangeState(GameState.MatchEnd);
    }

    private IEnumerator RunRound()
    {
        // Buy phase
        RespawnPlayer();
        int waveSize = baseEnemyCount + (round - 1) * enemyScalePerRound;
        enemySpawner?.SpawnWave(waveSize, round / 5);
        aliveEnemies = waveSize;
        yield return Countdown(buyPhaseTime, GameState.BuyPhase);

        // Live phase
        ChangeState(GameState.LivePhase);
        float timeLeft = roundTime;
        while (timeLeft > 0 && aliveEnemies > 0 && !playerHealth.IsDead)
        {
            timeLeft -= Time.deltaTime;
            timer = timeLeft;
            OnTimerTick?.Invoke(timeLeft);
            yield return null;
        }

        if (aliveEnemies <= 0 && !playerHealth.IsDead)
            AwardRound(true);
        else if (playerHealth.IsDead)
            AwardRound(false);
        else
            AwardRound(false); // time ran out

        yield return Countdown(roundEndTime, GameState.RoundEnd);
    }

    private IEnumerator Countdown(float duration, GameState phase)
    {
        ChangeState(phase);
        float t = duration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            timer = t;
            OnTimerTick?.Invoke(t);
            yield return null;
        }
    }

    private void AwardRound(bool playerWon)
    {
        if (playerWon) playerScore++;
        else enemyScore++;
        OnScoreUpdated?.Invoke(playerScore, enemyScore);
    }

    private void RespawnPlayer()
    {
        if (playerHealth == null) return;
        playerHealth.Respawn();

        if (playerSpawns.Length > 0)
        {
            Transform sp = playerSpawns[Random.Range(0, playerSpawns.Length)];
            playerHealth.transform.SetPositionAndRotation(sp.position, sp.rotation);
        }

        playerHealth.gameObject.SetActive(true);
    }

    public void RegisterKill()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }

    private void HandlePlayerDeath()
    {
        // Let the live phase loop detect it via playerHealth.IsDead
    }

    private void ChangeState(GameState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
