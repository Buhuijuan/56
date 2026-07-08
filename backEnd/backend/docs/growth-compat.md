# GrowthSystem 整合说明

`Temp/GrowthSystem` 原来的独立成长后端已经并入当前主后端，不再需要单独启动第二个 Spring Boot 服务。

当前主后端已经兼容旧 GrowthSystem 的接口协议，后续所有成长相关调用都应直接指向当前主后端地址。

## 兼容接口

- `POST /api/profile/init`
- `GET /api/profile/growth/{roleId}`
- `GET /api/profile/snapshot/{roleId}`
- `POST /api/task/accept`
- `POST /api/task/progress`
- `POST /api/task/claim`

## 接入约定

- 当前主后端是唯一真值源。
- 不再保留或启动 `Temp/GrowthSystem` 独立服务。
- 调用方需要携带当前主后端登录得到的 token。
- `roleId` 必须与当前登录账号的 `currentRoleId` 一致。

## 说明

这层兼容接口底层复用了当前主后端已有的：

- 玩家档案
- 等级/经验
- 任务状态
- 奖励发放

因此后续不应再维护第二套成长数据库或第二套任务状态。
