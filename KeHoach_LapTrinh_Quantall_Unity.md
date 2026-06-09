# KẾ HOẠCH LẬP TRÌNH GAME THỦ THÀNH QUANTALL (UNITY)

> Tài liệu này chuyển thiết kế hướng đối tượng trong báo cáo (Chương 1–3) thành một kế hoạch hiện thực hóa cụ thể trên Unity. Nguyên tắc xuyên suốt: **giữ nguyên tên lớp, tên thuộc tính và kiểu dữ liệu** đã thiết kế; chỉ bổ sung những thứ Unity bắt buộc phải có (vòng lặp `Update`, coroutine, tham chiếu scene…) mà không làm thay đổi mô hình lớp.

Giả định kỹ thuật (nếu khác, chỉ cần đổi lại tương ứng):
- Unity 2022 LTS trở lên, ngôn ngữ C#.
- Giao diện dùng **uGUI (Canvas)** — phổ biến và dễ làm cho đồ án. (Nếu dùng UI Toolkit thì phần View đổi cách viết, phần logic giữ nguyên.)
- Mỗi màn chơi là **một asset cấu hình** (ScriptableObject) được nạp vào **một scene Gameplay dùng chung**, thay vì tạo một scene riêng cho mỗi màn.

---

## 1. Nguyên tắc chuyển thiết kế OOP sang Unity

Trong báo cáo, mọi thứ là "lớp thực thể" thuần. Unity lại theo mô hình **component**: thứ gì xuất hiện trên màn hình và cần `Update` mỗi khung hình thì là `MonoBehaviour`; thứ gì là **dữ liệu cấu hình** thì nên là `ScriptableObject`; thứ gì chỉ là dữ liệu thuần trong bộ nhớ thì là class C# thường. Bảng dưới đây ánh xạ từng lớp trong thiết kế sang loại Unity phù hợp, **không đổi tên lớp**.

| Lớp (theo báo cáo) | Loại trong Unity | Lý do |
| :--- | :--- | :--- |
| `Player` | `MonoBehaviour` (kiểu InputManager) | Có `checkClick()`, `exit()` — xử lý input toàn cục |
| `Settings` | `ScriptableObject` + lưu PlayerPrefs/JSON | Dữ liệu cấu hình, cần lưu lại giữa các phiên |
| `GameSession` | `MonoBehaviour` (singleton điều phối ván đấu) | Trung tâm điều phối: tài nguyên, build/upgrade/sell, wave, pause |
| `Base` | `MonoBehaviour` | Có vị trí trên scene, nhận sát thương |
| `Level` | `ScriptableObject` (cấu hình) + SaveData (tiến trình) | Dữ liệu màn chơi tĩnh + tiến trình mở khóa |
| `TowerSlot` | `MonoBehaviour` | Điểm đặt cố định trên scene, click được |
| `Tower` | `MonoBehaviour` | Object trên scene, tự bắn mỗi frame |
| `Projectile` | `MonoBehaviour` | Object bay trên scene, va chạm |
| `Wave` | class C# `[System.Serializable]` (lồng trong `Level`) | Dữ liệu cấu hình một đợt lính |
| `Enemy` | `MonoBehaviour` | Object di chuyển trên scene |
| `PowerUp` | `ScriptableObject` (dữ liệu) + tham chiếu | Cấu hình kỹ năng; hiệu ứng do `GameSession` thực thi |
| `Path`, `Waypoint` | `MonoBehaviour` + mảng `Transform` | Đường đi của kẻ địch trên scene |
| Các lớp `*View` | `MonoBehaviour` (gắn lên Canvas) | Màn hình giao diện |

### Những bổ sung Unity bắt buộc (không phá vỡ thiết kế)
- **Coroutine sinh lính:** thiết kế đặt `spawnEnemy()` ở `Wave`, nhưng cần một thứ điều phối *thời gian*. `GameSession` chạy một coroutine theo vòng đời ván đấu và gọi logic của `Wave` — chức năng vẫn thuộc `Wave` đúng như thiết kế.
- **Object Pooling** cho `Enemy` và `Projectile`: làm ở giai đoạn tối ưu (Giai đoạn 8), không đổi mô hình lớp.
- **Singleton accessor** cho `GameSession` để các object trên scene (Tower, Enemy) truy cập tài nguyên/căn cứ.

---

## 2. Lưu ý đặt tên quan trọng trước khi code

Có một vài chỗ tên trong thiết kế xung đột với Unity. Cần quyết định ngay từ đầu để không phải refactor về sau:

1. **Thuộc tính `name`** xuất hiện ở `Player`, `Tower`, `Enemy`. Cả ba dự kiến là `MonoBehaviour`, mà `UnityEngine.Object` **đã có sẵn property `name`** (tên của GameObject). Để tránh che khuất:
   - `Player.name` → `playerName`
   - `Tower.name` → `towerName`
   - `Enemy.name` → `enemyName`
   
   (Giữ đúng tinh thần thiết kế, chỉ thêm tiền tố cho rõ nghĩa và tránh trùng.)

2. **Lớp `Base`**: tên hợp lệ trong C# (`base` viết thường mới là từ khóa), giữ nguyên `Base`. Lưu ý đặt nó trong namespace riêng (ví dụ `Quantall`) để không nhầm lẫn khi đọc code.

3. **`GameSession.status : boolean`**: thiết kế dùng boolean cho trạng thái thắng/thua. Thực tế cần 3 trạng thái (đang chơi / thắng / thua). Đề xuất giữ tên `status` nhưng đổi kiểu sang enum:
   ```csharp
   public enum GameStatus { Playing, Won, Lost }
   ```
   Đây là cải tiến nhỏ, vẫn giữ đúng tên thuộc tính.

4. **`type` (kiểu String)** ở `Tower`, `Projectile`, `Enemy`, `PowerUp`: thiết kế để String. Đề xuất chuyển thành enum cho an toàn (tránh gõ sai chuỗi), giữ nguyên tên thuộc tính:
   ```csharp
   public enum TowerType { Single, Multi, Explosive, Slow }      // Bắn đơn / đa / phát nổ / làm chậm
   public enum ProjectileType { Single, Explosive, Multi }
   public enum PowerUpType { Portal, Airstrike, SpeedBoost }     // Cổng dịch chuyển / Không kích / Tăng tốc
   ```
   Nếu muốn giữ đúng 100% kiểu `String` như báo cáo thì để nguyên — nhưng nên ghi chú lý do.

---

## 3. Cấu trúc thư mục & Scene

```
Assets/
  Scripts/
    Core/           GameSession.cs, Player.cs, SaveSystem.cs
    Data/           Settings.cs, Level.cs, Wave.cs (ScriptableObject / serializable)
    Entities/       Base.cs, TowerSlot.cs, Tower.cs, Projectile.cs, Enemy.cs, PowerUp.cs
    Map/            Path.cs, Waypoint.cs
    UI/             MainMenuView.cs, SettingsView.cs, LevelSelectView.cs,
                    LevelDetailView.cs, ConfirmExitView.cs, GameplayView.cs, ResultView.cs
    Enums/          GameStatus.cs, TowerType.cs, ProjectileType.cs, PowerUpType.cs
  Prefabs/
    Towers/         Tower_Single, Tower_Multi, Tower_Explosive, Tower_Slow
    Enemies/        Enemy_*
    Projectiles/    Projectile_*
    UI/
  ScriptableObjects/
    Levels/         Level_01.asset, Level_02.asset, ...
    PowerUps/       PowerUp_Portal.asset, PowerUp_Airstrike.asset, PowerUp_Speed.asset
  Scenes/
    Boot.unity      (khởi tạo Settings, load save, sang MainMenu)
    MainMenu.unity  (MainMenuView, SettingsView, LevelSelectView, LevelDetailView, ConfirmExitView)
    Gameplay.unity  (GameplayView, GameSession, Base, TowerSlot, Path; nạp Level đã chọn)
```

Luồng scene: **Boot → MainMenu → Gameplay → (quay lại) MainMenu**. Màn chơi được chọn lưu vào một biến tĩnh/`ScriptableObject` runtime để scene Gameplay đọc khi load.

---

## 4. Khung mã từng lớp (giữ nguyên tên & kiểu)

### 4.1. Lớp dữ liệu (Data layer)

**`Settings`** — `ScriptableObject`, lưu qua PlayerPrefs/JSON.
```csharp
[CreateAssetMenu(menuName = "Quantall/Settings")]
public class Settings : ScriptableObject
{
    public float musicVolume;
    public float sfxVolume;
    public string graphicsQuality;
    public string language;
    public string displayMode;

    // UC2 – Điều chỉnh cài đặt
    public bool saveSettings(float musicVolume, float sfxVolume,
                             string graphicsQuality, string language, string displayMode)
    {
        this.musicVolume = musicVolume;
        this.sfxVolume = sfxVolume;
        this.graphicsQuality = graphicsQuality;
        this.language = language;
        this.displayMode = displayMode;
        // TODO: ghi xuống PlayerPrefs/JSON; áp dụng AudioMixer, Screen.SetResolution...
        return true;
    }
}
```

**`Wave`** — class thường, lồng trong `Level`.
```csharp
[System.Serializable]
public class Wave
{
    public int waveIndex;
    public int enemyCount;
    public float spawnRate;
    public float difficultyMultiplier;

    // Tham chiếu prefab Enemy cần sinh (Unity cần biết sinh con gì)
    public GameObject enemyPrefab;

    // spawnEnemy() được GameSession gọi qua coroutine theo nhịp spawnRate
    public Enemy spawnEnemy(Vector3 position)
    {
        // TODO: Instantiate/lấy từ pool, áp difficultyMultiplier vào hp/damage
        return null;
    }
}
```

**`Level`** — `ScriptableObject` (cấu hình) + tiến trình. Tiến trình (`isUnlocked`, `bestScore`, `bestStars`) về logic thuộc `Level` đúng như thiết kế, nhưng khi *lưu game* sẽ được ghi qua `SaveSystem` (xem Giai đoạn 7).
```csharp
[CreateAssetMenu(menuName = "Quantall/Level")]
public class Level : ScriptableObject
{
    public int levelId;
    public int waveCount;
    public int initialResources;
    public int baseMaxHP;

    // Tiến trình (đọc/ghi qua SaveSystem lúc runtime)
    public bool isUnlocked;
    public int bestScore;
    public int bestStars;

    // Cấu hình màn chơi cho Unity
    public List<Wave> waves;             // Level "hợp thành" nhiều Wave
    public List<TowerSlot> towerSlots;   // tham chiếu các ô đặt tháp trên scene

    public static Level loadLevel(int levelId) { /* TODO: load asset theo id */ return null; }
    public void saveResult(int score, int stars)
    {
        if (score > bestScore) bestScore = score;
        if (stars > bestStars) bestStars = stars;
        // TODO: gọi SaveSystem.Save()
    }
}
```

### 4.2. Đối tượng trên scene (Entities)

**`Base`**
```csharp
public class Base : MonoBehaviour
{
    public int currentHP;
    public int maxHP;

    public void takeDamage(int damageAmount)
    {
        currentHP -= damageAmount;
        if (currentHP <= 0) GameSession.Instance.OnBaseDestroyed(); // -> thua (UC11)
    }
}
```

**`TowerSlot`**
```csharp
public class TowerSlot : MonoBehaviour
{
    public Position position;     // hoặc Vector3; giữ tên 'position'
    public bool isOccupied;

    public Tower tower;           // quan hệ 0..1: một ô chứa nhiều nhất một tháp
}
```
> `Position` trong thiết kế có thể dùng thẳng `Vector3` của Unity. Nếu muốn giữ kiểu riêng `Position`, tạo một struct nhỏ — nhưng `Vector3` tiện hơn cho mọi tính toán.

**`Tower`**
```csharp
public class Tower : MonoBehaviour
{
    public string towerName;   // 'name' trong thiết kế (đổi để tránh trùng Object.name)
    public TowerType type;     // 'type'
    public int buildCost;
    public int damage;
    public float range;
    public float fireRate;
    public int upgradeCost;
    public int sellPrice;
    public int currentLevel;

    private List<Projectile> projectiles = new List<Projectile>(); // Tower hợp thành Projectile

    void Update()
    {
        // tìm Enemy trong 'range', tới nhịp 'fireRate' thì gọi fire(target)
    }

    public void upgrade()
    {
        currentLevel++;
        // TODO: tăng damage/range/fireRate; cập nhật upgradeCost, sellPrice
    }

    public void fire(Enemy target) { /* TODO: tạo Projectile, thêm vào projectiles */ }
}
```

**`Projectile`**
```csharp
public class Projectile : MonoBehaviour
{
    public ProjectileType type;
    public float flySpeed;
    public int damage;
    public float explosionRadius;   // chỉ dùng cho loại phát nổ

    public void hit(Enemy target)
    {
        target.GetComponent<Enemy>(); // TODO: gây 'damage'; nếu phát nổ thì quét trong 'explosionRadius'
    }
}
```

**`Enemy`**
```csharp
public class Enemy : MonoBehaviour
{
    public string enemyName;   // 'name' trong thiết kế
    public EnemyType type;     // (hoặc giữ string 'type')
    public int hp;
    public float moveSpeed;
    public int damageToBase;
    public int rewardResource;
    public int pathPosition;   // chỉ số waypoint hiện tại trên Path

    private Path path;

    void Update() { move(); }

    public void move()
    {
        // di chuyển tới Waypoint thứ 'pathPosition'; tới đích cuối -> attackBase()
    }

    public void attackBase()
    {
        GameSession.Instance.theBase.takeDamage(damageToBase);
        // TODO: hủy/trả pool enemy
    }
}
```

**`PowerUp`** — `ScriptableObject` cho dữ liệu; hiệu ứng do `GameSession.activatePowerUp()` thực thi.
```csharp
[CreateAssetMenu(menuName = "Quantall/PowerUp")]
public class PowerUp : ScriptableObject
{
    public string powerUpName;   // 'name'
    public PowerUpType type;     // Portal / Airstrike / SpeedBoost
    public int resourceCost;
    public float cooldown;
    public float effectRadius;
    public float effectDuration;

    // Danh sách đối tượng chịu tác động khi kích hoạt (theo thiết kế: kết tập)
    [System.NonSerialized] public List<Tower> affectedTowers = new List<Tower>();
    [System.NonSerialized] public List<Enemy> affectedEnemies = new List<Enemy>();

    public void activate(Vector3 targetPosition)
    {
        // TODO theo 'type':
        //  - Portal      : dịch chuyển kẻ địch trong 'effectRadius' lùi lại trên Path
        //  - Airstrike   : gây sát thương diện rộng các Enemy trong 'effectRadius'
        //  - SpeedBoost  : tăng fireRate các Tower trong 'effectRadius' trong 'effectDuration'
    }
}
```

### 4.3. Lớp điều phối & input

**`GameSession`** — trái tim của ván đấu (UC4–UC10).
```csharp
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public int currentResources;
    public int currentWaveIndex;
    public int score;
    public int stars;
    public GameStatus status;        // 'status' (đổi boolean -> enum)

    public Base theBase;             // GameSession hợp thành Base (1-1)
    public List<PowerUp> powerUps;   // GameSession kết tập PowerUp (1-n)
    private Level currentLevel;

    void Awake() { Instance = this; }

    public bool buildTower(TowerType towerType, Vector3 position)
    {
        // kiểm tra slot trống + đủ currentResources -> trừ tiền -> đặt Tower
        return true;
    }
    public void upgradeTower(int towerId) { /* kiểm tra tiền -> Tower.upgrade() */ }
    public void sellTower(int towerId)    { /* gỡ Tower, hoàn 'sellPrice' vào currentResources */ }
    public void callNextWaveEarly()       { /* UC8: chỉ khi đã spawn >= 80% wave hiện tại */ }
    public void activatePowerUp(PowerUpType powerUpType, Vector3 targetPosition)
    {
        // kiểm tra cooldown + đủ tiền -> PowerUp.activate(targetPosition)
    }
    public void pause() { Time.timeScale = 0f; /* mở Pause Menu */ }

    public void OnBaseDestroyed() { status = GameStatus.Lost; /* -> ResultView */ }
}
```

**`Player`** — xử lý input chung.
```csharp
public class Player : MonoBehaviour
{
    public string playerName;   // 'name'
    public Settings settings;   // Player liên kết Settings (1-1)

    void Update() { checkClick(); }

    public void checkClick() { /* bắt input chuột/chạm, gọi handler tương ứng */ }
    public void exit()       { /* UC12: lưu rồi Application.Quit() */ }
}
```

### 4.4. Các lớp giao diện (View)

Mỗi `*View` là một `MonoBehaviour` gắn lên Canvas. Các thành phần `out*` / `outsub*` trong báo cáo trở thành **trường tham chiếu UI được serialize** (`[SerializeField]`) trỏ tới Text/Button/Slider trên Canvas. Ví dụ:

```csharp
public class GameplayView : MonoBehaviour
{
    [SerializeField] Text outResources;     // hiển thị tài nguyên
    [SerializeField] Text outWave;          // current wave
    [SerializeField] Text outScore;
    [SerializeField] Text outStar;
    [SerializeField] Slider outBaseHP;      // máu căn cứ
    [SerializeField] Button outsubTower;    // mở đặt tháp
    [SerializeField] Button outsubPowerUp;  // dùng power-up
    [SerializeField] Button outsubUpgrade;  // nâng cấp
    [SerializeField] Button outsubPause;    // tạm dừng
    [SerializeField] Button subCallNextWave;// gọi wave sớm
    [SerializeField] Button subSellTower;   // bán tháp
    // ... nối các sự kiện onClick tới phương thức tương ứng của GameSession
}
```

Danh sách View cần dựng (giữ đúng tên): `MainMenuView`, `SettingsView`, `LevelSelectView`, `LevelDetailView`, `ConfirmExitView`, `GameplayView`, `ResultView`. Mỗi View nối nút bấm tới phương thức của lớp logic tương ứng (`Settings.saveSettings`, `Level.loadLevel`, `GameSession.*`, `Player.exit`…).

---

## 5. Hiện thực quan hệ giữa các lớp trong Unity

Báo cáo phân loại quan hệ thành **hợp thành (composition)** và **kết tập (aggregation)**. Trong Unity:

- **Hợp thành** (vòng đời con phụ thuộc cha) → cha tạo và hủy con. Ví dụ `Tower` tạo/hủy `Projectile`; `Level` "sở hữu" `List<Wave>`, `List<TowerSlot>`; `GameSession` tạo/hủy `Base`; `Player` tạo/hủy `GameSession`.
- **Kết tập** (con có vòng đời riêng) → cha chỉ **giữ tham chiếu**, không hủy con. Ví dụ `Wave` giữ `List<Enemy>` (enemy sống/chết độc lập); `TowerSlot` giữ tham chiếu `Tower`; `PowerUp` giữ `List<Tower>`/`List<Enemy>` đang chịu tác động; `GameSession` giữ `List<PowerUp>`.
- **Liên kết** (association) → tham chiếu đơn thuần: `Player` ↔ `GameSession`, `Player` ↔ `Settings`.

Cách hiện thực: dùng `List<T>` cho quan hệ 1-n, tham chiếu trực tiếp cho 1-1, đặt prefab con dưới object cha trong Hierarchy với quan hệ hợp thành.

---

## 6. Lộ trình phát triển theo giai đoạn

Thứ tự dưới đây ưu tiên dựng **vòng lặp gameplay chạy được sớm nhất** rồi mới bồi đắp menu và lưu trữ. Cột "Use case" giúp bạn đối chiếu thẳng với Chương 1–2 của báo cáo (phục vụ Chương 4 – Cài đặt).

**Giai đoạn 0 — Thiết lập (1–2 ngày).** Tạo project, cài cấu trúc thư mục mục 3, thiết lập Git, dựng các file enum, tạo 3 scene rỗng (Boot/MainMenu/Gameplay). Tạo lớp `Settings`, `Level`, `Wave` (chỉ field + ScriptableObject, chưa có logic).

**Giai đoạn 1 — Đường đi & kẻ địch di chuyển (UC4 phần lõi).** Dựng `Path` + `Waypoint`; viết `Enemy.move()` để đi theo waypoint; cho `Base` đứng cuối đường và `Enemy.attackBase()` gọi `Base.takeDamage()`. *Mốc kiểm chứng:* một con enemy đi hết đường và trừ máu căn cứ.

**Giai đoạn 2 — Hệ thống đợt lính (UC4, UC8).** `GameSession` chạy coroutine wave: theo `Wave.spawnRate` gọi `Wave.spawnEnemy()` đủ `enemyCount`; chuyển `currentWaveIndex`. Thêm điều kiện 80% để mở `callNextWaveEarly()`. *Mốc:* nhiều wave sinh ra tuần tự, gọi wave sớm hoạt động.

**Giai đoạn 3 — Tháp & đạn (UC5).** `TowerSlot` click được; `GameSession.buildTower()` kiểm tra tiền + slot trống rồi đặt `Tower`; `Tower` tự tìm mục tiêu trong `range`, theo `fireRate` gọi `fire()` tạo `Projectile`; `Projectile.hit()` trừ `hp` của `Enemy`. Làm trước loại tháp **Single**. *Mốc:* đặt tháp → tháp bắn → enemy chết → cộng `rewardResource`.

**Giai đoạn 4 — Kinh tế & quản lý tháp (UC6, UC7).** Hoàn thiện `currentResources` (thu/chi); `upgradeTower()` + `Tower.upgrade()`; `sellTower()` hoàn `sellPrice`. Bổ sung 3 loại tháp còn lại: **Multi, Explosive, Slow** (mỗi loại khác cách chọn mục tiêu / hiệu ứng đạn).

**Giai đoạn 5 — Kỹ năng bổ trợ (UC9).** `PowerUp` với 3 loại: **Portal (Cổng dịch chuyển)** đẩy lùi enemy trên Path — *điểm nhấn của Quantall theo báo cáo*; **Airstrike (Không kích)** sát thương diện rộng; **SpeedBoost (Tăng tốc)** buff tháp. `GameSession.activatePowerUp()` xử lý chọn vùng + cooldown.

**Giai đoạn 6 — Trạng thái ván & UI (UC1, UC2, UC3, UC10, UC11, UC12).** Dựng `GameplayView` (HUD), `MainMenuView`, `SettingsView`, `LevelSelectView`, `LevelDetailView`, `ConfirmExitView`, `ResultView`. Nối `pause()`, kết quả thắng (qua wave cuối) / thua (`Base` hết máu) → `ResultView`. Nối `Player.exit()` qua `ConfirmExitView`.

**Giai đoạn 7 — Lưu trữ & tiến trình.** `SaveSystem` đọc/ghi tiến trình `Level` (`isUnlocked`, `bestScore`, `bestStars`) và `Settings`. `Level.loadLevel()`, `Level.saveResult()` hoạt động đầy đủ; mở khóa màn kế tiếp khi thắng.

**Giai đoạn 8 — Tối ưu & hoàn thiện.** Object Pooling cho `Enemy`/`Projectile`; áp `graphicsQuality`, `displayMode`, âm lượng `music/sfx`; cân bằng độ khó (`difficultyMultiplier`); hiệu ứng, âm thanh, polish.

---

## 7. Kế hoạch kiểm thử (phục vụ Chương 5)

Kiểm thử bám theo bảng kịch bản chuẩn/ngoại lệ ở mục 2.1 của báo cáo. Với mỗi use case, kiểm cả luồng chính và các ngoại lệ đã liệt kê:

- **UC5 Xây tháp** — ngoại lệ: không đủ tài nguyên → không cho đặt, có thông báo.
- **UC6 Nâng cấp** — ngoại lệ: tháp đạt cấp tối đa; không đủ tài nguyên.
- **UC8 Chuyển wave sớm** — ngoại lệ: chưa đạt 80% → nút bị khóa.
- **UC9 Power-up** — ngoại lệ: đang hồi chiêu; vùng/mục tiêu không hợp lệ.
- **UC11 Kết quả** — kiểm điều kiện thắng (qua wave cuối) và thua (`Base` hết máu); lưu `bestScore`/`bestStars` đúng.

Nên viết một vài **Unity Test (PlayMode)** cho logic thuần: ví dụ kiểm `buildTower()` trừ đúng tiền, `sellTower()` hoàn đúng `sellPrice`, `Base.takeDamage()` về 0 thì `status = Lost`.

---

## 8. Bảng đối chiếu nhanh lớp ↔ phương thức (để khỏi sót khi code)

| Lớp | Thuộc tính chính | Phương thức (theo thiết kế) |
| :--- | :--- | :--- |
| `Player` | playerName | exit(), checkClick() |
| `Settings` | musicVolume, sfxVolume, graphicsQuality, language, displayMode | saveSettings(...) |
| `GameSession` | currentResources, currentWaveIndex, score, stars, status | buildTower(), upgradeTower(), sellTower(), callNextWaveEarly(), activatePowerUp(), pause() |
| `Base` | currentHP, maxHP | takeDamage(damageAmount) |
| `Level` | levelId, waveCount, initialResources, baseMaxHP, isUnlocked, bestScore, bestStars | loadLevel(levelId), saveResult(score, stars) |
| `TowerSlot` | position, isOccupied | — |
| `Tower` | towerName, type, buildCost, damage, range, fireRate, upgradeCost, sellPrice, currentLevel | upgrade(), fire(target) |
| `Projectile` | type, flySpeed, damage, explosionRadius | hit(target) |
| `Wave` | waveIndex, enemyCount, spawnRate, difficultyMultiplier | spawnEnemy() |
| `Enemy` | enemyName, type, hp, moveSpeed, damageToBase, rewardResource, pathPosition | move(), attackBase() |
| `PowerUp` | powerUpName, type, resourceCost, cooldown, effectRadius, effectDuration | activate(targetPosition) |

> **Tóm tắt:** giữ nguyên 11 lớp thực thể + 7 lớp View đúng như báo cáo. Chỉ ba điều chỉnh nhỏ vì Unity: đổi `name` → `*Name` (tránh trùng `Object.name`), gợi ý đổi `status: boolean` → enum `GameStatus`, và gợi ý đổi các `type: String` → enum. Mọi tên còn lại bám sát 100% thiết kế.
