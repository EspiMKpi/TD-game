using UnityEngine;
using UnityEngine.Events;

// Căn cứ người chơi (UC11). Giữ đúng tên lớp 'Base' theo thiết kế —
// chỉ 'base' viết thường mới là từ khóa C# nên tên này hợp lệ.
//
// Tạm dùng singleton 'main' theo đúng pattern các manager hiện có
// (Level_Manager, EnemySpawner, BuildManager). Khi GameSession ra đời,
// nó sẽ sở hữu Base (quan hệ hợp thành) và lắng nghe onBaseDestroyed
// để chuyển status = GameStatus.Lost.
public class Base : MonoBehaviour
{
    public static Base main;

    [Header("Attributes")]
    [SerializeField] private int maxHP = 20;

    [Header("Events")]
    public UnityEvent onHealthChanged = new UnityEvent();
    public UnityEvent onBaseDestroyed = new UnityEvent();

    private int currentHP;
    private bool isDestroyed = false;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    private void Awake()
    {
        main = this;
        currentHP = maxHP;
    }

    // Khởi tạo lại theo Level đã chọn (Giai đoạn 7).
    public void Initialize(int newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = maxHP;
        isDestroyed = false;
        onHealthChanged.Invoke();
    }

    // Tên phương thức giữ đúng thiết kế: takeDamage(damageAmount).
    public void takeDamage(int damageAmount)
    {
        if (isDestroyed) return;

        currentHP -= damageAmount;
        if (currentHP < 0) currentHP = 0;
        onHealthChanged.Invoke();

        if (currentHP <= 0)
        {
            isDestroyed = true;
            onBaseDestroyed.Invoke();
            Debug.Log("Base destroyed — game over (Lost).");
            // GameSession.OnBaseDestroyed() sẽ được nối ở giai đoạn sau.
        }
    }
}
