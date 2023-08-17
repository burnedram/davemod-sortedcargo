# Sorted Cargo
Adds sorting to your inventory while diving.  
Simply press you bound `Sort` key (default `Z` on keyboard and `Y` on gamepad).

## Releases
* [Latest](https://github.com/burnedram/davemod-sortedcargo/releases/latest)

## Installation
### BepInEx
* Install `IL2CPP` variant of [BepInEx](https://docs.bepinex.dev/master)
* Put `SortedCargo.dll` in `BepInEx\plugins`

Note: As of current writing (august 2023) there's a bug preventing BepInEx from working correctly with Dave the Diver.  
A fix is available [here](https://github.com/burnedram/Dobby/releases).  
It is also recommended (as of now) to use the [bleeding edge](https://builds.bepinex.dev/projects/bepinex_be) builds of BepInEx.

## Sorting options
Your inventory will be sorted in the following order everytime you press your `Sort` key:
* `Acquired`: The default order that the game decides
* `Acquired ▼`: Inverse default order
* `Weight ▲`: Lightest items first
* `Weight ▼`: Heaviest items first
* `Grade ▲`: Worst grade first, where dead fish are the worst grade
* `Grade ▼`: Best grade first
* `Rank ▲`: Lowest rank first
* `Rank ▼`: Highest rank first
* `Type ▲`: Internal type order of the items (ingredients, fish, special, etc.)
* `Type ▼`: Revere internal type order
