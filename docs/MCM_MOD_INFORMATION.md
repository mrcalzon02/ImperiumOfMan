# Optional MCM: installed build identification

Open **Main Menu → Mods → Imperium Reforged → Mod information** with Crynano's
Mod Configuration Menu installed. The information page is intended to contain:

- Installed version: the four-part assembly version of the actual loaded DLL.
- Build ID: the first 12 hexadecimal characters of its module version ID (MVID).

The startup log records the full MVID and version even without MCM. These
values describe the loaded code, not the current GitHub source, workshop
listing, filename, configuration file, or latest available release. The short
MVID is a diagnostic fingerprint, not a release number or security hash.

The mod uses the typed MCM registration API and read-only StringConfig rows
through reflection. MCM 1.6.2+ is optional; an absent or incompatible API must
not stop normal mod startup. No MCM binary is distributed. The initial page
does not check for or install updates and does not alter gameplay or saves.

## Headed acceptance checks (pending)

1. Build the repository with the game's managed assemblies and package the DLL.
2. Start Quasimorph with the package and current MCM installed. Open Mods,
   select Imperium Reforged, and capture both information lines.
3. Compare the displayed version and short build ID with the startup log
   and the packaged DLL's assembly metadata. Change language to Russian and
   verify the labels; reset defaults and verify that the identity is unchanged.
4. Return to the main menu and reopen Mods; confirm there is exactly one
   Imperium entry and its information remains unchanged.
5. Repeat startup with MCM disabled; confirm the mod loads and the version
   and full MVID are present in the game log.
6. Repeat with a different built DLL and confirm the displayed identity
   follows that DLL rather than an old configuration file.

Release packaging should increment the existing AssemblyVersion in
src/Properties/AssemblyInfo.cs as appropriate. This source-only integration
does not increment the release version or represent a published build.
