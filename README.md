![image](https://github.com/FreshDaikon/MDMC3/assets/36603973/87a01509-36a9-4656-979c-191fd4dcca6c)
[![Build and deploy Server](https://github.com/FreshDaikon/MDMC/actions/workflows/build_server.yml/badge.svg?branch=main)](https://github.com/FreshDaikon/MDMC/actions/workflows/build_server.yml)
[![Build and deploy Daikon Connect](https://github.com/FreshDaikon/MDMC/actions/workflows/build_daikon.yml/badge.svg?branch=main)](https://github.com/FreshDaikon/MDMC/actions/workflows/build_daikon.yml)

**MDMC (Multi Dimensional Monster Combatants)** </br>
This project has been going for a while now, and as evident by the name, this is the 3rd reboot!
Time will tell if more reboots will be needed hehe. </br>

This readme serves as a reminder to myself what the project is about.
It can link to important information or style guides that i should make use of.

Useful Design Links for MDMC: </br>
- [Combat Design](https://github.com/FreshDaikon/MDMC3/wiki/Combat) </br>
- [Encounter Design](https://github.com/FreshDaikon/MDMC3/wiki/Bosses-and-Arenas) </br>
- [Systems Design](https://github.com/FreshDaikon/MDMC3/wiki/System) </br>
 </br>
 
**Link to Project :** </br>
[Overall Tracker](https://github.com/users/FreshDaikon/projects/1)</br>


Techical tips:</br>
In order to build/run with the included launch.json, make sure to install Godot like so:</br>
RootFolder(MDMC)/.Engine/Godot.exe</br>
and </br>
RootFolder(MDMC)/.Engine/Godot_Console.exe</br>
/.Engine will naturally be excluded by git, so on a local repo simply just download Godot at :</br>
https://godotengine.org/download/windows/</br>

Addionally, if the steam_api64.dll is missing from /Game/.. please download it and place it there.
When making a build you will also need to copy the Steam dll into the output folder.
In general Exporting/Making builds have become a bit of a chore, so better to not bother until we have proper CI/CD done.










