# Imperium of Man Asset Inclusion

This repository is the active and self-contained development fork for the mod:

- Repository: `mrcalzon02/ImperiumOfMan`
- Working branch: `main`
- Historical upstream repositories are reference material only.

This document covers asset import and bundle inclusion only. Runtime item registration, balance, stations, research, and acquisition are separate recovery phases.

## Required editor

Open `unityproj/ImperiumOfMan` with Unity `2022.3.50f1`, matching `ProjectSettings/ProjectVersion.txt`.

Run the ThunderKit import configuration against the currently installed Quasimorph build before trusting imported game assemblies or descriptor types.

## Why the new graphics were not usable

The graphics had been pushed as ordinary files, but many lacked committed Unity `.meta` files. The copied bolt-pistol and bolt-rifle Unity objects also reused the original bolter GUIDs, descriptor name, render ID, sound-bank identity, and sprite references. Unity therefore could not treat those copies as independent assets.

The repaired pistol and rifle assets now use unique descriptor, sound-bank, audio, and sprite GUIDs. Do not copy a Unity asset together with its `.meta` file when creating another object.

## Asset root and bundle

ThunderKit's `Assets/Manifest.asset` includes the complete folder:

`Assets/ImperiumOfManMod`

in:

`ImperiumOfMan.bundle`

New descendants of that folder do not need separate manifest entries. They do need valid Unity imports and unique GUIDs.

## Import repair

After pulling `main`, open the project and use:

`Imperium of Man > Assets > Repair Import Settings and Validate`

This command:

1. Refreshes the Unity Asset Database.
2. Generates Unity metadata for files that do not have it yet.
3. Reimports PNG and MP3 files under `Assets/ImperiumOfManMod`.
4. Applies the common sprite and audio import policy.
5. Checks source inclusion before a bundle build.

After the command finishes, commit every newly generated `.meta` file to `main`. Generated metadata must not remain only in one developer's local Unity project.

## Source validation

The validator rejects:

- source files without `.meta` files;
- `.meta` files whose asset or folder is absent;
- duplicate Unity GUIDs;
- source files Unity cannot import as the expected type;
- duplicate serialized `.asset` names;
- a ThunderKit manifest that no longer references the Imperium asset root;
- a changed bundle name.

A failed validation must be corrected before running the ThunderKit build pipeline.

## Built bundle validation

After ThunderKit builds the bundle, use:

`Imperium of Man > Assets > Validate Built Bundle...`

Select the newly built `ImperiumOfMan.bundle`. The command opens the bundle, enumerates it, and compares its contents with the current PNG, audio, prefab, material, and serialized assets under `Assets/ImperiumOfManMod`.

The asset-inclusion gate passes only when the validator reports that every current source asset is present in the selected bundle.

## Naming requirements

For an inventory/floor item graphic set, keep one clear base name and use:

- `<name>_icon.png` for inventory display;
- `<name>_floor.png` for the small or floor display;
- `<name>_shadow.png` for the floor shadow.

Capitalization should be consistent. Existing mixed names are retained during recovery to avoid accidental path breakage, but future additions should use lowercase suffixes.

Every serialized descriptor must have:

- a unique filename;
- a unique `m_Name`;
- a unique `_overridenRenderId` when used;
- a unique `.meta` GUID;
- references to the intended icon, floor, shadow, and sound-bank GUIDs.

## Current boundary

Passing this asset-inclusion process proves that Unity imported and bundled the files. It does not make an image into a functioning Quasimorph item. Functionality restoration begins only after the built bundle passes validation and will add runtime records, donor cloning, localization, acquisition routes, and gameplay tests.
