# 7 Days To Die Websocket Integration

## Introduction

This is a mod for [7 Days To Die](https://7daystodie.com/). It allows you to connect to the websocket server and receive updates about the game.

# IMPORTANT
You as a user are responsible for your own actions. This mod is not intended for revenue generation, and if used in such way, will have no support provided. This mod is provided as is, and as a user of the mod, you must follow all [End User License Agreements](https://7daystodie.com/eula/), and [Terms of Service](https://7daystodie.com/terms-of-service/) policies provided by The Fun Pimps Entertainment LLC. In no way shape or form is this mod associated with The Fun Pimps Entertainment LLC, or Streamer.bot, and any liabilities for any parties involved shall only be handled between yourself, and The Fun Pimps Entertainment LLC.

----

In the instance of this mod violating terms and conditions, file a removal request as an issue on the [GitHub repository](https://github.com/KK964/7-Days-To-Die-Websockets-Mod)

## Setup

To install you need to either download a release, or build the mod yourself.

### Download

1. Download the latest release from [GitHub](https://github.com/KK964/7-Days-To-Die-Websockets-Mod/releases)
2. Extract the zip file
3. Copy the folder to your [7 Days To Die mods folder](https://7daystodie.fandom.com/wiki/How_to_Install_Modlets#1.29_Create_a_folder_called_.22Mods.22_at_the_top_level_of_the_game_folder.)
4. Depending on if you want to run single player, or multiplayer, the next step will be different.

   - Single Player
     - When running the game, disable EAC (Easy Anti-Cheat)
   - Multiplayer
     - When running the game, enable EAC (Easy Anti-Cheat)

5. Restart the game

### Build

1. Clone the repository
2. Run the update local references script

   - Windows
     - Right click the script at `./scripts/updateLocalRefs.ps1`
     - Click `Run with PowerShell`
     - The script will ask for the path to the 7 Days To Die install directory
     - Debugging
       - If you get a file not found error, you may need to give the script the full path to the 7 Days To Die install directory
         - `./scripts/updateLocalRefs.ps1 -path /path/to/7dtd`

   - Linux or bash compatible shell
     - `./scripts/updateLocalRefs.sh "<path to 7 Days To Die install directory>"`
     - As 7D2D contains spaces in the path, you need to wrap the path in quotes
     - Debugging
       - If you get a permission denied error, you may need to give the script execute permissions
         - `chmod +x ./scripts/updateLocalRefs.sh`
       - If you get a file not found error, you may need to give the script the full path to the 7 Days To Die install directory
         - `./scripts/updateLocalRefs.sh /path/to/7dtd`

3. Inside visual studio, open `/src` as the project
4. Build the project
5. The resulting dll will be in `/src/bin/Debug/` as `7DTDWebsockets.dll`
6. Make a new folder in your mods folder called `7DTDWebsockets`
7. Copy the dll to the new folder
8. Copy `ModInfo.xml` to the new folder
9. Copy `Config.xml` to the new folder
10. Copy `websocket-sharp.dll` to the new folder
11. Copy `UnityEngine.dll` to the new folder
12. Copy `UnityEngine.CoreModule.dll` to the new folder
13. Copy `0Harmony.dll` to the new folder
14. Restart the game

### Config.xml

- Host: The hostname of the websocket server
- Port: The port of the websocket server

----

## Usage

Connect to the websocket server; the server will send updates about the game.

## Events

- ChatMessage:
  - `ChatMessage {"player": {"name":"PlayerName"}, "message":"Message"}`
- PlayerDeath:
  - `PlayerDeath {"player": {"name":"PlayerName"}}`
- PlayerKillZombie:
  - `PlayerKillZombie {"player": {"name":"PlayerName"}, "entity":"EntityName"}`
- PlayerKillAnimal:
  - `PlayerKillAnimal {"player": {"name":"PlayerName"}, "entity":"EntityName"}`
- PlayerJoin:
  - `PlayerJoin {"player": {"name":"PlayerName"}}`
- PlayerLeave:
  - `PlayerLeave {"player": {"name":"PlayerName"}}`
- PlayerSpawnIn:
  - `PlayerSpawnIn {"player": {"name":"PlayerName"}, "type":"TypeOfSpawn"}`
- PlayerDamage:
  - `PlayerDamage {"player": {"name":"PlayerName"}, "damage": "DamageAmount", "cause": "DamageCause"}`
- PlayerKillEntity:
  - `PlayerKillEntity {"player": {"name":"PlayerName"}, "entity":"EntityName", "animal":bool, "zombie":bool, "headshot":bool, "weaponType": "WeaponType"}`

## HTTP API

The HTTP API is not yet fully implemented.

Using:

1. Send POST/GET to host:port/api/endpoint

### Get

- This is currently not implemented.

### Post

- `/command`: Send a command to the server. The command to send is in the body of the request. The response will be the result of the command.

```bash
curl -X POST http://localhost:9000/api/command
   -H "Authentication: abc123"
   -d 'gettime'
```

->

> `Day 3, 06:23`
