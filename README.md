# CustomSkillsAPI for Quest

CGNik's CustomSkillsAPI v5, built for the Quest client. Same public API as the PC DLL (24 of 24 public members match, same assembly name and version), so a mod written against CustomSkillsAPI compiles for Quest without touching its CustomSkillsAPI calls.

Original: https://github.com/CG-Nik/CustomSkillsAPI

## Install

Headset: put `CustomSkillsAPI.dll` (this build) in the headset's Mods folder, next to the Quest builds of the mods that use it, or take it from the Download board:

```
/sdcard/Android/data/com.Alta.ATownshipTale/files/MelonLoader/com.Alta.ATownshipTale/Mods/
```

Server: CGNik's own PC `CustomSkillsAPI.dll` v5, as always. A Quest is never a server, so this build is the player side only.

On its own it shows nothing new. It is what Quest versions of custom skill mods need, such as StingerReadded for Quest.

## What it does

The same as on PC: the five events in the same order (`PreSetUpProgressionSlots`, `SetUpProgressionSlots`, `PostSetUpProgressionSlots`, then the slots go into their trees, `PostAddProgressionSlots`, then each slot's parents learn it, `PostAddInherits`), and `AddProfessionSkill`, `RegisterProfessionSkill`, `FixProfessionSkillInspectorValues` and `AddProgressionSlot`.

## Differences from the PC DLL

| PC v5 | Quest build |
|---|---|
| `new ProgressionSlot(skill)` | A copy of the tree's first slot, reset to what the constructor gives, with its own lists: a Quest mod can't build a game type with `new` |
| Reflection on private fields | The IL2CPP properties for the same fields |
| Runs at `OnLateInitializeMelon` | The same, and if the game hasn't loaded its skills by then it tries again every second for a minute |
| One handler that throws stops the rest | Each handler runs on its own; a failure is logged with the handler's name and the others still run |
| | Logs one line per slot it added and a summary, for players' logs |

## Porting a mod that uses it

Your CustomSkillsAPI calls stay as they are. What changes is what changes for any Quest port:

- Reference this DLL and the LemonLoader IL2CPP assemblies, `MelonGame("Alta", "Att_Quest")`, `MelonAdditionalDependencies("CustomSkillsAPI")`.
- Private fields you set by reflection on PC are properties on Quest: `skill.practiceGlyphSprites`, `skill.targetAchievement`, `skill.inputStructure` and so on.
- `ProfessionSkill.All.Where(...)` doesn't compile against an IL2CPP list: look skills up by hash in `HashedGeneralValue<ProfessionSkill>.items` after `CheckItems()`.
- A copy of a game asset is `Object.Instantiate` plus `TryCast`, with `hideFlags = HideFlags.DontUnloadUnusedAsset` so the game doesn't unload it.
- PC asset bundles don't load on Android, and Quest has no `AssetBundle.LoadFromStream`.

StingerReadded for Quest is a worked example.

## Building

`build.ps1` builds `quest\CustomSkillsAPI.Quest.csproj` into `build\`. Put LemonLoader's `net8` folder and its generated `Il2CppAssemblies` in `lemon\`.

## Credits

CustomSkillsAPI is by CGNik, under the MIT licence. Quest port by Shadow.

This is an unofficial, fan made mod for A Township Tale. It is not affiliated with or endorsed by the owners or developers of A Township Tale.
