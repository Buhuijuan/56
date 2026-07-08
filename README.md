# 此间方寸 / PocketCampus

PocketCampus 是一个面向校园场景的 Unity 互动应用项目。项目由 Unity 前端、Spring Boot 后端和数据库脚本三部分组成，围绕校园漫游、角色成长、任务、签到、打卡、答题、称号、剧情和 AI 问答等功能构建。

## 项目结构

```text
PocketCampus/
├── frontEnd/                 # Unity 前端核心脚本与资源片段
├── backEnd/
│   └── backend/              # Spring Boot 后端服务
├── sql/                      # MySQL 数据库表结构与初始化脚本
├── .gitignore
└── README.md
```

说明：`frontEnd` 目录是前端核心内容，不是完整 Unity 工程根目录；它更像一组可放入 Unity `Assets` 下的脚本、配置和 `.meta` 资源索引。导入 Unity 时请保留 `.meta` 文件。

## 技术栈

前端：

- Unity
- C#
- Unity UI / EventSystem
- CharacterController
- UnityWebRequest
- PlayerPrefs

后端：

- Java 17
- Spring Boot 3.3.4
- Maven
- Spring Web
- Spring Security
- Spring Data JPA
- Spring Validation
- Spring Mail
- JWT
- H2 / MySQL 驱动

数据：

- MySQL 建表脚本位于 `sql/`
- 后端开发和测试中也包含 H2 依赖

## 功能模块

### Unity 前端

`frontEnd` 主要负责客户端表现、玩家操作和 UI 交互：

- 角色移动：虚拟摇杆移动、加速、步行/骑行切换。
- 镜头控制：第三人称右摇杆环绕镜头、俯仰限制、平滑跟随、碰撞修正。
- 地图系统：小地图玩家点位映射。
- 宠物系统：宠物跟随玩家、宠物点击交互。
- 登录和账号界面：登录、注册、找回密码、角色管理。
- 游戏 UI：任务、签到、成长、等级、称号、天气、活动、地图、个人中心、设置。
- 数据层：账号、角色、任务、奖励、签到、活动、称号等前端模型和状态。
- 后端通信：通过 `BackendFacade` 和 `ApiClient` 调用后端接口。
- AI 交互：校园问答和剧情故事接口调用。

前端关键目录：

```text
frontEnd/
├── Camera/       # 第三人称镜头
├── Joy/          # 虚拟摇杆
├── Map/          # 小地图
├── Pet/          # 宠物跟随
├── Player/       # 玩家移动、生成、模型切换
└── UI/Scripts/
    ├── AI/       # AI 问答、剧情接口
    ├── Audio/    # BGM、SFX、UI 音效
    ├── Backend/  # 后端配置、HTTP 客户端、DTO、状态映射
    ├── Data/     # 配置、枚举、模型、状态
    ├── Managers/ # UI、场景、音频、图片、拍照、传送管理
    ├── Systems/  # 登录、账号、角色、任务、成长、签到、称号等业务系统
    └── UI/       # 具体页面和控件
```

### Spring Boot 后端

`backEnd/backend` 提供 Unity 客户端调用的业务接口：

- 认证：验证码、注册、登录、重置密码、JWT 鉴权。
- 玩家：账号信息、角色创建、角色切换、角色删除、头像上传。
- 首页聚合：返回当前角色、等级、成长、任务、签到、称号等概览。
- 任务：任务列表、章节、当前主线、接受任务、提交事件、领取奖励。
- 成长与等级：成长阶段、等级奖励、称号领取。
- 签到：每日签到、在线时长、累计登录奖励。
- 活动：答题、打卡、剧情记录。
- AI 问答：校园问答接口。
- 数据持久化：JPA Entity / Repository 管理玩家状态。
- 游戏配置：从打包资源或外部目录加载 JSON 游戏配置。

后端关键目录：

```text
backEnd/backend/
├── pom.xml
├── src/main/java/com/mycampus/backend/
│   ├── api/          # Controller 与 DTO
│   ├── auth/         # 账号与验证码实体、仓库
│   ├── security/     # JWT 与 Spring Security
│   ├── player/       # 玩家、角色、资产相关实体
│   ├── progression/  # 等级、成长、称号状态
│   ├── signin/       # 签到状态
│   ├── activity/     # 答题、打卡、剧情活动状态
│   ├── task/         # 任务、打卡记录、建筑位置
│   ├── game/         # 游戏配置加载
│   ├── mail/         # 邮件服务
│   ├── service/      # 业务服务
│   └── config/       # 安全、资源、生产检查等配置
├── src/main/resources/
│   ├── application.yml
│   ├── application-mysql.yml
│   └── application-prod.yml
├── docs/
├── scripts/
└── data/             # 本地数据库文件目录
```

### SQL 脚本

`sql` 目录包含 MySQL 表结构和配置表脚本，覆盖账号、角色、学校、任务、成长、等级、签到、活动、称号、奖励、剧情、打卡等数据表。

常见脚本类型：

- `campus_game_account.sql`：账号表。
- `campus_game_role.sql`：角色表。
- `campus_game_school.sql`：学校表。
- `campus_game_task_config.sql`：任务配置。
- `campus_game_player_task.sql`：玩家任务状态。
- `campus_game_growth_stage_config.sql`：成长阶段配置。
- `campus_game_level_config.sql`：等级配置。
- `campus_game_daily_award_config.sql`、`campus_game_online_award_config.sql`、`campus_game_total_award_config.sql`：签到奖励配置。
- `campus_game_quiz_*`：答题活动配置与玩家状态。
- `campus_game_clockin_*`：打卡活动配置与玩家状态。
- `campus_game_story_*`：剧情活动配置与记录。
- `campus_game_title_config.sql`：称号配置。

## 启动项目

建议启动顺序：

1. 准备数据库。
2. 启动后端服务。
3. 在 Unity 中打开或导入前端内容。
4. 将前端后端地址指向本地后端。
5. 运行 Unity 场景进行联调。

## 环境要求

### 后端环境

- JDK 17 或更高版本
- Maven 3.9 或更高版本
- MySQL 8.x，推荐用于完整联调
- PowerShell / CMD / 终端

检查命令：

```powershell
java -version
mvn -version
mysql --version
```

### 前端环境

- Unity Editor
- 可正常打开项目资源的 Unity 版本
- 已配置 EventSystem、Canvas、场景、Prefab、Resources 配置等 Unity 资源

当前仓库中的 `frontEnd` 不是完整 Unity 工程根目录，因此如果你手上已有完整 Unity 工程，请把 `frontEnd` 内容放入对应的 `Assets` 目录或项目已有前端脚本目录中。

## 启动后端：MySQL 模式

这是推荐的完整联调方式。

### 1. 创建数据库

登录 MySQL 后创建数据库：

```sql
CREATE DATABASE mycampus
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;
```

### 2. 导入 SQL 脚本

可以使用 MySQL 客户端、Navicat、DataGrip 等工具，将 `sql/` 目录下的脚本导入到 `mycampus` 数据库。

命令行示例：

```powershell
mysql -u root -p mycampus < sql\campus_game_account.sql
mysql -u root -p mycampus < sql\campus_game_school.sql
mysql -u root -p mycampus < sql\campus_game_role.sql
```

实际导入时建议将 `sql/` 下所有需要的建表和配置脚本导入。若出现外键或依赖顺序问题，先导入账号、学校、角色、基础配置表，再导入玩家状态和活动记录表。

### 3. 配置后端环境变量

进入后端目录：

```powershell
cd backEnd\backend
```

PowerShell 示例：

```powershell
$env:SERVER_PORT = "8080"
$env:APP_DB_URL = "jdbc:mysql://127.0.0.1:3306/mycampus?useSSL=false&serverTimezone=Asia/Shanghai&characterEncoding=utf8&allowPublicKeyRetrieval=true"
$env:APP_DB_USERNAME = "root"
$env:APP_DB_PASSWORD = "你的数据库密码"
$env:APP_JWT_SECRET = "replace-with-a-long-random-secret-for-local-dev"
$env:APP_MAIL_ENABLED = "false"
$env:APP_MAIL_MOCK_DELIVERY = "true"
```

如果需要真实邮箱验证码，再配置：

```powershell
$env:MAIL_HOST = "smtp.example.com"
$env:MAIL_PORT = "465"
$env:MAIL_USERNAME = "no-reply@example.com"
$env:MAIL_PASSWORD = "邮箱授权码或密码"
$env:MAIL_FROM_ADDRESS = "no-reply@example.com"
$env:MAIL_FROM_NAME = "MyCampus"
$env:APP_MAIL_ENABLED = "true"
$env:APP_MAIL_MOCK_DELIVERY = "false"
```

### 4. 启动后端

```powershell
mvn spring-boot:run
```

启动成功后，默认服务地址为：

```text
http://localhost:8080
```

健康检查：

```text
http://localhost:8080/actuator/health
```

## 启动后端：本地 H2 快速模式

后端依赖中包含 H2，适合快速开发或临时演示。当前 `application.yml` 中数据库 URL、用户名和密码来自环境变量，并且默认 driver 是 MySQL；因此使用 H2 时需要显式覆盖 driver。

PowerShell 示例：

```powershell
cd backEnd\backend

$env:SERVER_PORT = "8080"
$env:APP_DB_URL = "jdbc:h2:file:./data/mycampus;MODE=MySQL;DATABASE_TO_LOWER=TRUE;AUTO_SERVER=TRUE"
$env:APP_DB_USERNAME = "sa"
$env:APP_DB_PASSWORD = ""
$env:SPRING_DATASOURCE_DRIVER_CLASS_NAME = "org.h2.Driver"
$env:SPRING_H2_CONSOLE_ENABLED = "true"
$env:APP_JWT_SECRET = "replace-with-a-long-random-secret-for-local-dev"
$env:APP_MAIL_ENABLED = "false"
$env:APP_MAIL_MOCK_DELIVERY = "true"

mvn spring-boot:run
```

H2 控制台：

```text
http://localhost:8080/h2-console
```

连接信息：

```text
JDBC URL: jdbc:h2:file:./data/mycampus;MODE=MySQL;DATABASE_TO_LOWER=TRUE;AUTO_SERVER=TRUE
User: sa
Password: 留空
```

## 构建后端 Jar

进入后端目录：

```powershell
cd backEnd\backend
```

构建：

```powershell
mvn clean package
```

运行 Jar：

```powershell
java -jar target\mycampus-backend-0.0.1-SNAPSHOT.jar
```

生产模式示例：

```powershell
$env:SPRING_PROFILES_ACTIVE = "prod"
$env:SERVER_PORT = "8080"
$env:APP_DB_URL = "jdbc:mysql://127.0.0.1:3306/mycampus?useSSL=false&serverTimezone=Asia/Shanghai&characterEncoding=utf8&allowPublicKeyRetrieval=true"
$env:APP_DB_USERNAME = "mycampus_app"
$env:APP_DB_PASSWORD = "你的生产数据库密码"
$env:APP_JWT_SECRET = "足够长且随机的生产 JWT 密钥"
$env:APP_JPA_DDL_AUTO = "validate"

java -jar target\mycampus-backend-0.0.1-SNAPSHOT.jar
```

生产模式会阻止明显不安全的配置，例如使用开发 JWT 密钥、H2 数据源或开启 H2 控制台。

## 启动前端 Unity

### 1. 导入前端目录

将 `frontEnd` 目录内容放入 Unity 工程的 `Assets` 目录或对应模块目录中。

必须保留：

- `.meta` 文件
- 脚本目录结构
- Resources 下的 JSON 配置资源
- Prefab、场景和 Inspector 引用

### 2. 检查场景绑定

至少需要确认以下对象或组件已经绑定：

- `ControlBtnManager.moveJoystick`
- `ControlBtnManager.viewJoystick`
- `PlayerSpawner.spawnPoint`
- 玩家角色 prefab 中的 `PlayerAgentMove`
- 玩家角色 prefab 中的 `PlayerModelSwitcher`
- 摄像机上的 `CameraOrbitByJoystick`
- 宠物对象上的 `PetFollow`
- 玩家模型内的 `CameraPivot`
- 玩家模型内的 `PetFollowPoint`
- 小地图 UI 上的 `MinimapPlayerDot`

### 3. 配置前端后端地址

前端后端基础地址位于：

```text
frontEnd/UI/Scripts/Backend/BackendSettings.cs
```

当前默认值：

```csharp
DefaultBaseUrl = "http://47.109.31.215:8080"
```

本地联调时建议改为：

```text
http://localhost:8080
```

项目中也存在服务器切换 UI 和 `PlayerPrefs` 配置逻辑，运行时修改后会保存到本地。

### 4. 配置 AI 地址

AI 问答和剧情接口地址分别写在：

```text
frontEnd/UI/Scripts/AI/APIManager.cs
frontEnd/UI/Scripts/AI/StoryAPIManager.cs
```

当前默认值：

```csharp
baseUrl = "http://47.109.31.215:8000"
```

如果只联调 Spring Boot 后端，后端也提供了校园问答相关接口；如果使用独立 AI 服务，请确保 `8000` 端口服务可访问。

### 5. 运行 Unity

打开对应启动场景，点击 Play。

推荐验证流程：

1. 登录或注册账号。
2. 创建或选择角色。
3. 进入校园场景。
4. 测试摇杆移动、镜头旋转、小地图、宠物跟随。
5. 打开任务、签到、成长、称号、活动界面。
6. 观察 Unity Console 和后端日志是否有异常。

## 前后端接口关系

前端主要通过以下文件访问后端：

```text
frontEnd/UI/Scripts/Backend/BackendSettings.cs
frontEnd/UI/Scripts/Backend/ApiClient.cs
frontEnd/UI/Scripts/Backend/BackendFacade.cs
frontEnd/UI/Scripts/Backend/BackendDtos.cs
frontEnd/UI/Scripts/Backend/BackendStateMapper.cs
```

常用接口：

```text
POST /api/auth/send-code
POST /api/auth/register
POST /api/auth/login
POST /api/auth/reset-password
GET  /api/player/me
POST /api/player/roles
PUT  /api/player/current-role
GET  /api/game/home
GET  /api/tasks
GET  /api/tasks/chapters
GET  /api/tasks/current/main
POST /api/tasks/{taskCode}/accept
POST /api/tasks/{taskCode}/claim
POST /api/tasks/events
GET  /api/signin
POST /api/signin/daily
POST /api/player/heartbeat
GET  /api/activities/quiz/current
POST /api/activities/quiz/start
POST /api/activities/quiz/submit
GET  /api/activities/clockin/current
POST /api/activities/clockin/{locationId}/check
POST /api/campus/qa
```

更详细的接口说明见：

```text
backEnd/backend/docs/task-api.md
backEnd/backend/docs/growth-compat.md
backEnd/backend/docs/前端文件和接口对照说明.md
backEnd/backend/docs/前端调用接口说明和举例.md
```

## 配置项说明

### 后端核心环境变量

| 变量名 | 说明 | 示例 |
| --- | --- | --- |
| `SERVER_PORT` | 后端端口 | `8080` |
| `APP_DB_URL` | JDBC 数据库连接 | `jdbc:mysql://127.0.0.1:3306/mycampus?...` |
| `APP_DB_USERNAME` | 数据库用户名 | `root` |
| `APP_DB_PASSWORD` | 数据库密码 | `password` |
| `APP_JWT_SECRET` | JWT 签名密钥 | 本地可用长随机字符串 |
| `APP_JPA_DDL_AUTO` | Hibernate DDL 策略 | `update` / `validate` |
| `APP_MAIL_ENABLED` | 是否启用真实邮件 | `true` / `false` |
| `APP_MAIL_MOCK_DELIVERY` | 是否模拟发送邮件 | `true` / `false` |
| `APP_GAME_DATA_ROOT` | 外部游戏配置目录 | `D:\config\game-config` |

### 邮件环境变量

| 变量名 | 说明 |
| --- | --- |
| `MAIL_HOST` | SMTP 服务器 |
| `MAIL_PORT` | SMTP 端口 |
| `MAIL_USERNAME` | 邮箱账号 |
| `MAIL_PASSWORD` | 邮箱密码或授权码 |
| `MAIL_FROM_ADDRESS` | 发件地址 |
| `MAIL_FROM_NAME` | 发件名称 |

## 游戏配置加载

后端会加载游戏配置 JSON：

- 默认从 classpath 的 `game-config` 读取。
- `pom.xml` 会尝试把 `../Assets/Resources/Jsons` 下的 JSON 打包到 `game-config`。
- 也可以通过 `APP_GAME_DATA_ROOT` 指定服务器上的外部配置目录。

前端也会通过 Unity 的 `Resources.Load<TextAsset>("Jsons/...")` 读取配置，因此前后端配置字段需要保持一致。

## 开发建议

- 前端 `.meta` 文件必须提交，不能随意删除。
- 修改后端 DTO 后，要同步检查 Unity 的 `BackendDtos.cs` 和 `BackendStateMapper.cs`。
- 修改任务、签到、成长、称号等配置后，要同时考虑 SQL 配置、后端游戏配置和前端 Resources JSON。
- 本地联调优先使用 `http://localhost:8080`，避免误连线上服务。
- 后端生产环境必须设置强 JWT 密钥，并关闭 H2 控制台。
- 如果 Unity 中出现中文注释乱码，建议统一使用 UTF-8 编码保存脚本。

## 常见问题

### 后端启动时报 `APP_DB_URL` 相关错误

当前 `application.yml` 中数据库连接来自环境变量，需要先设置：

```powershell
$env:APP_DB_URL = "jdbc:mysql://127.0.0.1:3306/mycampus?useSSL=false&serverTimezone=Asia/Shanghai&characterEncoding=utf8&allowPublicKeyRetrieval=true"
$env:APP_DB_USERNAME = "root"
$env:APP_DB_PASSWORD = "你的数据库密码"
```

然后再执行：

```powershell
mvn spring-boot:run
```

### Unity 登录失败

检查：

- 后端是否已启动。
- Unity 中 `BackendSettings.BaseUrl` 是否是 `http://localhost:8080` 或正确服务器地址。
- 后端日志中是否有 401、403、500 等错误。
- 注册验证码是否配置了真实邮件或 mock 邮件。

### 前端请求线上地址而不是本地地址

默认地址写在 `BackendSettings.cs` 中，也可能已经被 `PlayerPrefs` 保存过。可以在游戏内服务器切换 UI 修改，或清理 Unity PlayerPrefs 后重新运行。

### H2 控制台打不开

确认启动前设置了：

```powershell
$env:SPRING_H2_CONSOLE_ENABLED = "true"
$env:SPRING_DATASOURCE_DRIVER_CLASS_NAME = "org.h2.Driver"
```

并访问：

```text
http://localhost:8080/h2-console
```

### 角色无法移动

检查：

- 场景中是否有 `ControlBtnManager`。
- `moveJoystick` 是否绑定。
- 玩家 prefab 是否带有 `CharacterController`。
- 当前玩家是否挂载 `PlayerAgentMove`。

### 镜头无法旋转

检查：

- `viewJoystick` 是否绑定。
- 摄像机是否挂载 `CameraOrbitByJoystick`。
- `CameraOrbitByJoystick.target` 是否绑定到当前玩家。
- 玩家模型中是否存在 `CameraPivot`。

### 宠物不跟随

检查：

- 宠物对象是否挂载 `PetFollow`。
- `followTarget` 是否指向玩家模型内的 `PetFollowPoint`。
- 角色切换后 `PlayerSpawner` 是否重新绑定宠物目标。

## 参考文档

- `frontEnd/README.md`：前端模块说明。
- `backEnd/backend/README.md`：后端模块说明。
- `backEnd/backend/docs/task-api.md`：任务接口和任务事件说明。
- `backEnd/backend/docs/growth-compat.md`：成长兼容接口说明。
- `backEnd/backend/docs/前端文件和接口对照说明.md`：前端文件与接口对照。
- `backEnd/backend/docs/前端调用接口说明和举例.md`：前端调用示例。
