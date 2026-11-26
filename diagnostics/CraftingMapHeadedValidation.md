# Crafting map — installed-game execution record

This record separates compilation, game startup, campaign loading, map interaction, and completed production. A successful runner job is not by itself a gameplay pass.

## Test environment

- Runner: `OLD-CORE30-homebrew`, active Windows desktop session.
- Installed game: Quasimorph `1.0.4.590s.969afc6`, Unity `2022.3.62f2`.
- Observed client resolution: 1920 × 1080 after the game applies its settings.
- Game assembly SHA-256: `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`.
- Inputs are issued against the latest captured game-client image; screenshots, action sequence, source revision, and package checksum are retained with each session.

## First session: 1.1.6

Source: `40939a398f1a2d3f79ec9ece08cfe5170444d885`. [Headed run 45801114949](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45801114949).

The mod booted and registered. A new disposable campaign reached the Supply module, but its Conveyor was uninstalled and the normal purchase requirement showed one required item with zero owned. Production and the map were not reached. The observed Drop Support heading also extended beyond its header panel; a later source update changed the header/tooltip presentation, awaiting direct comparison.

The original runtime package was restored, and all seven pre-existing slot-0 files remained unchanged. This session established startup and navigation only.

## Second session: 1.1.9 candidate

Source: `585715f554181ed56220c5ba2ed9055b46b44b38`. [Native build 45813237686](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45813237686), [headed run 45815345319](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45815345319).

Native compilation passed with zero warnings and zero errors. The 11,794,287-byte runtime ZIP passed chunk checks, ZIP CRC, exact four-member checks, manifest identity, member hashes, and non-executing DLL metadata inspection. Its SHA-256 is `f42be5c24cbd9d06e2d6e70984ba54a4186f77106f4c64763a7ffc0ba15fd925`.

For execution, the runner made an exact disposable copy of the established slot-0 campaign into unused slot 2. All eleven original files across slots 0 and 1 remained held against writes and deletion. No raw campaign content was uploaded to Git.

### Observed results

| Stage | Actual observation | Result |
| --- | --- | --- |
| Startup | Registration completed and the main menu appeared. | Passed |
| Organic recipe discovery | The accepted target list included `water_bottle_1`, `spring`, `weapon_parts`, `metal_tread`, and `wire`. | Defect: generic expiry conversions were being treated as organic spoilage. |
| Campaign selection | The copied campaign retained an existing error marker; native selection still attempted loading slot 2. | Copy and selection worked. |
| Campaign loading | `MagnumDevelopmentSystem.InjectItemRecord` threw for `ron_mask_1`, because its supplemental `PactComponentRecord` had no supported clone branch. Native result: `FailedReadSaveData`. | Blocked before Space/Dungeon and Production. |
| Map interaction | No production window was reached. | Not run |
| Cleanup | Original package restored; all originals unchanged; all seven owned slot-2 files archived locally and removed. | Passed |

The load exception occurred during `MagnumProjects.OnAfterLoad` and global-component reconstruction. It is not an original-save sharing violation. The protected originals produced separate, expected read-write-open failures for their error/thumbnail files in the campaign menu.

The original source also logged three custom output reachability gaps after declared external sources: `bone_knife`, `impaler`, and `skrivnus_knife`. These remain coverage findings, rather than evidence that all objects can be manufactured from raw materials.

### Evidence

- Native delivery: `f0a964e63c8bc8ce24e9d998ba64a3289160b2b1`, prefix `runner-results/quasimorph-native/45813237686-1` in the private runner repository.
- Load-failure capture: frame `0004`, action sequence `4`, evidence delivery `d8c19d9262942cb86741a4ea5e98cefacf6cbcc4`.
- Final cleanup receipt: delivery `1962c42e6ad67e3dd4d2aba9f0952bd698f25530`, prefix `runner-results/quasimorph-headed/45815345319-1`.
- Final flags: `original_package_restored=true`, `existing_campaign_files_unchanged=true`, `campaign_fixture.cleanup_complete=true`. No new campaign files remained.

## Follow-up acceptance

The second candidate is not a campaign/map acceptance pass. Correct the two observed runtime defects, compile the revised source against the installed game, and repeat campaign loading and the actual Production/map interactions before designating a final testing package. Record the exact revised source, ZIP, game frames, and completed transactions here.
