# OnlineKill

Just Another ULTRAKILL Multiplayer Mod

## About The Project

Experimental multiplayer for ULTRAKILL, built around a small dedicated UDP server instead of Steam lobbies or Steam P2P.

## Current State

This is the first playable slice:

- BepInEx plugin for ULTRAKILL.
- Standalone `OnlineKill.Server` executable.
- Manual IP/port connection through the in-game `F7` menu.
- Basic local player position and rotation sync.
- Remote players shown as simple capsule stand-ins with name tags.

It does not sync weapons, enemies, damage, doors, checkpoints, Cyber Grind, or level state yet. Those systems should be ported in stages from the JAKET/COAT/Polarite-style references onto this transport layer.

## Build

```powershell
dotnet build .\OnlineKill.sln -c Release
```

## Run a Server

```powershell
dotnet run --project .\OnlineKill.Server -- 27040
```

For internet play, forward UDP `27040` to the machine running the server.

## Install the Plugin

Copy `OnlineKill.Mod\bin\Release\netstandard2.1\OnlineKill.dll` into `BepInEx\plugins`.

In game:

1. Press `F7`.
2. Enter the server IP, port, and your player name.
3. Press `Connect`.

## Why Not Use MultiplayerUtil?

`MultiplayerUtil` is useful, but its documentation and XML comments show it is built around Steam lobbies and Steam P2P. OnlineKill uses direct UDP because the goal here is a dedicated server that can run without Steam networking.

## Warning

This is a work in progress and is not yet ready for public use. It is not recommended to use this mod in its current state. (also uh, its vibecoded lol)
