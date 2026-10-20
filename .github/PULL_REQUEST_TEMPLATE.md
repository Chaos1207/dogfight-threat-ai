<!--
PR 模板。提交 Pull Request 时 GitHub 会自动填入以下内容。
请完成自查后再请求 review。
-->

## 本 PR 的内容

<!-- 用一两句话说明改了什么，避免“更新了一些东西”这类描述。 -->

Closes #

## 改动类型

- [ ] `feat` 新功能
- [ ] `fix` 修复问题
- [ ] `docs` 文档
- [ ] `art` 素材
- [ ] `refactor` 重构
- [ ] `test` 测试
- [ ] `chore` / `build` / `ci` 其他

## 影响范围

- [ ] 客户端（Unity）
- [ ] GatewayServer
- [ ] LogicServer
- [ ] 共享协议 `src/shared/Protocol/`
- [ ] 文档
- [ ] 素材
- [ ] 仓库配置（`.gitignore` / `.gitattributes` / `.editorconfig` / `.github/`）

## 自查

### 通用

- [ ] 切换或合并分支前已关闭 Unity
- [ ] 拉取 `main` 后本地能编译、能运行
- [ ] 未提交 `Library/`、`Temp/`、`Logs/`、`UserSettings/`、`obj/`、`bin/`、`Build/`
- [ ] 未提交密钥、token、账号或本机绝对路径
- [ ] 未夹带与本次改动无关的文件

### 涉及 Unity

- [ ] 新增或删除的资源，其 `.meta` 文件在同一提交中
- [ ] 未与他人同时修改同一个 `.unity` 或 `.prefab`
- [ ] 修改 `ProjectSettings/`（Layers、Tags、物理、画质、Packages）后已在团队内同步

### 涉及协议

- [ ] 修改 `src/shared/Protocol/` 后，客户端与服务端双方均已确认
- [ ] 若为破坏性改动（删除字段或改变语义），已在下方说明需要同步修改的一端
- [ ] `*.schema.json` 已通过 JSON Schema 语法校验
- [ ] 协议变更已记录在协议文档中（文档建立后执行）

### 涉及文档

- [ ] 文档中的相对路径链接已检查，无断链
- [ ] 文档末尾的变更记录已更新
- [ ] 图表同时提交源文件（`.drawio`）与导出图（`.png`）

### 涉及素材

- [ ] 已在素材台账中登记来源与授权（台账建立后执行）
- [ ] 命名符合美术规范（规范建立后执行）
- [ ] 第三方素材保留 `License.txt`；如项目已建立 `THIRD-PARTY-NOTICES.md`，则同步更新
- [ ] 单文件未超过 50 MB；超过时已在团队内说明

## 破坏性改动说明

<!--
若改动协议、ProjectSettings 或他人依赖的接口，请说明哪一端需要同步修改什么。
没有则写“无”。
-->

无

## 截图或验证证据

<!-- UI、表现、联机效果等改动，附截图或日志。 -->

## 给 reviewer 的说明

<!-- 希望 reviewer 重点检查的位置，或尚不确定的地方。 -->
