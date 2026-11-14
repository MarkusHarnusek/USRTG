# USRTG Server

USRTG (Unified Sim Racing Telemetry Getter) collects live telemetry from several racing games,
normalises it into one game-agnostic packet and serves it over HTTP as JSON.

## Supported games

| Game | Transport | Notes |
|---|---|---|
| **Assetto Corsa** | shared memory (`Local\acpmf_*`) | nothing to configure |
| **Assetto Corsa Competizione** | shared memory (`Local\acpmf_*`) | nothing to configure; the running process decides AC vs. ACC |
| **F1 2020** | UDP, port 20777 | in game: *Settings → Telemetry Settings → UDP Telemetry: On, UDP Format: 2020, UDP Port: 20777* |

The server polls the sources at 60 Hz (configurable) and keeps the latest packet. The first source
that delivers data wins; when no game is running the server idles at 2 Hz.

## HTTP API

`GET /packet` returns the **common packet** – the data every supported game can provide, with the
same units, wheel order and enumerations regardless of the game. See [`server/Packets.md`](server/Packets.md)
for the field list and per-game sources and [`server/Mapping.md`](server/Mapping.md) for the enumerations.

| Status | Meaning |
|---|---|
| `200` | JSON packet (`application/json`) |
| `204` | no game is running / no data received yet |
| `404` | any other path |

CORS is enabled for all origins, responses are `Cache-Control: no-store`.

Example:

```json
{"game":2,"gameName":"AssettoCorsaCompetizione","id":48213,"timestamp":1794688253239,
 "throttle":0.75,"brake":0.0,"steer":-0.12,"clutch":1.0,"speedKmh":211.5,"gear":4,"rpms":6500,
 "maxRpm":7500,"fuel":55.0,"maxFuel":120.0,"fuelPercent":45.8,"pitLimiterOn":false,"tc":3,"abs":2,
 "heading":1.2,"pitch":0.01,"roll":-0.02,"accG":[1.1,-0.5,1.0],"gameState":2,"sessionType":2,"flag":0,
 "sessionTimeLeft":1234567.0,"numCars":24,"sectorCount":3,"totalLaps":0,"airTemp":22.0,"roadTemp":31.0,
 "carModel":"porsche_992_gt3_r","track":"spa","playerName":"Max Mustermann",
 "normalizedCarPosition":0.42,"completedLaps":3,"position":7,"currentTime":61000,"lastTime":138000,
 "bestTime":137500,"distanceTraveled":24000.0,"isInPit":false,"isInPitLane":false,"currentSectorIndex":1,
 "lastSectorTime":45000,"penaltyTime":0.0,"tyreCompound":"dry_compound",
 "tyreCoreTemp":[80,81,82,83],"tyrePressure":[27.1,27.2,27.3,27.4],"brakeTemp":[400,410,380,390],
 "wheelSlip":[0.1,0.1,0.2,0.2]}
```

The full, game-specific packet (`server/Packet.cs`) is still assembled internally and documented in
`Packets.md`; it is not exposed over HTTP.

## Build and run

Prerequisites: .NET 9 SDK, Windows (the Kunos shared memory is Windows-only; the F1 2020 UDP source works anywhere).

```pwsh
cd server
dotnet build
dotnet run -- [--port 8080] [--f1-port 20777] [--rate 60]
```

| Option | Default | Description |
|---|---|---|
| `--port` | `8080` | HTTP port |
| `--f1-port` | `20777` | UDP port the F1 2020 game sends to |
| `--rate` | `60` | telemetry poll rate in Hz |

Binding all interfaces (`http://+:port/`) requires a URL ACL on Windows unless the server runs elevated.
Without one the server falls back to `localhost` only and prints the command that grants LAN access:

```pwsh
netsh http add urlacl url=http://+:8080/ user=Everyone
```

## Tests

```pwsh
dotnet test tests/USRTG.Tests
```

The tests check the shared-memory struct layouts against the known game sizes/offsets, decode synthetic
F1 2020 datagrams (including over a real UDP socket), and verify the mapping and the common packet projection.

## Project structure

```
server/
  Program.cs             entry point, argument parsing
  Network.cs             HTTP server (GET /packet)
  Packet.cs              full packet (all games, nullable game-specific fields)
  CommonPacket.cs        game-agnostic packet served over HTTP
  Enums.cs               unified enumerations (Game, GameState, SessionType, Flag)
  Mapping.cs             game value -> unified value translation
  Units.cs               unit / wheel-order helpers
  Packets.md, Mapping.md documentation of the packets and enumerations
  Telemetry/             ITelemetrySource + TelemetryManager (poll loop)
  Kunos/                 shared-memory reader, AC/ACC game detection, TC/ABS level inference
  AC/                    Assetto Corsa structs + handler
  ACC/                   Assetto Corsa Competizione structs + handler
  F1_2020/               UDP listener, packet parser, lookup tables
tests/USRTG.Tests/       xunit tests
util/getCurrentData.js   browser panel that polls /packet
```

## Utility: `util/getCurrentData.js`

A browser-console snippet that shows a floating panel polling `/packet` at a configurable interval
(default 2000 ms) with start/stop controls and exponential back-off on errors. Edit the `url` constant
at the top to point at your server. Press `✕` or `Escape` to close the panel.

## License

MIT, see [LICENSE](LICENSE).
