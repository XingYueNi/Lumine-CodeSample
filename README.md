# Lumine: Aut Vincere Aut Mori — 代码切片

只狼向第三人称动作 Demo 的 **C# 代码部分**（Unity 2022.3 + URP）。
完整工程为私有仓库，原因是美术 / 模型 / 音频为第三方素材，版权不属于我，无法公开。

> 想快速判断水平的话，直接看 **「五个关键实现」** —— 每个都标了文件与行号。

---

## 技术栈

| 项 | 版本 / 说明 |
|---|---|
| Unity | 2022.3 LTS |
| 渲染 | URP |
| 输入 | 传统 Input Manager |
| 相机 | Cinemachine（自由 / 锁定 / 处决运镜三套虚拟相机） |
| UI | UGUI + TextMeshPro |
| 规模 | 69 个脚本 / 约 6,500 行（`Assets/Scripts`，不含插件） |

---

## 三十秒代码导览

| 你想看什么 | 去哪看 |
|---|---|
| 攻击判定为什么用扫掠 | `PlayerManager.cs:179`、`Enemy.cs:838` |
| 操作窗口怎么和动画对齐 | `CharacterCombat.cs:82`、`189`、`202` |
| 弹反的窗口与方向判定 | `CharacterCombat.cs:79`、`PlayerManager.cs:30` |
| 架势 / 破韧 / 处决 | `CharacterCombat.cs:58`、`175` |
| 顿帧 | `GameManager.cs:117` |
| 存档的原子替换 | `SaveDataManager.cs:373` |
| 锁定镜头 | `LockLookAtTarget.cs:43`、`PlayerControl.cs:18` |
| UI 框架 | `Manager/UiManager.cs`、`UI/BasePanel.cs` |

---

## 五个关键实现

### 1. 攻击判定：从碰撞箱到代码侧扫掠

**位置**：`PlayerManager.cs:179` / `Enemy.cs:838`

```csharp
RaycastHit[] hits = Physics.SphereCastAll(from, rayRadius, direction.normalized, distance, targetLayer);
```

**为什么改**：最早判定是武器上挂碰撞箱，快速挥砍时目标可能落在两帧之间，碰撞箱直接跳过 → **丢伤害**。
中间试过把判定挂到挥砍特效的碰撞体上，解决了丢伤害，但**判定与表现耦合**（改特效就改手感），最终改成每帧从刀尖上一帧位置向当前帧做 `SphereCastAll`（半径 0.1），等于给刀画了一条连续扫掠线，判定不再依赖任何特效物体。

### 2. 操作窗口用动画事件定义，不用计时器

**位置**：`CharacterCombat.cs:82-83`、`189`、`202`

```csharp
protected bool isAttackWindowOpen;      // 攻击判定窗口
public bool isCanControl = true;        // 是否允许操作
```

**为什么**：动画一改，计时器就飘。用 Animator 的动画事件在指定帧打开 / 关闭窗口，状态边界永远和画面一致。
两个窗口**并行但独立**：`isCanControl`（可操作）与 `isAttackWindowOpen`（攻击判定）分开，避免"后摇里也能弹反"这类越权操作。

### 3. 弹反：时间窗 + 方向判定

**位置**：`CharacterCombat.cs:79-81`、`PlayerManager.cs:30-32`

- 完美弹反窗口 `parryWindowDuration = 0.5s`
- 方向判定：受击点转到角色**局部坐标**，以 `parryHeightPivot = 1.1f` 为上下分界，侧向留 `0.05` 死区（正中命中时沿用上次方向，避免动画来回抖）
- 成功后触发顿帧

### 4. 生命 + 架势双资源

**位置**：`CharacterCombat.cs:58-59`、`175`

```csharp
private float maxPosture = 100;
private float postureRecoverySpeed = 10;   // 每秒恢复
```

**设计意图**：只有血量的话，防守没有收益。加架势之后，格挡变成**主动行为**——挡住会涨对方架势，打满就破韧，破韧后 4 秒内可处决。
格挡按倍率减伤（`CombatStats` 里配），并按倍率涨架势。

### 5. 存档：写入不破坏旧档

**位置**：`SaveDataManager.cs:373-384`

```csharp
string tempPath = path + ".tmp";
string backupPath = path + ".bak";

File.WriteAllText(tempPath, json);   // 先写临时文件
File.Move(path, backupPath);         // 旧档转备份
File.Move(tempPath, path);           // 临时文件转正
```

**为什么**：直接覆盖写，写到一半崩溃会得到半截 JSON，读档直接崩。
这套 `.tmp → .bak → Move` 保证任一时刻磁盘上都有一份完整存档；读档时若主文件缺失会从 `.bak` 恢复。
另外写盘走 `Task.Run`（`SaveDataManager.cs:235`），避免主线程卡顿。

---

## 其他值得一提的

| 内容 | 位置 | 一句话 |
|---|---|---|
| 敌人 AI 警戒值 | `Enemy.cs` | 0~100 连续量，按「角度加权 × 距离加权」累积，非布尔 |
| 锁定镜头 | `LockLookAtTarget.cs:43` | 相机看向玩家与敌人的**中点**，兼顾看敌人起手和自己的走位 |
| 顿帧 | `GameManager.cs:117` | `timeScale` 0.4 → 1（处决用 0.2） |
| 异步场景加载 | `GameManager.cs:63` | `progress / 0.9f` 归一化（Unity 加载到 0.9 就停了） |
| UI 框架 | `Manager/UiManager.cs` | 单例 + 三字典（路径 / 预制体缓存 / 已打开实例）+ `BasePanel` 基类 |
| 背包排序 | `PackageManager.cs` | `IComparer` 三级排序：星级 > id > 等级 |

---

## 目录结构

```
Assets/Scripts/
├── Base/        CharacterCombat.cs         角色战斗基类（窗口、架势、破韧）
├── Manager/     PlayerManager / Enemy? / SaveDataManager / GameManager / UiManager
├── Player/      PlayerControl（三套相机）、PlayerTakeDamage
├── Enemy/       Enemy.cs（AI + 战斗，893 行）
├── FSM/         有限状态机（巡逻 / 警戒 / 追击 / 攻击 / 弹反 / 死亡）
├── UI/          16 个界面脚本（背包 MVC、设置、暂停、存档格）
├── Effect/      ParryFlash（程序化火花）、SwordTrail、ExecutionMarkerUI
├── Config/      ScriptableObject 配置（CombatStats / AttackData）
└── Pugins/      SpringBone 等小工具
```

---

## 项目背景（三代演进）

| 版本 | 判定方式 | 遇到的问题 |
|---|---|---|
| v1 | 武器挂**碰撞箱** | 快速挥砍时目标位于两帧之间 → 丢伤害 |
| v2 | 判定挂到**挥砍特效**碰撞体 | 解决丢伤害、扩大近战范围；但判定与表现耦合，重演出稀释了核心玩法 |
| v3（当前） | **代码侧 SphereCast 扫掠** | 删减非核心演出，重心转向系统层（UI / 背包 / 存档 / 性能 / 热更） |

项目原名《基于 Unity 的 ARPG 战斗系统设计与实现》，第三版明确目标是"只狼式的冷兵器近距离交锋、刀刀有反馈"。

---

## 已知不足

| 问题 | 说明 | 打算怎么改 |
|---|---|---|
| `Enemy.cs` 893 行 | AI / 战斗 / 表现职责混在一起 | 按"决策 / 执行 / 表现"拆分 |
| UI 无面板栈 | 跨面板切换要调用方手动关旧面板 | 加 `Stack<BasePanel>`，ESC 关栈顶 |
| 背包列表全量重建 | 每次打开清空再 `Instantiate` 全部格 | 对象池 + 虚拟滚动 |

---

## 关于素材（重要）

本仓库**只包含 C# 代码**，不含任何美术、模型、音频资源——这些资源版权不属于我：

- 角色 / 敌人模型与动画：第三方
- 场景美术：第三方
- 图标：商店资源（授权允许用于产品，不允许再分发原始文件）
- 音乐 / 音效：第三方

因此代码可以公开，完整工程不能。

如需查看完整工程（含场景与预制体），可以按需开放私有仓库的只读权限。

---

## 联系

XingYueLi ｜ Unity 客户端开发（ARPG / 类魂动作方向）
演示视频：https://www.bilibili.com/video/BV1aRWK6AEcN/?vd_source=446bcdd6512ea7fee2710734fade7f23
