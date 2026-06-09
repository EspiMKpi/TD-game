using System.Collections.Generic;
using UnityEngine;

// Pool tái sử dụng GameObject theo prefab (Giai đoạn 8 — tối ưu Enemy/Projectile).
// Get: lấy từ pool hoặc Instantiate mới. Release: tắt active rồi trả về pool.
// Đối tượng tự reset trạng thái trong OnEnable khi được bật lại.
public static class SimplePool
{
    private static readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        GameObject obj = null;
        if (pools.TryGetValue(prefab, out var q))
            while (q.Count > 0 && obj == null) obj = q.Dequeue();   // bỏ qua phần tử đã bị hủy

        if (obj == null)
        {
            obj = Object.Instantiate(prefab, position, rotation);
            obj.AddComponent<PooledObject>().SourcePrefab = prefab;
        }
        else
        {
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);   // -> OnEnable reset
        }
        return obj;
    }

    public static void Release(GameObject obj)
    {
        if (obj == null) return;
        var p = obj.GetComponent<PooledObject>();
        if (p == null || p.SourcePrefab == null) { Object.Destroy(obj); return; }   // không thuộc pool

        obj.SetActive(false);
        if (!pools.TryGetValue(p.SourcePrefab, out var q))
        {
            q = new Queue<GameObject>();
            pools[p.SourcePrefab] = q;
        }
        q.Enqueue(obj);
    }
}
