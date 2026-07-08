# MyCampus Backend

Spring Boot 3 backend MVP for the Unity campus game.

## What is included

- JWT auth: register, login, send-code, reset-password
- Player profile and role management
- Aggregated home endpoint
- Growth, tasks, sign-in, quiz, clock-in, story, campus QA endpoints
- Persistent player state via Spring Data JPA
- Game config loading from an external folder or the packaged classpath data under `game-config`

## Run

1. Install JDK 17+
2. Install Maven 3.9+
3. Start with the default local database:

```bash
mvn spring-boot:run
```

The default profile uses file-based H2 for quick MVP development.

To switch to MySQL:

```bash
mvn spring-boot:run -Dspring-boot.run.profiles=mysql
```

To run a production-style deployment:

```bash
SPRING_PROFILES_ACTIVE=prod java -jar target/mycampus-backend-0.0.1-SNAPSHOT.jar
```

Deployment variables and server notes are documented in:

- `docs/deploy.md`

Frontend integration notes are documented in:

- `docs/frontend-api-guide.md`
- `docs/unity-frontend-file-api-map.md`

## Main endpoints

- `POST /api/auth/send-code`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/player/me`
- `POST /api/player/roles`
- `GET /api/game/home`
- `GET /api/tasks`
- `POST /api/tasks/events`
- `GET /api/signin`
- `GET /api/activities/quiz/current`
- `POST /api/campus/qa`

Task event payload examples and the current seeded NPC/building ids are documented in:

- `docs/task-api.md`

## Task backend manual checks

For task-chain verification without Unity, use:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\task-manual-check.ps1 -Step full
```

Supported steps:

- `m1_1`
- `chapter1`
- `branch1`
- `full`
- `negative`

## Notes

- Production mode refuses to start if it still uses the development JWT secret or an H2 datasource.
- Packaged builds include the Unity JSON gameplay config, and `APP_GAME_DATA_ROOT` can override it on the server.
