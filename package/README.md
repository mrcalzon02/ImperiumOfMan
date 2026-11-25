# Runtime package

For the Bound Ash inventory correction, use **[ImperiumOfMan-BoundAsh-Fix.zip](ImperiumOfMan-BoundAsh-Fix.zip)**. Close Quasimorph, extract the ZIP's `ImperiumOfMan` folder over the existing mod folder, and reload your save. It contains the patched DLL, manifest, bundle, and thumbnail.

The loose `ImperiumOfMan/ImperiumOfMan.dll` is the original compiled baseline and does **not** contain this correction. It is retained for reproducibility; the current patch is in the ZIP. Building `src/ImperiumOfMan.csproj` against a current Quasimorph installation regenerates the loose runtime package from the updated source.

The patch has compile and binary-preservation checks, but live-game validation remains pending. See [verification and scope](../diagnostics/BoundAshInventoryFix.md).
