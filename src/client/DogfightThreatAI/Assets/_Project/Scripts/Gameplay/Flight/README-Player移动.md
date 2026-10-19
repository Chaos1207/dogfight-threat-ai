# Player 移动 —— 架构与落地说明

> 这份文档解释 `Scripts/Gameplay/Flight` 这一套代码为什么这么分层，以及在 Unity 里怎么把它跑起来。
>
> 立项书：《基于实时威胁评估与动态决策的飞机对战游戏》
>
> 引擎：Unity 2022.3.62f3c1 · 2D · 输入暂用 Legacy Input

---

## 一、四层结构（整个项目的样板间）

```
AircraftData   【配置数据 · ScriptableObject · 只读】
     │            Settings/Aircraft/Ship_*.asset
     ▼
FlightModel    【模拟层 Simulate · 纯 C# · 零 UnityEngine】
     ▲            Scripts/Gameplay/Flight/FlightModel.cs
     │                    │
FlightInput    【意图】   ▼    FlightState 【状态 · 纯 struct】
     ▲              （位置 / 速度 / 机头）
     │                    │
IPlayerInputSource ─► PlayerController ─► PlayerView 【表现层 View】
  KeyboardInputSource      （胶水）            Scripts/Gameplay/View/
```

**这一套跑通之后，武器、敌人、AI 全部照这个模式复制。**

| 层 | 文件 | 允许引用 | 禁止引用 |
|---|---|---|---|
| 配置数据 | `Core/Config/AircraftData.cs` | UnityEngine | — |
| 模拟层 | `Gameplay/Flight/FlightModel.cs` | `Core.Math`、`Core.Input`、`Core.Config` | **UnityEngine 一个字都不能有** |
| 表现层 | `Gameplay/View/PlayerView.cs` | UnityEngine | 禁止访问 `FlightModel` |
| 胶水 | `Gameplay/Flight/PlayerController.cs` | 全部 | 禁止写飞行数学、禁止写绘制 |

---

## 二、Unity 里的落地步骤

### 步骤 1：建飞机档案（`Settings/Aircraft/`）

1. 在 `Assets/_Project/Settings/Aircraft/` 右键 → `Create` → `DogfightThreatAI` → `Ship Definition`
2. 命名 `Ship_Ship01`
3. 选中它，填：

| 字段 | 值 |
|---|---|
| Display Name | `试作机 01` |
| Body Sprite | `Art/Characters/Third_Party_Ships/ship_0001` |
| Min Speed | `2` |
| Max Speed | `9` |
| Acceleration | `18` |
| Turn Rate Deg Per Sec | `240` |
| Drag | `0.3` |

> 这组数是「街机空战」手感的起步值（推导见文档末尾「调参基线」）。
> 手感不对**只改这里，不改代码**。

> **做选角色界面就是重复这一步。** 再建 `Ship_Ship02`，Body Sprite 换 `ship_0002`，其他代码零改动。

### 步骤 2：改 `ship_0001.png` 的导入设置 ⚠️

现在这张图 `Pixels Per Unit = 100`，而它是 **32×32**，结果只有 0.32 个世界单位——小到几乎看不见。

| 字段 | 改成 |
|---|---|
| Pixels Per Unit | `32` |
| Filter Mode | `Point (no filter)` |
| Compression | `None` |
| Pivot | `Center` ⚠️ 必须 |

改完 Apply。**然后存成 Preset**（Inspector 右上角滑块图标 → Save current to...），以后所有像素素材一键套用。

### 步骤 3：建 Prefab（`Prefabs/Aircraft/`）

新建 Prefab `Player_Ship01`：

```
Player_Ship01  (根)
├─ Tag: Player        Layer: Player
├─ Rigidbody2D        ← 见步骤 4
├─ CircleCollider2D   ← Radius 0.5
├─ PlayerController   ← Ship: Ship_Ship01 ｜ Input Source: 本物体的 KeyboardInputSource
├─ KeyboardInputSource
└─ Art                (空物体)
   ├─ SpriteRenderer  ← 随便放个 sprite，运行时会自动换成 Ship 里的
   └─ PlayerView      ← Sprite Renderer 拖给它自己
```

> ⚠️ `PlayerView` 必须挂在**子物体 `Art`** 上，不要挂在根上——
> 因为 `PlayerView` 会写 `transform.position/rotation`，
> 挂在根上会和 `PlayerController` 的 `MovePosition` 抢同一个 Transform。

### 步骤 4：Rigidbody2D

| 字段 | 值 | 说明 |
|---|---|---|
| Body Type | `Kinematic` | 位置完全由代码说了算，物理引擎不插手 |
| Simulated | ✅ 勾选 | 取消勾选会连碰撞回调都没了 |
| Interpolate | `Interpolate` | 让 `MovePosition` 在显示帧之间平滑 |
| Collision Detection | `Continuous` | 飞机速度快，防穿透 |
| Gravity Scale | `0` | |
| Constraints | 勾 `Freeze Rotation Z` | ⚠️ 必须。旋转由 `PlayerView` 控制 |

### 步骤 5：场景 + 相机

1. 打开 `Scenes/Menu/Menu.unity`（等 `Battle.unity` 建好再搬）
2. 把 `Player_Ship01` Prefab 拖进场景（单个玩家，拖进来最方便调参）
3. Main Camera：`Orthographic`，`Size = 6`，`Position = (0,0,-10)`
4. 给 Main Camera 挂 `CameraFollow`，Target 拖场景里的玩家

> ⚠️ **不要把相机设为玩家的子物体。** 父子跟随会让相机跟着飞机旋转。

### 步骤 6：跑测试

`Window` → `General` → `Test Runner` → **EditMode** → `Run All`

应该看到 **4 个绿**：
- 油门拉满_速度不会超过上限
- 单步加速度受限制_所以有惯性
- 油门调低_速度会下降
- 相同输入_结果完全一致

**跑不起来先查 asmdef**：`Tests/EditMode/DogfightThreatAI.Tests.EditMode.asmdef` 的 `references` 必须含 `UnityEngine.TestRunner`、`UnityEditor.TestRunner`，`precompiledReferences` 必须含 `nunit.framework.dll`。

### 步骤 7：Play

**W** 加油门，**A / D** 转向。

---

## 三、验收标准

| # | 检查 | 怎么验 |
|---|---|---|
| 1 | 控制台零报错，`.csproj` 从 1 个变 4 个 | 看解决方案 |
| 2 | Test Runner 4 个绿 | 步骤 6 |
| 3 | 有惯性：按 W 后松手，飞机继续滑行 | 手感 |
| 4 | 有转向延迟：按 D 后机头慢慢转 | 手感 |
| 5 | 改 asset 立即生效（Play 中改 Max Speed 立刻变慢，不重编译） | `Ship_Ship01.asset` |
| 6 | `FlightModel.cs` 搜 `MonoBehaviour`/`transform`/`UnityEngine`/`Sprite` → 0 命中 | Ctrl+F |
| 7 | 换 `Ship` 引用 → 外观直接变 | 拖 `Ship_Ship02` 进去 |

**第 5、7 条证明"数据驱动"，第 6 条证明"以后写服务器不用返工"。**

---

## 四、已知问题

1. **Legacy Input 可能被禁用**：工程装了 `com.unity.inputsystem`。若 `Project Settings → Player → Active Input Handling` 是 `Input System Package (New)`，`KeyboardInputSource` 的 `Input.GetKey` 会运行时报错。改成 **`Both`**（需重启编辑器）。
2. **还没有边界**：飞机会飞出屏幕。加 `Boundary` 层碰撞体 + `OnTriggerEnter2D`，不要用坐标硬判断。
3. **Play 第一帧可能轻微闪一下**：`PlayerView` 首次 `Render` 时 `_previous` 是初始值，插值略偏。视觉不可见，联机版会换成正经的快照缓冲区。

---

## 五、以后联机要改哪里

**只改 `PlayerController.FixedUpdate` 的两三行**（从"本地模拟"改成"上报输入 + 接收快照"）。下面这些一行都不用动：

- `FlightModel` —— 原样搬进 `LogicServer`
- `FlightState` / `FlightInput` —— 直接当网络消息体
- `AircraftData` —— 服务器读同一份数值（所以它在 Core 层）
- `PlayerView` —— 只多一个"要不要插值"的分支
- `IPlayerInputSource` —— 多一个 `RemoteInputSource` 实现

**这就是"框架先搭好"的实际含义。**

---

## 六、调参基线（街机空战手感）

### 6.1 先说单位

设 `Pixels Per Unit = 32` 后：**1 个世界单位 = 1 个机身长**。

所以「最大速度 9」的物理含义是：**每秒飞过 9 个机身长**。
相机 `Orthographic Size = 6` 时，竖向能装 12 个机身，横向约 21 个 —— 屏幕宽度飞过去约 **2.3 秒**。

> **所有手感讨论都必须先对齐这个单位。** 不然"8 太快/太慢"没法判断。

### 6.2 四个参数怎么定

| 参数 | 值 | 直觉解释 | 改它会怎样 |
|---|---|---|---|
| **Max Speed** | `9` | 屏幕宽约 2.3 秒飞完 | 调大 → 更快但更难瞄准 |
| **Acceleration** | `18` | 从静止到满速约 `9/18 = 0.5 秒` | **这是" inertial 感"的主旋钮**。调到 9 → 1 秒到满速，明显更"重"；调到 36 → 0.25 秒，像街机 |
| **Turn Rate** | `240°/s` | 掉头 180° 需 `0.75 秒` | 调到 180 → 掉头 1 秒，更像"飞机"；调到 360 → 0.5 秒，更灵活 |
| **Drag** | `0.3` | 松油门后速度每秒衰减约 26% | 调到 0 → 永远不减速（太空感）；调到 1 → 松手很快停（车感） |

**判别自己调歪了的口诀：**

- 如果飞机**像鼠标光标**一样跟着按键走 → `Acceleration` 太大 / `Drag` 太大
- 如果飞机**像冰面上的冰壶**一样停不下来 → `Drag` 太小
- 如果**掉头要绕大圈** → `Turn Rate` 太小
- 如果**按一下就瞬间掉头** → `Turn Rate` 太大

### 6.3 关键设计：横向抓地力模型（漂移）

`FlightModel` 每步把速度**分解成两个轴**：

```
1. 机头转：heading += steer × turnRate × dt
2. 分解速度：forwardSpeed = 速度·机头方向    lateralSpeed = 速度·垂直机头方向
3. 前进分量：油门控制，按 acceleration 逼近目标   ← 加速/减速的惯性在这里
4. 横向分量：lateralSpeed ×= e^(−lateralGrip × dt)  ← ★ 漂移与收敛都在这一步
5. 漂移掉速：侧滑比例越大，前进极速越低 ×(1 − driftScrub × 侧滑比例)
6. 合成：速度 = 机头方向 × forwardSpeed + 垂直方向 × lateralSpeed
```

**为什么机头一转就会甩尾？**
机头旋转后，原来的速度在新的"机头坐标系"里**必然多出一个横向分量**——
这个分量不会立刻消失，而是被抓地力慢慢吃掉。这就是甩尾。

| `Lateral Grip` | 手感 | 甩尾角（240°/s 硬转时） |
|---|---|---|
| `20` | 几乎不甩，像**钉在轨道上**转圈 | ~12° |
| `10` | 轻微甩尾，接近传统飞行游戏 | ~23° |
| **`6`（默认）** | **明显甩尾，能看出机身在飘** | ~35° |
| `3` | 赛车式大甩尾 | ~54° |
| `1` | 几乎纯侧滑（冰面） | ~76° |

**`Drift Scrub`**（0~1，默认 0.35）：侧滑越厉害，前进极速掉得越多。
- `0` → 漂移不掉速（容易变成"无限速度漂移"，不推荐）
- `0.35` → 高速甩尾时约损失 10–15% 速度（推荐）
- `0.8` → 甩得越狠越慢（很"重"）

**手感排查表：**

| 症状 | 调什么 |
|---|---|
| 只画圆、完全没有甩尾（像被拴住） | `Lateral Grip` **调小**（6 → 3） |
| 甩出去后收不回来、像冰面 | `Lateral Grip` **调大**（6 → 10~15） |
| 漂移不掉速、贴脸乱窜 | `Drift Scrub` 调大（0.35 → 0.6） |
| 掉速太狠、一漂就失速 | `Drift Scrub` 调小（0.35 → 0.15） |

### 6.4 三个预设（直接抄）

| 预设 | Max Speed | Accel | Turn Rate | Drag | Lateral Grip | Drift Scrub | 适合 |
|---|---|---|---|---|---|---|---|
| **街机（默认）** | 9 | 18 | 240 | 0.3 | 6 | 0.35 | 主流手感，有甩尾但不失控 |
| **重机 / 轰炸机** | 7 | 10 | 150 | 0.5 | 3 | 0.5 | 厚重、甩得狠，AI 的"重装敌人" |
| **狙击机 / 轻战机** | 12 | 26 | 320 | 0.2 | 12 | 0.2 | 灵活、几乎不甩，AI 的"精英敌人" |

> ⚠️ 注意 **`Min Speed` 保持 2**，但因为油门 `0` 时目标是 `(0+1)/2 × 9 = 4.5`，速度不会真掉到 2 以下（下界只在极端减速时生效）。
> 如果想让"完全松手时明显减速"，把 `Drag` 提到 `0.8`。

### 6.5 怎么判断调对了

不要用眼睛盯着数字，用**三个可执行测试**：

1. **滑行测试**：满速后松手，飞机应该继续滑行**至少 2 秒**才开始明显减速
2. **掉头测试**：从静止开始，按 D 一直不放，机头转 180° 应该需要**约 0.7–0.8 秒**，而不是一瞬间
3. **手感对比测试**：把 `Turn Rate` 改成 `0`（不能转向）和 `1000`（瞬间转向），各玩 10 秒——**你应该能立刻感到差别**。如果你感觉不到，说明你的 `FlightModel` 没接对（或者 `PlayerView` 在覆盖旋转）

---

## 七、镜头与战场：两套方案

### 7.1 方案 A：固定镜头 + 有界战场（**当前采用**）

```
Main Camera：不挂 CameraFollow，位置 (0, 0, -10)，Orthographic Size = 5
场景：加一个 DebugStarfield 提供视觉参照
PlayerController：Arena Half Size = (8.5, 4.5)
```

- 战场就是"一屏"，飞机在屏幕内活动，越界会贴边
- 边界算在 `FlightModel` 里（`Bounds2D`），**服务器也判**，客户端改不了
- 威胁网格 = 这一屏，好做；多人同步量小，好做

### 7.2 方案 B：跟随镜头 + 无限世界

```
Main Camera：挂 CameraFollow，Look Ahead = 2.5
场景：必须有多层视差背景（否则飞机永远在屏幕中心，看起来"被拴住"）
PlayerController：Arena Half Size = (0, 0)  ← 关闭边界
```

- 飞机永远在画面里，世界向后滚动
- 没有背景/星点参照时，**看起来像原地不动**（这是跟随相机的固有特性，不是 bug）
- 需要滚动加载、视差层、更大的战场数据

### 7.3 怎么切换

| 想切到 | 操作 |
|---|---|
| 方案 B | `Main Camera` 加 `CameraFollow`（Target 拖玩家，Look Ahead 2.5）；`PlayerController` 的 `Arena Half Size` 改 `(0,0)`；场景必须有背景参照 |
| 方案 A | 删掉 `CameraFollow`；相机 `(0,0,-10)`；`Arena Half Size` 填 `(8.5, 4.5)`；加 `DebugStarfield` |

> 两套方案在代码层是共存的：`CameraFollow` 和 `Bounds2D` 都是可选项，互不冲突。

