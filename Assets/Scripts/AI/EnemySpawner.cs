using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnDelay = 0.5f;

    private readonly List<GameObject> alive = new List<GameObject>();

    public int AliveCount
    {
        get
        {
            alive.RemoveAll(e => e == null);
            return alive.Count;
        }
    }

    public void SpawnWave(int count, int difficulty = 0)
    {
        StartCoroutine(SpawnRoutine(count, difficulty));
    }

    private IEnumerator SpawnRoutine(int count, int difficulty)
    {
        alive.RemoveAll(e => e == null);

        int prefabIdx = Mathf.Clamp(difficulty, 0, enemyPrefabs.Length - 1);

        for (int i = 0; i < count; i++)
        {
            if (spawnPoints.Length == 0) break;
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var enemy = Instantiate(enemyPrefabs[prefabIdx], sp.position, sp.rotation);
            alive.Add(enemy);

            // Notify GameManager when each enemy dies
            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
                health.OnKilled += () => GameManager.Instance?.RegisterKill();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void DespawnAll()
    {
        foreach (var e in alive)
            if (e != null) Destroy(e);
        alive.Clear();
    }
}
