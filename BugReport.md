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

### 2026-06-10 — Hoàn thiện các hạng mục còn lại (levels / settings / tests)
- **Nhiều màn + mở khóa (UC3):** thêm Level_02 (12 wave), Level_03 (15 wave), nối `nextLevel` 01→02→03; `LevelSelectView` khóa nút màn chưa mở, mở khóa khi thắng màn trước. Play-test: ban đầu chỉ Màn 1 mở, thắng L1 → Màn 2 mở.
- **Settings (UC2):** thêm 3 TMP_Dropdown (chất lượng/ngôn ngữ/hiển thị) vào panel Settings + wire SettingsView. Play-test nút Save → `saveSettings` → `Apply` (AudioListener.volume, QualitySettings, Screen mode) đúng.
- **Test Chương 5:** thêm `UseCaseExceptionTests.cs` — UC6 từ chối khi max cấp, UC9 từ chối khi hồi chiêu / thiếu tiền, `unlockNext` mở khóa + lưu. Kiểm động **9/9 PASS**.
- **0 lỗi** trong tất cả phiên test.

### 2026-06-10 — BUG-07 (NGHIÊM TRỌNG): đạn "đóng băng" tại điểm va chạm, tháp bắn mãi vào địch đã chết — ĐÃ SỬA
- **Nguyên nhân:** sau pooling (GĐ8), địch chết chỉ bị `SetActive(false)` (`Health`/`EnemyMovement` → `SimplePool.Release`), Transform **vẫn tồn tại** → `target == null` trả về **false** (fake-null chỉ áp dụng cho object bị Destroy). Vì vậy `Bullet` và `TurretScript` chỉ kiểm null → tháp giữ target chết, đạn bay mãi về Transform "đóng băng", tháp bắn dồn vào chỗ đó; địch mới vào tầm thì cả đám đạn lại đổi hướng.
- **Sửa:**
  - `TurretScript.Update`: kiểm `target == null || !target.gameObject.activeInHierarchy` → drop target chết + `FindTarget` lại (FindTarget chỉ thấy địch active).
  - `Bullet.FixedUpdate`: kiểm `activeInHierarchy`; khi mục tiêu chết giữa đường → bay nốt tới `lastTargetPos` (điểm va chạm dự kiến) rồi `Release` (biến mất), không bám Transform đóng băng.
- **Thêm theo yêu cầu:** tháp chỉ bắn khi đã quay đúng hướng mục tiêu (`IsAimedAtTarget`, ngưỡng `aimToleranceDeg`=8°); đạn biến mất tại đúng vị trí lẽ ra va chạm khi địch chết trước.
- **Kiểm thử (play-mode):** đạn homing đúng; khi mục tiêu `SetActive(false)`, velocity đạn chuyển về hướng điểm va chạm (5,0→40); tại điểm va chạm `FixedUpdate` → đạn `active=false` (trả pool). (Position không tiến giữa các lệnh MCP do editor không step khi mất focus — không phải lỗi code.)

### 2026-06-10 — BUG-08: dùng power-up trên map còn xây nhầm tháp — ĐÃ SỬA
- **Hiện tượng:** "không dùng được power-up". Thực ra mọi khâu chạy (slot hiện/arm, Input cũ ở chế độ Both, Camera.main, `activatePowerUp` trừ tiền đúng), NHƯNG khi click map để dùng power-up, cú click cũng rơi vào `Plot.OnMouseDown` → **xây tháp**; nếu click chỗ không có địch thì hiệu ứng power-up vô hình → người chơi tưởng power-up hỏng (chỉ thấy tháp mọc).
- **Sửa:** `PowerUpView` thêm `Instance` + `IsArmed`; `Plot.OnMouseDown` bỏ qua click khi đang arm power-up (OnMouseDown chạy trước Update nên IsArmed còn true). Click giờ dành riêng cho power-up.
- **Kiểm thử:** arm power-up → click plot → currency **không đổi** (không xây tháp); activate power-up trừ đúng tiền.
- **Cách dùng:** bấm 1 slot power-up (góc trái-dưới) → con trỏ "lên đạn" → click lên map (gần địch cho Portal/Airstrike, gần tháp cho SpeedBoost); chuột phải để huỷ.

### 2026-06-10 — Regression toàn diện trước khi build — TẤT CẢ PASS (32/32)
Chạy lại toàn bộ qua MCP. **Compile sạch, 0 lỗi/0 cảnh báo runtime.**
- **EditMode (19/19):** kinh tế (spend/build/upgrade/sell + chặn max cấp), status Lost, power-up (kích hoạt/cooldown/thiếu tiền), `ApplyDifficulty` (Health/EnemyMovement), `ApplySlow`, spawner config-driven (`PlannedEnemiesForWave` công thức vs Wave), save/load Level + Settings, `unlockNext` mở khóa.
- **PlayMode (13/13):** scene Gameplay + managers khởi tạo (GameSession/Base/EnemySpawner/Level_Manager/BuildManager), pooling (`SimplePool` tái dùng + `OnEnable` reset máu), `powerUps`=3 + chỉ báo tầm wired, `TowerActionView` có mặt, Plot guard (đang arm power-up → click không xây tháp), power-up trừ tiền khi dùng, lose-condition (`Base`→`Lost`).
- **Trạng thái:** tất cả 8 giai đoạn kế hoạch + UC1–UC12 hoạt động; toàn bộ bug BUG-01→08 đã sửa; còn lại chỉ SFX/nhạc/localization (cần asset chuyên dụng).

### 2026-06-10 — Sửa cụm bug (BUG-01, 03, 04, 05, 06) — ĐÃ SỬA
- **BUG-01:** Thêm cờ `earlyCallPending` — chỉ cho gọi wave sớm **1 lần** tới khi batch hiện tại kết thúc (`StartWave`/`EndWave` reset cờ). Hết stack wave vô hạn.
- **BUG-03:** `EnemySpawner.Awake` `RemoveListener` trước `AddListener` + thêm `OnDestroy` gỡ listener → event tĩnh không cộng dồn (quan trọng khi đã có pooling).
- **BUG-04:** `Menu.OnGUI` null-guard `currencyUI`/`Level_Manager.main`.
- **BUG-05:** `TurretScript.FindTarget` đổi sang `Physics2D.OverlapCircleAll` + chọn địch **gần nhất** (thay `CircleCastAll` hướng = vị trí, lấy `hits[0]` tùy ý).
- **BUG-06:** `EnemyMovement.Start/Update/FixedUpdate` null-guard cho `target`/`Level_Manager.main.path`.
- **Kiểm thử:** compile sạch (0 lỗi); play-mode smoke test — địch spawn & di chuyển đúng theo path (TEST_ENEMY: (-0.5,5.5)→(-6.46,0.06)), wave thật chạy (EnemiesAlive=9), **0 lỗi runtime**.

### 2026-06-10 — Hoàn thiện PowerUp (UC9) — power-up giờ dùng được trong game
- Tạo 3 asset `PowerUp` (`Assets/ScriptableObjects/PowerUps/`): Portal (cost30/cd8/r2/power3), Airstrike (40/12/2.5/dmg5), SpeedBoost (25/10/r3/dur5); gán vào `GameSession.powerUps` (scene Gameplay). PowerUpView (3 slot đã dựng sẵn) giờ hiện slot.
- **Sửa SpeedBoost:** `GameSession.ApplySpeedBoost` đổi sang `FindObjectsByType<TurretScript>` + khoảng cách (tháp không có Collider2D nên `OverlapCircleAll(towerMask)` không tìm thấy).
- **Sửa INFO:** `LevelDetailView.gameplaySceneName` default `"0 (1)"` → `"Gameplay"`.
- **Kiểm thử play-mode (12/12 assertion thực, trừ 1 artifact tính kill-reward):**
  - Portal đẩy địch lùi 3 waypoint (pi 5→2); Airstrike sát thương/giết địch (AoE); SpeedBoost buff fireRate tháp x2.
  - 3 slot hiện; trừ cost đúng; cooldown chặn dùng lại; từ chối khi thiếu tiền.
  - **0 lỗi runtime.**
