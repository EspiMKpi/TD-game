using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{

    public static EnemySpawner main;


    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 8;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float difficulyScalingFactor = 0.75f;
    [SerializeField] private int totalWaves = 10;   // hết wave -> thắng (UC11)

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave = 1;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;

    private int enemiesThisWave;        // tổng địch dự kiến của wave hiện tại (mốc tính 80%)
    private int spawnedInCurrentWave;   // số địch đã sinh của wave hiện tại

    private void Awake()
    {
        main = this;
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void Start()
    {
        StartCoroutine(StartWave());
    }

    private void Update()
    {
        if (!isSpawning) return;

        timeSinceLastSpawn += Time.deltaTime;

        if(timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            spawnedInCurrentWave++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0){
            EndWave();
        }
    }

    

    private void EnemyDestroyed()
    {
        enemiesAlive--;
    }

    public static int EnemiesAlive
    {
        get { return main != null ? main.enemiesAlive : 0; }
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        enemiesThisWave = EnemiesPerWave();
        enemiesLeftToSpawn = enemiesThisWave;
        spawnedInCurrentWave = 0;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetCurrentWaveIndex(currentWave);
        }
    }

    // UC8 — chỉ cho gọi wave kế sớm khi đã sinh >= 80% wave hiện tại.
    public bool CanCallNextWaveEarly()
    {
        if (!isSpawning || enemiesThisWave <= 0) return false;
        int threshold = Mathf.CeilToInt(enemiesThisWave * 0.8f);
        return spawnedInCurrentWave >= threshold;
    }

    public bool CallNextWaveEarly()
    {
        if (!CanCallNextWaveEarly()) return false;

        currentWave++;
        int nextWaveEnemies = EnemiesPerWave();
        enemiesLeftToSpawn += nextWaveEnemies;   // gộp wave kế vào hàng chờ (cho phép chồng wave)
        enemiesThisWave = nextWaveEnemies;       // mốc 80% tính theo wave mới
        spawnedInCurrentWave = 0;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetCurrentWaveIndex(currentWave);
        }
        return true;
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;

        // UC11 — qua wave cuối thì thắng, không sinh thêm.
        if (currentWave >= totalWaves)
        {
            if (GameSession.Instance != null) GameSession.Instance.OnAllWavesCleared();
            return;
        }

        currentWave++;
        StartCoroutine(StartWave());
    }

    private void SpawnEnemy()
    {
        GameObject prefabToSpawn = enemyPrefabs[0];
        Instantiate(prefabToSpawn, Level_Manager.main.startPoint.position, Quaternion.identity);
    }  

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficulyScalingFactor));
    }
}
