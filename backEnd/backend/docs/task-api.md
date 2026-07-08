# 任务模块联调说明

当前任务后端已经按 `task_config + player_task` 运行，Unity 侧后续应以任务快照和任务事件为准，不再自行判定任务完成。

## 任务核心接口

- `GET /api/tasks`
  - 返回进行中、可接取、已完成、已领奖任务，以及当前主线和章节概览
- `GET /api/tasks/{taskCode}`
  - 返回单个任务详情
- `POST /api/tasks/{taskCode}/accept`
  - 手动接取 `AVAILABLE` 状态的任务
- `POST /api/tasks/{taskCode}/progress`
  - 针对某个任务直接推进进度
- `POST /api/tasks/{taskCode}/claim`
  - 领取任务奖励
- `GET /api/tasks/current/main`
  - 返回当前主线任务；如果主线全部完成则返回 `null`
- `GET /api/tasks/chapters`
  - 返回主线章节完成概览
- `POST /api/tasks/events`
  - 上传玩家行为事件，由后端决定哪些任务推进或触发

## 统一事件载荷

```json
{
  "eventType": "NPC_DIALOGUE",
  "targetType": "npc_dialogue",
  "targetId": 1003,
  "increment": 1,
  "currentPosX": 0,
  "currentPosY": 0,
  "currentPosZ": 0,
  "distanceToTarget": 0,
  "success": true,
  "extra": {}
}
```

说明：

- `eventType`：行为事件类型，必填
- `targetId`：目标对象 ID，例如 NPC、建筑、单车点位
- `increment`：自定义增量，主要用于单车试骑距离
- `currentPosX/Y/Z`：拍照打卡等需要上传当前位置时使用
- `distanceToTarget`：当前版本可传可不传，照片打卡以后端计算为准
- `success`：当前版本仅作兼容字段，照片打卡以后端判定为准

## 支持的 eventType

- `AI_DIALOGUE`
- `ARRIVE_BUILDING`
- `LANDMARK_VISIT`
- `NPC_DIALOGUE`
- `PHOTO_CHECKIN`
- `BIKE_STATION_VISIT`
- `BIKE_TRIAL_DISTANCE`

## 当前固定 ID 对照表

### NPC ID

- `1001` 迎新志愿者
- `1002` 迎新接待处志愿者
- `1003` 楼管阿姨
- `1004` 校医院护士

### 建筑 / 地标 ID

- `2001` 迎新接待处
- `2002` 竹苑3号楼
- `2003` 校医院
- `2004` 缙湖
- `2005` 植物园
- `2006` 图书馆
- `2007` 综合楼
- `2008` 第一教学楼
- `2009` 第一实验楼
- `2010` 艺术楼
- `2011` 钟楼
- `4001` 校园单车停放点

## 事件到任务的对应关系

| 玩家行为 | eventType | targetId | 推进 / 触发的任务 |
|---|---|---:|---|
| 首次与 AI 伙伴完成对话 | `AI_DIALOGUE` | 可不传 | `M_1_1` |
| 到达迎新接待处 | `ARRIVE_BUILDING` | `2001` | `M_1_2` |
| 与楼管阿姨对话 | `NPC_DIALOGUE` | `1003` | `M_1_3` |
| 与护士对话 | `NPC_DIALOGUE` | `1004` | `M_1_4` |
| 到达缙湖 | `ARRIVE_BUILDING` 或 `LANDMARK_VISIT` | `2004` | `M_2_1` |
| 到达植物园 | `ARRIVE_BUILDING` | `2005` | `M_2_2` |
| 到达图书馆 | `ARRIVE_BUILDING` | `2006` | `M_2_3` |
| 到达综合楼 | `ARRIVE_BUILDING` | `2007` | `M_3_1` |
| 到达第一教学楼 | `ARRIVE_BUILDING` | `2008` | `M_3_2` |
| 到达第一实验楼 | `ARRIVE_BUILDING` | `2009` | `M_3_3` |
| 到达艺术楼 | `ARRIVE_BUILDING` | `2010` | `M_3_4` |
| 首次访问任意地标 | `LANDMARK_VISIT` | 任意合法地标 ID | 触发 `B_1_1` |
| 在地标位置拍照打卡 | `PHOTO_CHECKIN` | 对应建筑 / 地标 ID | 推进 `B_1_1` |
| 首次经过单车停放点 | `BIKE_STATION_VISIT` | `4001` | 触发 `B_2_1` |
| 完成单车试骑距离 | `BIKE_TRIAL_DISTANCE` | `4001` | 推进 `B_2_1` |

## 当前已实现的任务流规则

- 新角色创建后，`M_1_1` 自动进入 `IN_PROGRESS`
- 主线任务领取奖励后，会自动解锁后续任务
- 第二章和第三章的起始任务在上一章完成后进入 `AVAILABLE`
- `B_1_1` 在 `M_1_3` 完成后首次地标访问时自动触发
- `B_2_1` 在 `B_1_1` 完成后首次经过单车停放点时自动触发
- `PHOTO_CHECKIN` 已改为后端根据 `building_location` 的标准坐标和半径自行判定成功
- 任务奖励会写入规范化玩家表：经验、纪念币、称号、单车解锁状态

## 联调建议

- Unity 不要自己判断任务是否完成，只上传行为事件
- 任务页面数据统一从 `GET /api/tasks` 和 `GET /api/tasks/current/main` 拉取
- 领奖统一调用 `POST /api/tasks/{taskCode}/claim`
- 拍照打卡事件请务必上传角色当前位置坐标，后端会自行判断是否命中地标
