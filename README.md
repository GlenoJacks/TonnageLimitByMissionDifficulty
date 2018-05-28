# TonnageLimitByMissionDifficulty
A Battletech mod to limit the players drop tonnage to match the difficulty rating of the current contract.

This mod is inteded to encourage the player to maintain a greater variety of mechs in their mech bay during the early and mid game. 

Drop tonnage limits scaled to mission difficulty help to ensure that each battle is reasonably challenging, and the constraints imposed by only being able to bring a limited amount of resources puts more pressure on the player to construct optimized lances that contain a mix of weight classes and specializations.


Based off of **Mpstarks** [**LeopardDropLimit**](https://github.com/Mpstark/LeopardDropLimit)

## Installation

Install [BTML](https://github.com/Mpstark/BattleTechModLoader) and [ModTek](https://github.com/Mpstark/ModTek) if you haven't already.

Extract the files from TonnageLimitByMissionDifficulty.zip (found in the [releases](https://github.com/GlenoJacks/TonnageLimitByMissionDifficulty/releases) page) to BATTLETECH\Mods\TonnageLimitByMissionDifficulty\

## Configuration

You can change the drop tonnage limit for each of the contract difficulty levels by modifying the field "tonnageByDifficulty" in  mod.json.
There are 10 difficulty levels, where each entry expresses the maximum drop tonnage for the difficulty.
