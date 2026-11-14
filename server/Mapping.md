# Packet Mapping

Game-agnostic enumerations used by both packet types. The translation from the raw game values
is implemented in `Mapping.cs`.

## `game`

| Int Value | Value | Notes |
|---|---|---|
| `0` | `Unknown` | |
| `1` | `AssettoCorsa` | shared memory |
| `2` | `AssettoCorsaCompetizione` | shared memory |
| `3` | `F1_2020` | UDP, format 2020 |
| `4` | `BeamNGDrive` | reserved, not implemented |
| `5` | `RaceRoom` | reserved, not implemented |

## `gameState`

Represents the current state of the game.

| Int Value | Value | Description | AC / ACC | F1 2020 |
|---|---|---|---|---|
| `0` | `Off` | Game is off / in a menu | `status = 0` | never (no packet is produced) |
| `1` | `Replay` | Game is showing a replay | `status = 1` | never (not reported by the game) |
| `2` | `Live` | Game is live | `status = 2` | `m_gamePaused = 0` |
| `3` | `Paused` | Game is paused | `status = 3` | `m_gamePaused = 1` |

## `sessionType`

| Int Value | Value | AC / ACC | F1 2020 `m_sessionType` |
|---|---|---|---|
| `-1` | `Unknown` | `-1` | `0` |
| `0` | `Practice` | `0` | `1` P1, `2` P2, `3` P3, `4` short practice |
| `1` | `Qualify` | `1` | `5` Q1, `6` Q2, `7` Q3, `8` short Q, `9` one-shot Q |
| `2` | `Race` | `2` | `10` race, `11` race 2 |
| `3` | `Hotlap` | `3` | – |
| `4` | `TimeAttack` | `4` | `12` time trial |
| `5` | `Drift` | `5` | – |
| `6` | `Drag` | `6` | – |
| `7` | `Hotstint` | `7` (ACC only) | – |
| `8` | `Superpole` | `8` (ACC only) | – |

## `flag`

| Int Value | Value | Description | AC | ACC | F1 2020 |
|---|---|---|---|---|---|
| `0` | `NoFlag` | No flag | `0` | `0` | `m_vehicleFiaFlags` `0` / `-1` |
| `1` | `Green` | Green flag | – | `7` | `1` |
| `2` | `Yellow` | Yellow flag | `2` | `2` | `3` |
| `3` | `WavedYellow` | Waved yellow flag | – | – | – |
| `4` | `DoubleYellow` | Double yellow flag | – | – | – |
| `5` | `White` | White flag | `4` | `4` | – |
| `6` | `Checkered` | Checkered flag | `5` | `5` | – |
| `7` | `Blue` | Blue flag | `1` | `1` | `2` |
| `8` | `WavedBlue` | Blue waved flag | – | – | – |
| `9` | `Black` | Black flag | `3`, `6` (penalty) | `3`, `6` (penalty) | – |
| `10` | `OrangeBlack` | Black flag with orange circle | – | `8` | – |
| `11` | `BlackWhite` | Black and white flag | – | – | – |
| `12` | `YellowRed` | Yellow and red striped flag | – | – | – |
| `13` | `GreenYellow` | Yellow and green diagonal flag | – | – | – |
| `14` | `SafetyCar` | Safety car | – | – | `m_safetyCarStatus = 1` |
| `15` | `VirtualSafetyCar` | Virtual safety car | – | – | `m_safetyCarStatus = 2` |
| `16` | `Code60` | Code 60 | – | – | – |
| `17` | `FullCourseYellow` | Full course yellow | – | – | – |
| `18` | `Red` | Red flag | – | – | `4` |

F1 2020: a safety car or virtual safety car period takes precedence over the per-vehicle flag.
Value `18` used to be a duplicate `FCY` entry and is now `Red`.

## ACC specific values (full packet only)

`penalty` – `ACC_PENALTY_TYPE`: `0` none, `1` drive-through (cutting), `2`/`3`/`4` stop&go 10/20/30 s (cutting),
`5` DSQ (cutting), `6` remove best lap (cutting), `7` drive-through (pit speeding), `8`/`9`/`10` stop&go 10/20/30 s
(pit speeding), `11` DSQ (pit speeding), `12` remove best lap (pit speeding), `13` DSQ ignored mandatory pit,
`14` post-race time, `15` DSQ trolling, `16` DSQ pit entry, `17` DSQ pit exit, `18` DSQ wrong way,
`19` drive-through ignored driver stint, `20` DSQ ignored driver stint, `21` DSQ exceeded driver stint limit.

`trackGripStatus` – `0` green, `1` fast, `2` optimum, `3` greasy, `4` damp, `5` wet, `6` flooded.

`rainIntensity*` – `0` no rain, `1` drizzle, `2` light rain, `3` medium rain, `4` heavy rain, `5` thunderstorm.

## F1 2020 specific values (full packet only)

`weather` – `0` clear, `1` light cloud, `2` overcast, `3` light rain, `4` heavy rain, `5` storm.

`safetyCarStatus` – `0` none, `1` full safety car, `2` virtual safety car, `3` formation lap.

`driverStatus` – `0` in garage, `1` flying lap, `2` in lap, `3` out lap, `4` on track.
