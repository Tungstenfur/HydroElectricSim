## What the project is
Multiplatform simulation of a hydroelectric power plant heavily inspired by roblox game HES
## What it does
Its a simulator, it simulates somewhat realistically how a hydroelectric power plant would work
## How to install/run it
Make sure dotnet 10 sdk is installed
### Desktop (Windows and linux)
[Showcase](https://drive.google.com/file/d/1fTrMrHWvrBwGoCgNywhFtYUDZ4UKEZwO/view?usp=sharing)
```bash
cd HydroElectricSim.Desktop
dotnet run -c Release
```
### Web
Available at <https://tungstenfur.github.io/HydroElectricSim/>
```bash
cd HydroElectricSim.Web
dotnet publish -o out -c Release
```
Then run webserver from out/wwwroot
### Android
[Showcase on a emulator](https://drive.google.com/file/d/1jMO4qftb9ohU_H4vre8bTBHxEcOE8TIB/view?usp=sharing)  
Make sure Java 21 and Android SDK with API level 36 is installed

```bash
dotnet workload install android
cd HydroElectricSim.Android
dotnet publish -o out -c Release
```
Apk file will be in out folder
## How to use it
User interacts with switches to change parameters of the simulation, ive designed it in a way that user should be able to learn from the mistakes
## Requirements/dependencies
- Avalonia ui - framework for creating multiplatform gui apps in C# in WPF style
- MsBox.Avalonia - library for creating messsage boxes in avalonia
### Important technical decisions
#### Why avalonia
C# is my main programming language, i have experience with that framework, i really enjoy multiplatfrom aspect of it, the more popular WPF is windows exclusive which severely limits what i can do, as i use linux on my PC
#### Why hydroelectric sim
My first idea idea was an image processing app, but i realized that i wont manage to finish it in time, so i choose to lose 2 hours of work than extend creation of it into weeks, so ive decided to go with something simpler and there we are
