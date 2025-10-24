# Packet Mapping

## ``GameState``

Represents the current state of the game.

| Int Value     | Value       | Description                     |
|---------------|-------------|---------------------------------|
| `0`			| `OFF`       | Game is off						|
| `1`			| `REPLAY`    | Game is showing a replay 		|
| `2`			| `LIVE`      | Game is live					|
| `3`			| `PAUSED`    | Game is paused					|

## ``SessionType``

Represents the type of session currently being played.

| Int Value     | Value           | Description                     |
|---------------|-----------------|---------------------------------|
| `0`			| `PRACTICE`      | Practice session                |
| `1`			| `QUALIFY`       | Qualifying session              |
| `2`			| `RACE`		  | Race session                    |
| `3`			| `HOTLAP`		  | Hotlap session					|
| `4`			| `TIME_ATTACK`   | Time Attack session             |
| `5`			| `DRIFT`		  | Drift session					|
| `6`			| `DRAG`		  | Drag session					|

## ``Flag``

Represents the current flag status in the game.

| Int Value     | Value           | Description                     |
|---------------|-----------------|---------------------------------|
| `0`			| `NO_FLAG`       | No flag							|
| `1`			| `GREEN`         | Green flag						|
| `2`			| `YELLOW`        | Yellow flag						|
| `3`			| `WAVED_YELLOW`  | Waved flag						|
| `4`			| `DOUBLE_YELLOW` | Double yellow flag				|
| `5`			| `WHITE`         | White flag						|
| `6`			| `CHECKERED`     | Checkered falg					|
| `7`			| `BLUE`          | Blue flag						|
| `8`			| `WAVED_BLUE`    | Blue waved flag					|
| `9`			| `BLACK`         | Black flag						|
| `10`			| `ORANGE_BLACK`  | Black flag with organge circle  |
| `11`			| `BLACK_WHITE`   | Black and white flag            |
| `12`			| `YELLOW_RED`    | Yellow and red striped flag     |
| `13`			| `GREEN_YELLOW`  | Yellow and green diagonal flag  |
| `14`			| `SC`            | Safety car flag                 |
| `15`			| `VSC`           | Virtual safety car flag         |
| `16`			| `CODE_60`       | Code 60                         |
| `17`			| `FCY`           | Full course yellow              |
| `18`			| `FCY`           | Full course yellow              |