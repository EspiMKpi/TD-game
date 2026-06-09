using UnityEngine;

// Một đợt lính (dữ liệu cấu hình, lồng trong Level). Class thường + [Serializable].
[System.Serializable]
public class Wave
{
    public int waveIndex;
    public int enemyCount;
    public float spawnRate;
    public float difficultyMultiplier = 1f;

    // Prefab Enemy cần sinh (Unity cần biết sinh con gì).
    public GameObject enemyPrefab;

    // Thiết kế gốc: spawnEnemy() trả Enemy. Lớp Enemy chưa tách riêng
    // (đang là EnemyMovement + Health), nên tạm trả GameObject vừa sinh.
    // Sẽ đổi kiểu trả về khi refactor Enemy ở increment sau.
    // GameSession sẽ gọi hàm này qua coroutine theo nhịp spawnRate.
    public GameObject spawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null) return null;
        GameObject enemy = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        // TODO: áp difficultyMultiplier vào hp/damage của enemy sau khi tách lớp Enemy.
        return enemy;
    }
}
