# ECS 160 Windows

This is the repository for the ECS 160 Winter 2017 Windows version of Warcraft.

## Compatibility

This version of the game is aimed to be compatible with Windows 7 and above.
Should you have any trouble running this program with any of the the supported operating systems, please leave a Github issue!

## Requirements

In order to compile the game, you will need:
* [Visual Studio](https://www.visualstudio.com/vs/) (2015 or later)
* [.NET Framework 4.5](https://www.microsoft.com/net/download/framework)

## Compiling

Open the project in Visual Studio and build the solution (`Build` > `Build Solution`).

## Running the game

Simply run `Warcraft.exe` in the `bin/Debug/` or `bin/Release/` directory after compiling the game.

### Controls

* Selecting Units: Left Click, Click + Drag for multiple units
* Moving/Commanding Units: With units selected, right click the target 
* Building Assets: Use the hotkeys described in the user manual (also found [here](https://github.com/UCDClassNitta/ECS160Windows/blob/master/src/App/Hotkeys.cs))
* Registering a Unit Group: With units selected, press Ctrl + # key
* Selecting a Unit Group: Press the respective # key

## Common Problems

### I get this exception when I run the game:

```
Exception thrown: 'SharpDX.SharpDXException' in SharpDX
Warcraft.vshost.exe Error: 0 : SharpDX.SharpDXException: HRESULT: [0x887A0005], Module: [SharpDX.DXGI], ApiCode: [DXGI_ERROR_DEVICE_REMOVED/DeviceRemoved]
```
This seems to be an issue with Windows 7. Install this [Windows Update](https://www.microsoft.com/en-us/download/details.aspx?id=36805) and verify that the issue is resolved.

### I hear the background music, but not the sound effects.

Install the latest [DirectX runtime](https://www.microsoft.com/en-us/download/details.aspx?id=35) and verify that the issue is resolved.
