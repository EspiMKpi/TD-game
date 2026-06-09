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

    // Giai đoạn 6: cấu hình theo Level.waves (nếu có) — cho phép nhiều loại lính.
    private Level currentLevel;
    private float currentSpawnRate;     // tốc độ sinh của wave hiện tại (từ Wave.spawnRate hoặc mặc định)
    private GameObject currentWavePrefab; // prefab lính của wave hiện tại (từ Wave.enemyPrefab hoặc mặc định)
    private float currentDifficulty = 1f; // hệ số độ khó của wave hiện tại (Wave.difficultyMultiplier)

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

        if(timeSinceLastSpawn >= (1f / Mathf.Max(0.01f, currentSpawnRate)) && enemiesLeftToSpawn > 0)
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
        ApplyWaveConfig(currentWave);
        enemiesLeftToSpawn = enemiesThisWave;
        spawnedInCurrentWave = 0;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetCurrentWaveIndex(currentWave);
        }
    }

    // Đặt tổng số wave theo Level đã chọn (Giai đoạn 7).
    public void SetTotalWaves(int t)
    {
        if (t > 0) totalWaves = t;
    }

    // Giai đoạn 6: nhận Level đã chọn để sinh lính theo cấu hình Wave (nhiều loại lính).
    public void SetLevel(Level lvl)
    {
        currentLevel = lvl;
        if (lvl != null && lvl.waveCount > 0) totalWaves = lvl.waveCount;
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
        ApplyWaveConfig(currentWave);            // số lính/tốc độ/prefab theo wave mới
        enemiesLeftToSpawn += enemiesThisWave;   // gộp wave kế vào hàng chờ (cho phép chồng wave)
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
        GameObject prefabToSpawn = currentWavePrefab != null
            ? currentWavePrefab
            : (enemyPrefabs != null && enemyPrefabs.Length > 0 ? enemyPrefabs[0] : null);
        if (prefabToSpawn == null || Level_Manager.main == null || Level_Manager.main.startPoint == null) return;
        GameObject enemy = SimplePool.Get(prefabToSpawn, Level_Manager.main.startPoint.position, Quaternion.identity);
        if (enemy == null) return;

        // Giai đoạn 8: áp độ khó của wave vào máu/sát thương lính vừa sinh.
        if (currentDifficulty != 1f)
        {
            var h = enemy.GetComponentInChildren<Health>();
            if (h != null) h.ApplyDifficulty(currentDifficulty);
            var m = enemy.GetComponentInChildren<EnemyMovement>();
            if (m != null) m.ApplyDifficulty(currentDifficulty);
        }
    }

    // Áp cấu hình của một wave: ưu tiên Wave trong Level, nếu không có thì dùng công thức/mặc định.
    private void ApplyWaveConfig(int waveNumber)
    {
        Wave w = GetWave(waveNumber);
        enemiesThisWave  = PlannedEnemiesForWave(waveNumber);
        currentSpawnRate = (w != null && w.spawnRate > 0f) ? w.spawnRate : enemiesPerSecond;
        currentWavePrefab = (w != null && w.enemyPrefab != null)
            ? w.enemyPrefab
            : (enemyPrefabs != null && enemyPrefabs.Length > 0 ? enemyPrefabs[0] : null);
        currentDifficulty = (w != null && w.difficultyMultiplier > 0f) ? w.difficultyMultiplier : 1f;
    }

    private Wave GetWave(int waveNumber)
    {
        if (currentLevel == null || currentLevel.waves == null) return null;
        int idx = waveNumber - 1;
        return (idx >= 0 && idx < currentLevel.waves.Count) ? currentLevel.waves[idx] : null;
    }

    // Số lính dự kiến của một wave: theo Wave.enemyCount nếu có, nếu không theo công thức scaling.
    public int PlannedEnemiesForWave(int waveNumber)
    {
        Wave w = GetWave(waveNumber);
        if (w != null && w.enemyCount > 0) return w.enemyCount;
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(waveNumber, difficulyScalingFactor));
    }
}
