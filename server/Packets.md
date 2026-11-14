# Packets

USRTG produces two packet types:

* **`CommonPacket`** (`server/CommonPacket.cs`) – game-agnostic. It only contains data that
  **every** supported game can provide, normalised to one set of units and conventions.
  **This is the only packet the web server serves** (`GET /packet`).
* **`Packet`** (`server/Packet.cs`) – the full packet. It is a superset of the common packet and
  additionally carries game-specific data as nullable fields (omitted from JSON when null).
  Game handlers produce this packet; `CommonPacket.From(Packet)` projects the common subset.

## Conventions

| Topic | Convention |
|---|---|
| Wheel arrays | `[FL, FR, RL, RR]` (F1 2020 sends `[RL, RR, FL, FR]`; it is reordered) |
| Times | milliseconds (`int`), `sessionTimeLeft` milliseconds (`float`) |
| Temperatures | °C |
| Pressures | psi |
| Speed | km/h |
| Distances | metres |
| Angles | radians (`heading`, `pitch`, `roll`) |
| Gear | `-1` reverse, `0` neutral, `1..` forward (AC/ACC raw `0=R,1=N` is shifted by -1) |
| Pedals | `throttle`, `brake`, `clutch` 0..1, `steer` -1..1 |
| Enums | `gameState`, `sessionType`, `flag` use the values in `Mapping.md` |
| JSON keys | camelCase (`speedKmh`, `tyreCoreTemp`, …) |

## Common packet – field sources

| Field | Assetto Corsa | Assetto Corsa Competizione | F1 2020 | Notes |
|---|---|---|---|---|
| `game`, `gameName` | constant 1 | constant 2 | constant 3 | see `Enums.Game` |
| `id` | physics `packetId` | physics `packetId` | header `m_frameIdentifier` | monotonic counter |
| `timestamp` | server clock | server clock | server clock | Unix ms when assembled |
| `throttle` | physics `gas` | physics `gas` | telemetry `m_throttle` | 0..1 |
| `brake` | physics `brake` | physics `brake` | telemetry `m_brake` | 0..1 |
| `steer` | physics `steerAngle` | physics `steerAngle` | telemetry `m_steer` | -1..1 |
| `clutch` | physics `clutch` | physics `clutch` | telemetry `m_clutch` / 100 | 0..1 |
| `speedKmh` | physics `speedKmh` | physics `speedKmh` | telemetry `m_speed` | |
| `gear` | physics `gear` - 1 | physics `gear` - 1 | telemetry `m_gear` | |
| `rpms` | physics `rpms` | physics `rpms` | telemetry `m_engineRPM` | |
| `maxRpm` | static `maxRpm` | static `maxRpm` | status `m_maxRPM` | |
| `fuel` | physics `fuel` | physics `fuel` | status `m_fuelInTank` | **litres in AC/ACC, kilograms in F1** |
| `maxFuel` | static `maxFuel` | static `maxFuel` | status `m_fuelCapacity` | same unit caveat |
| `fuelPercent` | computed | computed | computed | unit independent |
| `pitLimiterOn` | physics `pitLimiterOn` | physics `pitLimiterOn` | status `m_pitLimiterStatus` | |
| `tc` | inferred level from physics `tc` (see below) | graphics `TC` | status `m_tractionControl` (0..2) | 0 = off |
| `abs` | inferred level from physics `abs` | graphics `ABS` | status `m_antiLockBrakes` (0/1) | 0 = off |
| `heading`, `pitch`, `roll` | physics | physics | motion `m_yaw`, `m_pitch`, `m_roll` | radians |
| `accG` | physics `accG` reordered `[x, z, y]` | same | motion `m_gForceLateral/Longitudinal/Vertical` | `[lateral, longitudinal, vertical]` |
| `gameState` | graphics `status` | graphics `status` | session `m_gamePaused` → Paused, else Live | F1 never reports Replay |
| `sessionType` | graphics `session` | graphics `session` | session `m_sessionType` (grouped) | |
| `flag` | graphics `flag` | graphics `flag` | status `m_vehicleFiaFlags` + session `m_safetyCarStatus` | |
| `sessionTimeLeft` | graphics `sessionTimeLeft` | graphics `sessionTimeLeft` | session `m_sessionTimeLeft` × 1000 | ms |
| `numCars` | static `numCars` | graphics `activeCars` (fallback static `numCars`) | participants `m_numActiveCars` | F1: 0 until the first participants packet (every 5 s) |
| `sectorCount` | static `sectorCount` | static `sectorCount` | constant 3 | |
| `totalLaps` | graphics `numberOfLaps` | graphics `numberOfLaps` | session `m_totalLaps` | 0 for timed sessions |
| `airTemp`, `roadTemp` | physics `airTemp`, `roadTemp` | physics | session `m_airTemperature`, `m_trackTemperature` | °C |
| `carModel` | static `carModel` | static `carModel` | team name from participants `m_teamId` | F1 has no car model, the team is the closest |
| `track` | static `track` | static `track` | track name from session `m_trackId` | |
| `playerName` | static `playerName` + `playerSurname` | same | participants `m_name` | F1 online may hide names |
| `normalizedCarPosition` | graphics `normalizedCarPosition` | same | lap `m_lapDistance` / session `m_trackLength` | 0..1 |
| `completedLaps` | graphics `completedLaps` | same | lap `m_currentLapNum` - 1 | |
| `position` | graphics `position` | same | lap `m_carPosition` | |
| `currentTime`, `lastTime`, `bestTime` | graphics `iCurrentTime`, `iLastTime`, `iBestTime` | same | lap times × 1000 | ms |
| `distanceTraveled` | graphics `distanceTraveled` | same | lap `m_totalDistance` | m |
| `isInPit` | graphics `isInPit` | same | lap `m_pitStatus == 2` | in the pit area / box |
| `isInPitLane` | graphics `isInPitLane` | same | lap `m_pitStatus >= 1` | |
| `currentSectorIndex` | graphics `currentSectorIndex` | same | lap `m_sector` | 0-based |
| `lastSectorTime` | graphics `lastSectorTime` | same | derived from `m_sector1TimeInMS`, `m_sector2TimeInMS` and the lap time at lap change (S3) | ms |
| `penaltyTime` | graphics `penaltyTime` | same | lap `m_penalties` | seconds |
| `tyreCompound` | graphics `tyreCompound` | same | status `m_actualTyreCompound` name (C1…C5, Intermediate, Wet, …) | |
| `tyreCoreTemp` | physics `tyreCoreTemperature` | same | telemetry `m_tyresInnerTemperature` | F1 has no core temperature; inner/carcass is the closest |
| `tyrePressure` | physics `wheelsPressure` | same | telemetry `m_tyresPressure` | psi |
| `brakeTemp` | physics `brakeTemp` | same | telemetry `m_brakesTemperature` | °C |
| `wheelSlip` | physics `wheelSlip` | same | motion `m_wheelSlip` (player car only) | game-defined scale |

### Why these fields are *not* in the common packet

| Field | Reason |
|---|---|
| `tyreWear` | ACC marks `tyreWear` as not used (always 0). AC and F1 provide it. |
| `tyreTempInner/Middle/Outer` | ACC does not fill `tyreTempI/M/O`; F1 only has surface + inner. |
| `surfaceGrip`, `windSpeed`, `windDirection` | Not in the F1 2020 UDP data. |
| `drs` | ACC GT cars have no DRS (field unused). |
| `turboBoost`, `ballast`, `kers*` | AC (and partly ACC) only. |
| `isLapValid`, `deltaLapTime`, `estimatedLapTime` | Not in AC shared memory. |
| `carDamage` | Present everywhere but with incompatible semantics (AC/ACC: 5 body zones, F1: wings/engine/gearbox). Kept in the full packet only. |
| `velocity`, `localAngularVelocity`, `carCoordinates` | Present everywhere but in different coordinate systems; kept in the full packet as raw game values. |

## Full packet – additional fields

All common fields plus the following game-specific fields (null/omitted when not available).

| Field | AC | ACC | F1 2020 |
|---|---|---|---|
| `turboBoost` | physics | physics | – |
| `ballast`, `kersCharge`, `kersInput` | physics | – | – |
| `drs` | physics `drs` | – | telemetry `m_drs` |
| `drsAllowed` | physics `drsAvailable` | – | status `m_drsAllowed` |
| `ersStoreEnergy`, `ersDeployMode` | – | – | status |
| `brakeBias` | physics | physics | status `m_frontBrakeBias` / 100 |
| `engineMap`, `tcCut`, `tcInAction`, `absInAction` | – | graphics / physics | – |
| `waterTemp` | – | physics | – |
| `engineTemp` | – | – | telemetry `m_engineTemperature` |
| `ignitionOn`, `starterEngineOn`, `isEngineRunning` | – | physics | – |
| `autoShifterOn`, `isAiControlled` | physics | physics | participants `m_aiControlled` (F1) |
| `velocity`, `localAngularVelocity`, `carCoordinates` | physics / graphics | physics / graphics (player entry of the 60-car arrays) | motion (world frame) |
| `sessionIndex`, `driverStintTotalTimeLeft`, `driverStintTimeLeft`, `missingMandatoryPits`, `globalFlags`, `penalty` | – | graphics | – |
| `safetyCarStatus`, `weather` | – | – | session |
| `pitWindowStart`, `pitWindowEnd`, `isOnline` | static | static | `isOnline` = session `m_networkGame` |
| `mandatoryPitDone`, `idealLineOn`, `replayTimeMultiplier` | graphics | graphics (`replayTimeMultiplier` unused) | – |
| `surfaceGrip`, `windSpeed`, `windDirection` | graphics | graphics | – |
| `airDensity` | physics | – | – |
| `trackGripStatus`, `rainIntensity`, `rainIntensityIn10Min`, `rainIntensityIn30Min`, `clock` | – | graphics | – |
| `trackConfiguration`, `carSkin` | static | – | – |
| `isLapValid`, `deltaLapTime`, `estimatedLapTime`, `isDeltaPositive`, `split`, `gapAhead`, `gapBehind` | – | graphics | `isLapValid` = !`m_currentLapInvalid` |
| `sector1Time`, `sector2Time`, `gridPosition`, `driverStatus` | – | – | lap data |
| `fuelPerLap`, `fuelEstimatedLaps`, `usedFuel` | – | graphics | `fuelEstimatedLaps` = status `m_fuelRemainingLaps` |
| `tyreWear` | physics (0..1) | – | status `m_tyresWear` (%) |
| `tyreTempInner/Middle/Outer` | physics | – | `tyreTempInner` = telemetry inner |
| `tyreSurfaceTemp` | – | – | telemetry |
| `tyreDirtyLevel`, `wheelLoad`, `rideHeight` | physics | – | – |
| `wheelAngularSpeed`, `suspensionTravel` | physics | physics | – |
| `slipRatio`, `slipAngle`, `brakePressure`, `padLife`, `discLife`, `frontBrakeCompound`, `rearBrakeCompound` | – | physics | – |
| `currentTyreSet`, `strategyTyreSet`, `rainTyres`, `mfd*` | – | graphics | – |
| `tyresAgeLaps`, `visualTyreCompound`, `tyreDamage` | – | – | status |
| `carDamage` | physics | physics | status (wings, engine, gearbox) |
| `numberOfTyresOut`, `finalFF` | physics | `finalFF` | – |
| `rainLights`, `flashingLights`, `lightsStage`, `wiperLevel`, `directionLightsLeft/Right`, `exhaustTemperature` | – | graphics | – |
| `kerbVibration`, `slipVibration`, `gVibration`, `absVibration` | – | physics | – |

## ACC fields that could not be mapped

The ACC shared-memory documentation marks these fields as not used by ACC. They are read but
never put into the packet because they are always zero: `wheelLoad`, `tyreWear`, `tyreDirtyLevel`,
`camberRAD`, `drs`, `cgHeight`, `numberOfTyresOut`, `kersCharge`, `kersInput`, `rideHeight`, `ballast`,
`airDensity`, `performanceMeter`, `engineBrake`, `ers*`, `kersCurrentKJ`, `drsAvailable`, `drsEnabled`,
`tyreTempI/M/O`, `P2P*`, `currentMaxRpm`, `mz`, `fx`, `fy`, `suspensionDamage`, `tyreTemp`,
`replayTimeMultiplier`, and most of the static page after `aidAutoBlip` (`hasDRS`, `hasERS`, `hasKERS`,
`kersMaxJ`, `engineBrakeSettingsCount`, `ersPowerControllerCount`, `trackSPlineLength`, `trackConfiguration`,
`ersMaxJ`, `isTimedRace`, `hasExtraLap`, `carSkin`, `reversedGridPositions`).
`tcInAction` and `absInAction` are documented as unused too but are forwarded as they cost nothing;
treat them as unreliable.

## AC traction control / ABS levels

AC exposes only the raw TC/ABS value of the active setting. `Kunos/LevelMapping.cs` learns the distinct
values per car while the driver cycles through the levels and reports the 1-based index in the sorted
set. Until every level has been seen once, indices may still shift, and `-1` is reported for a value
that has not been classified yet. ACC and F1 report real levels, so no inference is needed there.
