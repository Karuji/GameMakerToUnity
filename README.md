# GameMakerToUnity
Imports Assets from Game Maker Studio to Unity

## Important
**The importer *DOES NOT* convert GML to Unity C#!**

## What is it
GameMakerToUnity imports the project and assets from a Game Maker Studio 1 project to Unity.

It will preserve the folder structure as seen in the GM IDE.

In doing this it imports assets like sprites with the meta data from GM.

Objects from GM are converted to Prefabs in Unity.

Rooms are recreated as scenes.

## Why do it
I've been looking at doing console ports for some games, and Unity is currently the better option. With the amount of assets it would take a lot of time to create the project by hand. Fortunately editor scripts made it easy to just make a program in Unity to recreate the projects.

## Why no script converter

 - GM has a kind of weird object orientation, that isn't analogous to the component based system in Unity.
 - Code would be more performant if it is written in Unity from the get go, instead of retro-fitted.
 - It'd probably take as long to write a converter as just rewriting the codebase from scratch.