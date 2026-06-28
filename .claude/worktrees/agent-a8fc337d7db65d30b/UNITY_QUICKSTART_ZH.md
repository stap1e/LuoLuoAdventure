# Unity 打开后逐步操作指南

适用版本：**Unity 2022.3.62f3 LTS**
项目根目录：`C:\Users\16025\PythonProjects\LuoLuoAdventure`

---

## 1. 在 Unity Hub 中打开项目

1. 打开 **Unity Hub**。
2. 确认已安装 **2022.3.62f3**。
3. 点击 **Open**。
4. 选择项目目录：`C:\Users\16025\PythonProjects\LuoLuoAdventure`。
5. 等待 Unity 完成首次导入、脚本编译、Package 恢复。

### 你应该看到什么
- 底部状态栏会经历：`Importing` / `Compiling Scripts` / `Reloading Domain`
- 左下角 Console 不应持续报红色编译错误
- 顶部菜单栏中应出现 **LuoLuoTrip** 菜单

---

## 2. 第一次打开后先做的检查

### 2.1 检查是否编译成功
1. 点击底部或顶部的 **Console** 窗口。
2. 看是否有红色错误。
3. 如果没有红色错误，可以继续下一步。

### 2.2 检查菜单是否出现
在 Unity 顶部菜单栏查找：

- `LuoLuoTrip`

如果能看到这个菜单，说明编辑器脚本已经加载成功。

如果看不到：
1. 回 Console 看是否有报错
2. 确认你用的是 **2022.3.62f3**
3. 等待脚本编译彻底结束后再看一次

---

## 3. 生成项目运行所需资产

这些步骤建议第一次都执行一遍。

### 3.1 生成阵营配置
顶部菜单点击：

- `LuoLuoTrip/Setup/Generate All Sub Faction Configs`

### 执行后会发生什么
会自动生成：
- `Assets/Data/Factions/` 下的子阵营配置
- `Assets/Data/Factions/SubFactionDatabase.asset`
- `Assets/Resources/SubFactionDatabase.asset`

### 你可以如何确认成功
在 **Project** 面板中查看：
- `Assets > Data > Factions`
- `Assets > Resources`

应能看到：
- 多个子阵营 `.asset`
- `SubFactionDatabase.asset`

---

### 3.2 生成受击反馈配置
顶部菜单点击：

- `LuoLuoTrip/Setup/Create Hit Feedback Profile`

### 执行后检查
在 **Project** 面板查看：
- `Assets > Data > HitFeedbackProfile.asset`

---

### 3.3 生成战斗动画配置
顶部菜单点击：

- `LuoLuoTrip/Setup/Create Combat Animator Config`

### 执行后检查
在 **Project** 面板查看：
- `Assets > Data > Animation > CombatAnimatorConfig.asset`

---

### 3.4 生成游戏配置
顶部菜单点击：

- `LuoLuoTrip/Setup/Create Game Config Asset`

### 执行后检查
在 **Project** 面板查看：
- `Assets > Data > GameConfig.asset`

---

## 4. 生成可直接运行的原型场景

这是最推荐的使用方式。

顶部菜单点击：

- `LuoLuoTrip/Setup/Create Combat Prototype Scene`

### 这个菜单会自动做什么
它会自动：
1. 生成阵营配置
2. 创建新场景
3. 创建 `GameBootstrap` 对象
4. 挂上世界初始化与存档组件
5. 创建地面
6. 创建一个玩家角色
7. 创建一个敌人角色
8. 给玩家挂战斗动画桥接
9. 保存场景

### 生成后的场景位置
在 **Project** 面板查看：
- `Assets > Scenes > CombatPrototype.unity`

---

## 5. 打开并运行战斗原型场景

### 5.1 打开场景
1. 在 **Project** 面板进入：`Assets > Scenes`
2. 双击：`CombatPrototype.unity`

### 5.2 检查 Hierarchy
打开场景后，**Hierarchy** 大概率会看到这些对象：
- `Main Camera`
- `Directional Light`
- `GameBootstrap`
- `Ground`
- `Player`
- `Enemy_Beast`
- `CombatHUD`

如果对象名略有差异，以场景生成结果为准。

### 5.3 点击运行
点击 Unity 顶部中间的 **Play** 按钮。

---

## 6. 运行时怎么操作

进入 Play Mode 后，使用以下按键：

- `W / A / S / D`：移动
- `鼠标左键`：攻击
- `Space`：闪避
- `Q`：锁定最近敌人
- `Tab`：切换锁定目标
- `F5`：快速存档
- `F9`：快速读档

---

## 7. 如何确认系统真的在工作

### 7.1 看玩家能否移动
进入 Play 后按 `WASD`：
- 玩家胶囊体应该会移动
- 朝向会跟随移动方向改变

### 7.2 看能否攻击敌人
让玩家接近敌人后点击鼠标左键：
- 会触发攻击
- 敌人应受到伤害或受击反馈

### 7.3 看能否锁定
按 `Q`：
- 应锁定一个敌人
- Scene 视图中可能看到黄色连线（Gizmos 开启时）

按 `Tab`：
- 如果有多个可锁定敌人，会切换目标

### 7.4 看 HUD 是否显示
屏幕左上区域应看到调试 HUD，包含：
- HP
- ST
- Poise
- State

### 7.5 看 Console 输出
运行后如果 `_logInitialization` 开启，会在 Console 里看到世界初始化日志，例如：
- 阵营领袖
- 成员数量
- 阵营关系示例

---

## 8. 如何测试存档读档

### 8.1 存档
1. 进入 Play 模式
2. 移动玩家到一个新位置
3. 按 `F5`
4. Console 应打印类似：
   - `[Save] 快速存档完成 (F5)`

### 8.2 读档
1. 继续移动到另一个位置
2. 按 `F9`
3. 玩家应回到存档时的位置
4. 战斗状态也会按存档恢复

### 8.3 自动读档说明
如果 `GameBootstrap` 同物体上的 `SaveLoadManager` 开启了自动读档：
- 再次运行时，会尝试读取最近存档

---

## 9. 如果你想从零手动搭一个场景

如果你不想用自动生成场景，也可以手动搭。

### 9.1 新建场景
1. `File > New Scene`
2. 选择默认空场景或默认带相机灯光的模板
3. 保存到：`Assets/Scenes/你的场景名.unity`

### 9.2 创建启动对象
1. 在 Hierarchy 空白处右键
2. 选择：`Create Empty`
3. 命名为：`GameBootstrap`
4. 在 Inspector 点击 **Add Component**
5. 添加以下组件：
   - `GameBootstrap`
   - `SaveLoadManager`

### 9.3 确保阵营数据库已生成
先执行过一次：
- `LuoLuoTrip/Setup/Generate All Sub Faction Configs`

否则世界初始化会缺少配置资产。

### 9.4 创建玩家角色
你至少需要给玩家对象添加：
- `CharacterEntity`
- `Combatant`
- `CombatController`

### 9.5 创建敌人角色
给敌人对象添加：
- `CharacterEntity`
- `Combatant`
- `SimpleCombatAI`

### 9.6 角色可视化
当前原型最简单的做法是直接用：
- `GameObject > 3D Object > Capsule`

然后把脚本挂到 Capsule 上。

---

## 10. 推荐的第一次试玩流程

建议你严格按这个顺序来：

1. 打开 Unity 项目
2. 等待编译完成
3. 确认顶部有 `LuoLuoTrip` 菜单
4. 点击：`LuoLuoTrip/Setup/Generate All Sub Faction Configs`
5. 点击：`LuoLuoTrip/Setup/Create Hit Feedback Profile`
6. 点击：`LuoLuoTrip/Setup/Create Combat Animator Config`
7. 点击：`LuoLuoTrip/Setup/Create Game Config Asset`
8. 点击：`LuoLuoTrip/Setup/Create Combat Prototype Scene`
9. 在 `Assets/Scenes` 中打开 `CombatPrototype.unity`
10. 点击 **Play**
11. 用 `WASD` 移动
12. 用鼠标左键攻击
13. 用 `Q` 锁定
14. 用 `F5` 存档
15. 用 `F9` 读档

---

## 11. 常见问题排查

### 问题 1：看不到 LuoLuoTrip 菜单
检查：
1. Console 是否有红色错误
2. 是否使用 Unity 2022.3.62f3
3. 是否还在编译中

### 问题 2：点了生成菜单但没看到资源
检查位置：
- `Assets/Data/Factions`
- `Assets/Resources`
- `Assets/Data/Animation`
- `Assets/Scenes`

### 问题 3：按 Play 后角色不动
检查：
1. 当前打开的是不是 `CombatPrototype.unity`
2. Hierarchy 里是否有 `Player`
3. `Player` 上是否有 `CombatController`
4. Game 视图是否聚焦，按键是否真的输入到 Game 窗口

### 问题 4：F5/F9 没反应
检查：
1. `GameBootstrap` 对象上是否挂了 `SaveLoadManager`
2. 当前是否在 Play 模式
3. Console 是否有 `[Save]` 日志

### 问题 5：敌人不攻击或不受击
检查：
1. `Enemy_Beast` 是否带 `SimpleCombatAI`
2. `Player` 和 `Enemy_Beast` 是否都带 `Combatant`
3. 两者距离是否过远

---

## 12. 这个仓库当前最适合的使用方式

当前更适合这样用：
- 作为 **Unity 战斗原型项目** 运行
- 验证角色数值和阵营关系
- 验证基础存档读档
- 在现有原型上继续扩展 UI、关卡、敌人、动画和正式玩法

它目前不是完整内容型游戏，而是一个可运行的系统原型底座。

---

## 13. 你下一步最值得做的事

如果你已经能跑起来，建议下一步做其中一个：

1. 增加更多敌人，测试 `Q/Tab` 锁定切换
2. 调整 `CombatStats` 相关参数，观察战斗手感
3. 新建正式场景，把原型角色放进去
4. 给角色替换真实模型与 Animator
5. 扩展 UI，而不只是调试 HUD

---

## 14. 相关关键代码位置

如果你想边看边改，优先看这些文件：

- 世界初始化：[GameBootstrap.cs](Assets/Scripts/Game/GameBootstrap.cs)
- 存档管理：[SaveLoadManager.cs](Assets/Scripts/Save/SaveLoadManager.cs)
- 存档读写：[SaveService.cs](Assets/Scripts/Save/SaveService.cs)
- 玩家控制：[CombatController.cs](Assets/Scripts/Combat/CombatController.cs)
- 战斗主体：[Combatant.cs](Assets/Scripts/Combat/Combatant.cs)
- 敌人 AI：[SimpleCombatAI.cs](Assets/Scripts/Combat/SimpleCombatAI.cs)
- 编辑器一键搭建：[LuoLuoTripSetupMenu.cs](Assets/Scripts/Editor/LuoLuoTripSetupMenu.cs)

---

如果你愿意，下一步我可以继续帮你再补一份：
- **“Hierarchy 里每个对象该长什么样”检查清单**
- 或 **“每个 Inspector 字段怎么配”超细版**
