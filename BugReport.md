# Báo cáo Bug & Kiểm thử — Quantall TD

> Ngày: 2026-06-09 · Kiểm thử qua Unity MCP (kiểm động trực tiếp) + test chính thức trong `Assets/Tests/`.
> Trạng thái build: **0 lỗi compile**. Kiểm động logic lõi: **29/29 PASS**.

## 1. Cách kiểm thử

- **Kiểm động qua MCP:** chạy script trong Editor, dựng đối tượng và gọi method thật, kiểm 29 assertion (kinh tế, lose-condition, upgrade/sell, Settings, Level). Kết quả: 29/29 pass.
- **Test chính thức (`Assets/Tests/`):**
  - `EditMode/EconomyTests.cs` — kinh tế & quản lý tháp (UC5/6/7), không cần Awake.
  - `EditMode/DataLayerTests.cs` — `Tower`, `Settings`, `Level`, `Wave`.
  - `PlayMode/LifecyclePlayModeTests.cs` — `Base` (Awake init, takeDamage), tích hợp Base→GameSession Lost, `sellTower` (cần Destroy/play mode).
  - Chạy: **Window ▸ General ▸ Test Runner ▸ EditMode/PlayMode ▸ Run All**.

## 2. Bug & vấn đề phát hiện

Mức độ: 🔴 cao · 🟡 trung bình · 🟢 thấp · ⚪ thông tin/chưa hiện thực.

### 🟡 BUG-01 — `CallNextWaveEarly` có thể bỏ qua cổng 80% và stack wave vô hạn
- **File:** `EnemySpawner.cs` (`CallNextWaveEarly`).
- **Hiện tượng:** Sau khi gọi wave sớm, số địch còn sót của wave trước vẫn nằm trong `enemiesLeftToSpawn`. Khi chúng sinh ra, `spawnedInCurrentWave++` lại tính vào wave MỚI, nên ngưỡng 80% đạt gần như tức thì → người chơi bấm gọi sớm liên tục, chồng nhiều wave không kiểm soát.
- **Đề xuất:** Tách bộ đếm "đã sinh của riêng wave hiện tại" khỏi hàng chờ gộp, hoặc khóa nút sau mỗi lần gọi cho tới khi wave mới thực sự bắt đầu sinh.

### ✅ BUG-02 — Không có điều kiện THẮNG (wave vô hạn) — ĐÃ SỬA
- **File:** `EnemySpawner.cs` (`EndWave`).
- **Sửa:** Thêm `totalWaves` (mặc định 10). Khi `currentWave >= totalWaves` và đã dọn sạch → gọi `GameSession.OnAllWavesCleared()` (→ `Won`), không sinh thêm wave. `ResultView` hiện panel "CHIẾN THẮNG".
- **Còn lại:** Sau này nên lấy `totalWaves` từ `Level.waveCount` thay vì hằng số (Giai đoạn 7).

### 🟡 BUG-03 — `onEnemyDestroy` là `static` + `AddListener` trong `Awake`
- **File:** `EnemySpawner.cs`.
- **Hiện tượng:** Event `static` không tự reset. Nếu **Enter Play Mode tắt Domain Reload** hoặc nạp lại scene, `AddListener` cộng dồn → `EnemyDestroyed` chạy nhiều lần → `enemiesAlive` giảm sai → wave kết thúc sớm / đếm lệch.
- **Đề xuất:** Trong `Awake`, gán lại `onEnemyDestroy = new UnityEvent()` hoặc `RemoveListener` trước khi `AddListener`.

### 🟢 BUG-04 — `Menu.OnGUI` đọc currency mỗi frame, không null-check
- **File:** `Menu.cs` (`OnGUI`).
- **Hiện tượng:** `Level_Manager.main.currency` không kiểm null → nếu scene thiếu `Level_Manager` sẽ spam `NullReferenceException` mỗi `OnGUI`.
- **Đề xuất:** Null-check; tốt hơn là bỏ `OnGUI`, cập nhật currency qua event khi thay đổi (đỡ tốn mỗi frame).

### 🟢 BUG-05 — `TurretScript.FindTarget` chọn mục tiêu chưa hợp lý
- **File:** `TurretScript.cs` (`FindTarget`).
- **Hiện tượng:** `Physics2D.CircleCastAll` truyền **hướng = `transform.position`** (dùng vị trí làm hướng) với khoảng cách 0, và lấy `hits[0]` thay vì kẻ địch ưu tiên (gần đích nhất / vào trước).
- **Đề xuất:** Dùng `Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask)` và chọn mục tiêu theo tiến độ đường đi.

### 🟢 BUG-06 — Thiếu null-guard cho `Level_Manager.main.path`
- **File:** `EnemyMovement.cs` (`Start`/`Update`).
- **Hiện tượng:** Nếu `path` rỗng/null → lỗi truy cập. (Tiền đề từ prototype.)
- **Đề xuất:** Kiểm `path != null && path.Length > 0` trước khi dùng.

### ⚪ INFO-07 — `Base` phải được gắn thủ công vào scene
- Nếu scene thiếu `Base`, địch tới cuối đường **biến mất mà không trừ máu** (do null-guard cố ý trong `EnemyMovement`). Cần wiring trong Editor: gắn `Base` vào GameObject cuối đường + đặt `maxHP`.

### ⚪ INFO-08 — Phụ thuộc thứ tự khởi tạo của currency
- `Level_Manager.currency` đặt ở `Start()` (=100); `GameSession.currentResources` đọc từ đó. Trong gameplay không sao (đọc lúc click), nhưng nếu code nào đọc currency trong `Start` có thể nhận 0. Cân nhắc khởi tạo currency trong `Awake`.

## 3. Đã sửa trong đợt kiểm thử này

- Gỡ `using` thừa: `Unity.VisualScripting` (`BuildManager`), `System.Runtime.CompilerServices`/`System.Xml.Serialization`/`UnityEngine.EventSystems` (`EnemySpawner`), `UnityEngine.UI`/`System` (`Menu`), `System`/`UnityEditor` (`TurretScript`).
- `TurretScript.OnDrawGizmosSelected`: đổi `Handles` (UnityEditor) → `Gizmos` (UnityEngine) — gỡ hẳn phụ thuộc assembly editor (xử lý dứt điểm cảnh báo CLAUDE.md về `using UnityEditor`).
- Đưa toàn bộ code game vào asmdef `Quantall.Runtime` để hỗ trợ test assembly.

## 4. Đã xác minh ĐÚNG (29/29 kiểm động)

- `Level_Manager`: `SpendCurrency` từ chối khi thiếu tiền / trừ đúng khi đủ; `IncreaseCurrency` cộng đúng.
- `GameSession`: status mặc định `Playing`; `currentResources` ủy quyền đúng; `OnBaseDestroyed`→`Lost`; `OnAllWavesCleared` không ghi đè được `Lost`.
- `buildTower`: null + không trừ tiền khi thiếu; instantiate + trừ đúng khi đủ.
- `upgradeTower`: thành công + trừ đúng `upgradeCost` cũ + tăng cost; từ chối khi thiếu tiền (không đổi cấp).
- `sellTower`: hoàn đúng `sellPrice`.
- `Base`: máu kẹp tại 0 (không âm); `onBaseDestroyed` bắn đúng 1 lần; takeDamage sau khi phá bị bỏ qua.
- `Settings.saveSettings`: trả `true` + lưu đủ field.
- `Level.saveResult`: chỉ cập nhật khi điểm/sao cao hơn.

## 5. Nhật ký kiểm thử theo phiên

### 2026-06-09 — Wiring scene + luồng menu (GĐ6) + tích hợp
- Wiring `_Recovery/0 (1)`: GameSession/Base/HUD/Result — **0 ref thiếu**; MainMenu: 5 view + EventSystem — **0 ref thiếu**.
- Play-test live: Base bị phá → `GameSession.status = Lost` (end-to-end).
- HUD cập nhật wave/score/máu; Result hiện "THẤT BẠI" khi thua.
- Luồng menu: Boot → MainMenu (tự chuyển); Play → Chọn màn → Màn 1 → Bắt đầu → **scene gameplay nạp, khởi tạo OK**.
- **Bug phát hiện & sửa:** `LevelDetailView`/`ConfirmExitView` tự gọi `panel.SetActive(false)` trong `Start` (view nằm trên chính panel → Start chạy sau `Show` rồi tắt nhầm). Đã bỏ dòng tự tắt; panel lưu inactive sẵn trong scene.

### 2026-06-09 — Giai đoạn 7 (Lưu trữ & tiến trình)
Kiểm động qua MCP + test chính thức (`Assets/Tests/EditMode/SaveTests.cs`). **Tất cả PASS, không phát hiện bug mới.**
- `SaveSystem` (PlayerPrefs): round-trip Level (unlocked/score/stars) + Settings — đúng.
- `Level.saveResult` → lưu thật; chỉ ghi đè khi điểm/sao cao hơn (kiểm qua nhiều instance).
- `Base.Initialize(maxHP)`: đặt đúng maxHP + currentHP.
- Tích hợp play-mode: chọn Level_01 (res=150, baseHP=25) → vào gameplay → **currency=150, Base.MaxHP=25** (ApplyLevel ghi đè default 100/20 nhờ `[DefaultExecutionOrder(100)]`).
- Thắng (đầy máu) → **stars=3**, `saveResult` lưu bestScore/bestStars đọc lại đúng.
- Console: **0 error / 0 warning**. (Đã dọn save data test `lvl_1`.)

### 2026-06-10 — Kiểm thử commit merge `8d99f7c` (pooling/PowerUp/TowerAction/settings — GĐ8)
Sau khi merge `origin/main`. Kiểm động qua MCP. **Code game compile sạch** (`Quantall.Runtime.dll`).

**Blocker môi trường (không phải code game):** package `com.ivanmurzak.unity.mcp@0.79.1` (tooling MCP của teammate, thêm vào `manifest.json` khi merge) thiếu DLL NuGet (`ReflectorNet`…) vì `Assets/Plugins/` không có trên máy này → **20 lỗi compile chặn play mode**. Đã tạm gỡ package khỏi `manifest.json` (local, không commit) để test, sau đó revert. **Cần restore package** (restart Unity để resolver tải DLL, hoặc lấy `Assets/Plugins` từ teammate) để dùng lại tooling đó.

**Kết quả (tất cả PASS):**
- Pooling: `SimplePool.Get/Release` tái dùng đúng instance; `Health`/`EnemyMovement` reset trạng thái qua `OnEnable` khi lấy lại từ pool (xác minh trong **play mode**; edit-mode không chạy OnEnable nên bỏ qua).
- `ApplyDifficulty`: máu/sát thương nhân theo độ khó dựa trên giá trị **gốc** (không cộng dồn).
- `EnemySpawner` config-driven: `PlannedEnemiesForWave` dùng `Wave.enemyCount` nếu có, nếu không theo công thức scaling; `SetLevel` đặt totalWaves.
- Scene đổi tên `Gameplay.unity`: build settings (Boot/MainMenu/Gameplay = 0/1/2) + `LevelDetailView.gameplaySceneName="Gameplay"` đúng; `GameFlow.Load("Gameplay")` chuyển scene OK.
- `ApplyLevel` post-merge: chọn Level_01 → gameplay **currency=150, Base.MaxHP=25** (session sạch).
- Managers init, lose-condition (`Base`→`Lost`) còn nguyên; `TowerActionView` & `PowerUpView` có trong scene; `TowerActionView.Show` chạy OK.
- Console runtime: **0 error**.

**Ghi nhận (INFO, chưa phải bug):**
- `GameSession.powerUps` rỗng → các slot `PowerUpView` ẩn → **power-up chưa dùng được trong game** cho tới khi tạo asset `PowerUp` (Portal/Airstrike/SpeedBoost) + gán vào `GameSession.powerUps`.
- `LevelDetailView.gameplaySceneName` default trong code vẫn là `"0 (1)"` (scene không còn tồn tại); chỉ giá trị serialized trong scene là `"Gameplay"`. Nên sửa default thành `"Gameplay"` để an toàn.
- Khi ván kết thúc, `ResultView` đặt `Time.timeScale=0`; chỉ reset về 1 khi bấm Restart/Menu. Bình thường OK, nhưng nếu thoát ván bằng đường khác cần đảm bảo reset timeScale.

**Bug cũ còn mở (commit merge KHÔNG sửa):** BUG-01 (cổng 80%), BUG-03 (`onEnemyDestroy` static + AddListener trong Awake), BUG-04/05/06.
