# Dogfight Threat AI

一个 2D 空战小游戏与技术验证项目，重点探索实时威胁评估与动态决策的敌方 AI。

项目规模较小，目标不是复刻商业作品，而是验证两件事：

- 飞行手感：惯性、转向阻尼、漂移收敛与边界约束。
- 敌方 AI：将战场量化为威胁网格，并据此在巡逻、搜索、追击、攻击、规避之间切换。

AI 设计参考顽皮狗在敌人行为与战斗节奏上的思路，但不复刻其实现。本项目与 Naughty Dog 无隶属或合作关系。

## 主要特性

- 2D 俯视空战，飞行参数由 ScriptableObject 配置，增加机型不需要修改代码。
- 模拟层与表现层分离：模拟代码不依赖 UnityEngine，可在测试与逻辑服务器中复用。
- 实时威胁网格：按队伍维护，10 Hz 更新，供 AI 路径规划与调试热力图使用。
- AI 决策：行为树与 BFS/A* 路径规划，后续加入多目标效用评估。
- 武器：机炮命中扫描与直线导弹；追踪导弹列为后续目标。
- 联机：服务器权威与客户端快照插值，规划中。

## 技术栈

| 项 | 内容 |
|---|---|
| 引擎 | Unity 2022.3 LTS |
| 语言 | C# |
| 渲染 | 2D，Built-in Render Pipeline |
| 测试 | Unity Test Framework，EditMode |
| 网络 | 规划中，客户端—网关—逻辑服务器三层结构 |

## 项目状态

项目处于早期开发阶段。飞行模型、逻辑网格与配置分层已有实现和单元测试；威胁网格、AI 决策、武器与联机功能正在推进。

当前重点：

1. 完成核心战斗闭环：飞行、机炮、导弹、碰撞与伤害。
2. 实现威胁网格与 AI 行为切换。
3. 接入服务器权威与客户端同步。

## 快速开始

1. 安装 Unity 2022.3.62f3c1。
2. 使用 Unity Hub 打开 `src/client/DogfightThreatAI`。
3. 打开 `Assets/_Project/Scenes/Battle/Battle.unity`。
4. 点击 Play。

操作：W/S 控制油门，A/D 控制转向。

运行测试：

1. 打开 `Window → General → Test Runner`。
2. 选择 EditMode。
3. 执行 Run All。当前共有 16 条测试。

## 仓库结构

    src/client/DogfightThreatAI   Unity 客户端
    src/server/GatewayServer      网关服务器（规划中）
    src/server/LogicServer        逻辑服务器（规划中）
    src/shared/Protocol           前后端共享协议（规划中）
    docs                          技术文档
    art-source                    原始美术素材，不直接导入 Unity

## 文档

- [文档索引](docs/INDEX.md)
- [架构约定](docs/02-design/架构约定.md)
- [Player 移动说明](docs/02-design/Player移动说明.md)
- [项目分工](docs/00-project/分工.md)

## 第三方素材

- [Kenney Pixel Shmup](https://kenney.nl/assets/pixel-shmup)，CC0 1.0。
- 素材来源与授权记录见 `art-source/third-party/kenney-pixel-shmup/NOTES.md`。

## 许可

项目代码暂未声明开源许可证。第三方素材遵循其原始许可。
