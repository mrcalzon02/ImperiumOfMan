# Crafting map — installed-game execution record

Source-version note: `main` advanced to [1.1.10 source `5fae43b`](https://github.com/mrcalzon02/ImperiumOfMan/commit/5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad), which removes finished Corpse Starch recovery and its batch UI. The session-007 evidence below applies to the unchanged **1.1.9 / `347de7d`** archive and does not validate that later source. See [1.1.10 change notes](../docs/crafting/CHANGE_NOTES_1.1.10.txt) for the correction.

## Current continuation result — session 007

[Session 007](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45869745770) completed the missing named-stack production check on unchanged source `347de7db77f9121ad26e1c12c534b97760ee92f2` and package SHA-256 `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`. No mod source or runtime archive changed for this continuation.

| Check | Observed result | Session-007 frames |
| --- | --- | --- |
| Named starting stack | Native Metal smelting feedstock tooltip: Capacity 2/30 | 0013 |
| One native batch | Quantity 1 selected; queue showed four outputs, one hour, Conveyor 1/1 | 0024–0025 |
| Completed delivery | Native named ×4 completion banner; the same named stack became 6/30 | 0028, 0031 |
| Cleared order | Conveyor 0/1 and empty Production slot | 0035–0036 |
| Map catalogue | Left Production tab; 2,363 items and 3,017 loaded recipes | 0036–0037 |
| Selection and focus | Ordinary selection preserved the view; Focus and double-click recentered the selected branch | 0037–0040 |
| Station cooutputs | Two textile plus one polymer stock → eight flak laminate and eight fitted armor liner; 120 base hours | 0043–0045 |
| Item without a producer | All-source Purity Seal: zero producing routes, three uses, explicit other-acquisition explanation | 0048–0049 |
| Fit | All twelve Purity Seal nodes fit at 40% zoom | 0050 |

The named **2/30 → 6/30** comparison, one-batch selector, queue and completion banner were independently checked against their screenshot transport hashes. This establishes **+4 in the identified stack**, not an inventory aggregate across unobserved storage tabs. It closes the identification/baseline gap from sessions 005 and 006 without changing their historical findings.

The complete 15,967-byte native log contains no managed-exception headings or missing-preset diagnostics. The copied campaign loaded normally, preserved the Ron Mask pact component, reached Space and saved. Three expected original-save sharing warnings came from the existing write/delete guards.

Operator stop at sequence 53 finished before the session deadline; the workflow concluded successfully. Final delivery `64d12e10c05b364fee33e392c6c38bdc09e9d034` confirms restoration of the original mod package, all eleven original campaign files unchanged, and complete removal of the disposable fixture. Six archived/removed fixture files match, with zero retained, unexpected or new campaign files.

[Session-007 acceptance, frame hashes and cleanup](CraftingMapAcceptance-347de7d-007.json) · [Prepared crafting notes](../docs/crafting/CHANGE_NOTES_1.1.9_CRAFTING_MAP.txt) · [Named stack before](../docs/crafting/native-crafting-feedstock-before-347de7d-007.png) · [Named stack after](../docs/crafting/native-crafting-feedstock-after-347de7d-007.png) · [Station details](../docs/crafting/native-crafting-station-cooutputs-347de7d-007.png) · [No-producer item](../docs/crafting/native-crafting-no-producer-347de7d-007.png)

Capability purchases, physical-chip learning, recycling, cancellation/refunds, active-order reload, migration and the separate Drop Support/turret checks retain their own acceptance scope. The Refresh comparison across a cargo change was not exercised in this session. The evidence below is retained under its original session and source.

## Earlier corrected-directory result — session 006

Corrected-directory [session 006](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45849635092) used the same 347 binary, completed the Space transition and save, and advanced from 26.10.2200 00:04 to 27.10.2200 22:24: **46 hours 20 minutes of in-game time**. Its complete 15,897-byte log contains zero missing-preset diagnostic lines and zero managed-exception headings. This confirms the bounded launch-context retest, not every gameplay transaction.

Session 006 ended at `bounded_slot_finished`, sequence 32. Immutable delivery `99ca540fe800abc8c9adda725c1bf4281bb339eb` confirms the original package was restored, original campaign files were unchanged and the disposable fixture was fully cleaned.

The exact source remains `347de7db77f9121ad26e1c12c534b97760ee92f2`, candidate SHA-256 `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`; no mod C# was changed for this retest. Controller `1ffc3f3b8094452b2031c83b08b236d022d6c3d6` launches from the installed game directory. The complete final log SHA-256 is `ff0ab3f3f813bf26637d43fe4e69d40ff4591ced7c611b79f09af79af84d2bdd`; cleanup receipt SHA-256 is `c20b112d26f68e18d8b72d085c3d6dcd40eb6f3dc4f7834407e152abe465021b`.

[Current receipt](OccultBuildReceipt.json) · [Session-005 cases and frame hashes](CraftingMapAcceptance-347de7d.json) · [Current ship/UI evidence](../docs/crafting/native-occult-ui-347de7d.json). Current Culture and Manifestation hovers passed in focused run 45857447221: Recipes / Acquired in game fits, and the three Manifestation Materials names are complete. Six-node topology, native status/costs and green/crimson/yellow states are retained. The focused session stopped at sequence 17 with restoration and fixture cleanup verified. Session 006 did not establish a cargo delta; session 007 above now verifies the identified stack's exact increase. Capability purchases, chip learning and recycling transactions remain separate checks. Session 005's map/queue observations and the original launch-context failure remain recorded below under their actual scope.

This record separates compilation, game startup, campaign loading, map interaction, and completed production. A successful runner job is not by itself a gameplay pass.

## Session 005 acceptance — original launch context

The latest executed candidate is `347de7db77f9121ad26e1c12c534b97760ee92f2`, built against the installed game by native run `45838490203`. Runtime ZIP SHA-256: `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`.

The in-game Crafting map passed the follow-up scrollbar, selection-label, Fit and Production-lifecycle checks. One native custom material batch queued four outputs and its queue later completed. A post-test cargo icon showed six units, but no before-stack total was captured: an exact **+4 cargo delta was unverified within session 005**. Session 007 above supplies the later named-stack comparison.

Mission generation raised a native null-collection exception. The diagnostic confirmed an empty global preset catalogue at the failing calls. A completed post-session source audit identified relative preset-file loading and the test launcher's working-directory mismatch; the subsequent session-006 runtime comparison is recorded above. Remaining transaction cases keep their separate acceptance scope. Original package restoration and all eleven original-save preservation checks passed. The raid Call-ins layout passed; the separate spacecraft Drop Support header comparison remains untested. Earlier session results below are retained as history.

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

The original source also logged three custom output reachability gaps after declared external sources: `bone_knife`, `impaler`, and `skrivnus_knife`. That diagnostic checks Magnum and workbench recipes and excludes ship recycling. It therefore does not establish global unreachability. The source already registers Raw Clone recycling with four human meat, two human skin and one human rib per clone; all three flagged recipes use human rib. The dynamic map includes that route under Recovery, and the All filter can trace across both production and recovery. These routes still require direct map and transaction checks.

### Evidence

- Native delivery: `f0a964e63c8bc8ce24e9d998ba64a3289160b2b1`, prefix `runner-results/quasimorph-native/45813237686-1` in the private runner repository.
- Load-failure capture: frame `0004`, action sequence `4`, evidence delivery `d8c19d9262942cb86741a4ea5e98cefacf6cbcc4`.
- Final cleanup receipt: delivery `1962c42e6ad67e3dd4d2aba9f0952bd698f25530`, prefix `runner-results/quasimorph-headed/45815345319-1`.
- Final flags: `original_package_restored=true`, `existing_campaign_files_unchanged=true`, `campaign_fixture.cleanup_complete=true`. No new campaign files remained.

## Follow-up acceptance

The second candidate is not a campaign/map acceptance pass. Correct the two observed runtime defects, compile the revised source against the installed game, and repeat campaign loading and the actual Production/map interactions before designating a final testing package. Record the exact revised source, ZIP, game frames, and completed transactions here.


## Third session: guarded 1.1.9 candidate — actual map execution

Source: `bdb17590322a8f27090918f83e0bf613eaee04de`. [Native build 45822897943](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45822897943), [headed run 45825014723](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45825014723).

The revised source requires a spoiled-organic target rather than accepting generic expiry replacements. It also preserves supplemental pact components when native project generation clones this mod's occult equipment. The compatibility path applies only to owned occult recipes and only after the native method actually runs; it retains native primary-equipment cloning and project modifications.

The guarded candidate compiled against the installed game with zero warnings and zero errors in 9.46 seconds. Its 11,794,864-byte ZIP was independently verified, including all transport chunks, CRC, the exact four runtime members, DLL version 1.1.9.0, and a byte-for-byte readback of its immutable public Git blob. ZIP SHA-256: `3e958daebacf3cee4fe7f503bc453d2dbc773bddb7a619b292a376f5d37b9f23`.

The headed request was pinned to that exact source and package, with the same reviewed slot-0-to-slot-2 disposable-copy procedure. Controller: `c34fb6d0914b2172a784c7400bec1faa4ca8b757`. The runner operated the game on its real desktop, with each input issued against a verified screenshot. The copied raid was ended through the normal Abort Mission confirmation on the disposable slot only, then Spacecraft > Supply > Conveyor opened Production.

### Observed results

| Check | Actual result | Evidence frames |
| --- | --- | --- |
| Startup and organic targets | Main menu reached. Exactly seven intended spoiled-organic IDs accepted; the five water/mechanical false positives were absent. No registration or Harmony startup exception. | 0002 |
| Saved campaign and pact equipment | Native slot-2 load succeeded in 4.7648913 seconds and reached the dungeon. The log confirmed preservation of the Ron Mask pact component on the custom project clone. | 0004–0005 |
| Production tab | The muted sage Crafting map tab appeared on the far left in Production and remained available in the native recipe picker. | 0014, 0030 |
| Expanded layout | Full panel opened from the left, graph above details, collapse handle on the right. The live index contained 2,363 items and 3,017 loaded recipe routes. No partial-index or map-capture error was observed. | 0015 |
| Selection | Clicking a process changed its highlight and lower detail panel while preserving graph position. | 0016–0017 |
| Wheel zoom | Wheel input changed the displayed zoom from 90% to 78%, with the graph scaled accordingly. | 0017–0018 |
| Drag pan | A drag moved the graph while keeping the selected process and its details. | 0018–0019 |
| Search | Searching exact item IDs found the current custom Lasgun and Raw Clone in the active game catalogue. Selecting the result focused that item and its local recipe graph. | 0020–0022, 0024–0027 |
| Metal feedstock values | Process details showed no inventory ingredients, four output units, three base hours, and learned/material-ready status. | 0017 |
| Lasgun values | The current project recipe showed one rolled armor plate, one armor fitting, and one weapon part producing two Lasguns in twelve base hours. The recipe was learned, but this campaign lacked cargo ingredients. | 0022–0023 |
| Occult foundation | Raw Clone production showed no inventory ingredients, two outputs, eight base hours, and separate missing Culture capability and unlearned-pattern requirements. Its graph linked Torment Imprints and clone recovery materials. | 0027–0028 |
| Escape and picker | Escape collapsed only the map and retained Production. The empty production slot then opened the native picker with the map tab still present. | 0029–0030 |
| Ordinary native queue | An available native medical item was selected, its quantity slider explicitly reduced from ten to one, and Start production submitted the order. Production then showed one item, one hour, and Conveyor 1/1. | 0034–0036 |
| Cleanup | Operator stop restored the original runtime package; all original campaign files remained unchanged; owned slot-2 fixture files were cleaned up. | Final receipt |

The native medical order confirms ordinary queue submission remains functional. It does **not** establish custom batch completion, ingredient refunds, or recovery transaction results. Those checks remain separate. The campaign had no Recycling module, and its Lasgun ingredients and Culture capability were unavailable. Fit/Focus buttons, detail-list scrolling, leaving Production entirely, and the earlier Drop Support header comparison were not exercised in this slot.

### Findings from execution

1. **Overlapping graph quantities:** counts from different connections shared the same midpoint and were visibly drawn over one another in dense views. Follow-up source `d5a15a06c8f5eacdb060bbab514432ac4637b5d9` displays quantities only on connections incident to the selected item/process. All graph connections and full detail values remain intact.
2. **Detail scroll affordance:** only about three list rows fit vertically, while the heading describes the complete page. Source review confirmed the remaining rows already exist in a working ScrollRect; this was not evidence of lost recipe entries. The same follow-up adds an auto-hiding scrollbar with a stable gutter to both detail columns. Search pagination is unchanged. These historical findings led to the source-347 refinements verified in session 005 below.
3. **Mission generation after return to Space:** `ArgumentNullException` (`collection`) occurred in installed `MissionFactory.GenerateProceduralPlan` at offset `0x001f2`, through `GenerateProceduralStructure` and the faction mission tick. Spacecraft and map UI continued to function. The cause is not established by this stack, and no Crafting map method appears in it. A bounded read-only audit of the installed mission API was queued as run `45831181826` to identify the actual collection before making a compatibility change. This candidate is not a clean overall gameplay pass.

### Evidence and integrity

- Final private delivery: `002336903bf7561e9b2bfbddc38c927b9b1c13d3`, prefix `runner-results/quasimorph-headed/45825014723-1`, consumed action sequence 38, state `operator_stopped`.
- Final flags: `original_package_restored=true`, `existing_campaign_files_unchanged=true`, `campaign_fixture.cleanup_complete=true`.
- Lasgun process screenshot: frame 0023, action 24, delivery `98de6745001e300f45e609826907849b6e334860`, PNG SHA-256 `f2165e725df933d466ef8d4991fa78cf751cc5df00373b2e83ba266cd071f00e`.
- Raw Clone process screenshot: frame 0028, action 29, delivery `c83d2534904f0119f5dfe5b73e22f3344a193a01`, PNG SHA-256 `d07b0e1b209b36ab6e758e9a663eb94103cdafb8b88a96bbee4d7e1c2d65a820`.
- Native queue screenshot: frame 0036, action 37, delivery `61cbf3faf4c8eddcfdf37aabe9e9b011445f4c11`, PNG SHA-256 `8790466c4923c74d36ae5f973d409f3fcbfc0561e78839c994b2d35cf08ca7d8`.
- Screenshot transport chunks were verified by length and SHA-256 before each PNG was decoded and viewed. The raw final receipt was retained without lossy integer reserialization.

## Fourth session: transport failure before launch

Source and package were the same `347de7d` / `4a096861…825413` candidate used below. [Headed run 45840559081](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45840559081) failed during preflight download with `IncompleteRead(16233029 bytes read, 19899 more expected)`.

No fixture was prepared, game launched or frame produced. This is a transport failure and supplies no gameplay result. The final receipt reports `original_package_restored=true`, `existing_campaign_files_unchanged=null`, no fixture, and sequence 0. Delivery: `f68ddc7343f512bef76523be252dc69cec413c8f`; status blob: `4377249ed959e75248457d05b32d4fb5407fbe31`. A fresh request retried the identical candidate.

## Fifth session: map refinements and native custom batch

Source: `347de7db77f9121ad26e1c12c534b97760ee92f2`. [Native build 45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203) passed with zero warnings and zero errors. Runtime ZIP SHA-256: `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`. [Headed run 45841032741](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45841032741) used controller `11277b5f0a23c7e3239396c381f54924101a08f6` and session `crafting-map-ui-005`.

This candidate includes the two map refinements from `d5a15a0` and a bounded mission-preset diagnostic that preserves native execution and exceptions. It used the same reviewed disposable slot-2 copy and eleven original-file guards. Startup again accepted exactly the seven intended spoiled-organic IDs. The log confirmed Ron Mask pact-component preservation and successful slot-2 Dungeon loading in 4.608027 seconds, followed by a normal return to Space.

### Observed results

| Check | Actual observation | Frames |
| --- | --- | --- |
| Raid Call-ins layout | At 1920 × 1080 the collapsed label was untruncated. The expanded Weapons / Pod Support / No pods installed panel stayed within its bounds. This is separate from the spacecraft Drop Support header. | 0006–0007, 0010 |
| Production entry and map | The sage tab appeared at the left of Production. The expanded map reported 2,363 items, 3,017 loaded recipes and 19 graph nodes. | 0018–0019 |
| Detail scrollbars | Both overflow bars were visible. Dragging reached producer row four and usage row six. Wheel input over Used by scrolled details while graph zoom stayed at 90%. Both bars hid for the short process-detail lists. | 0019–0022, 0024 |
| Selection quantities | Item-incident quantities were shown. Selecting the feedstock production process left only its outgoing `4` bright/numeric while the other graph connections remained visible and dim. Details retained no inventory inputs, four feedstock outputs, three base hours, Materials ready and Recipe learned. | 0019, 0024 |
| Fit | All 19 nodes were centered inside the viewport at 48% zoom. | 0023 |
| Ownership and lifecycle | Escape closed only the map. Native Production remained; its recipe picker retained the tab. Leaving Production for Space removed the tab. | 0025–0026, 0033 |
| Native custom submission | The Other picker showed the raw-ore and water rows with no inventory inputs. Raw ore's default quantity 30 was explicitly reduced to one batch. Production then showed the raw-metal icon `×4`, one hour and Conveyor 1/1. | 0029–0032 |
| Queue completion and cargo observation | Native time controls advanced 26.10.2200 from 00:08 to 11:23. Returning to Supply showed Conveyor 0/1. Arsenal tab 1 later showed an ore-matching icon with stack six at row four, column two. | 0033–0034, 0037, 0039 |

The native custom submission and subsequent queue completion were observed. The post-test stack of six has no captured baseline, and the matching icon alone does not prove its exact item identity. **An exact four-unit cargo increase was not measured.** The spacecraft native/Drop Support header comparison was not exercised in this slot; the raid Call-ins result does not replace it.

### Mission finding: empty global preset catalogue

The first complete diagnostic snapshot contains fourteen station/template identities, all reporting `availableFolders=[] (total=0,omitted=0)`. The first is station `Patton`, template `cerera_cereraMine_annaherman`, requesting `CereraSurface`; resolved folder, Elevators, Common and Corridors are null. Other requested folders include orbital, Moon and Callisto locations, and the reports include `iomMeridianWaterworks`.

Fifteen original `ArgumentNullException: collection` stacks follow through native `MissionFactory.GenerateProceduralPlan`, mission creation and the faction Space tick. The same failure recurred after time advancement. The captured diagnostic establishes that `Data.PresetsMap.AllPresets` was empty at those calls.

After this session, [read-only audit 45849018302](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45849018302) completed and was independently verified at delivery `bee68f710da80411898ca20206f4489101532a8e`. The [current native loader](https://github.com/mrcalzon02/Dens-Mortis/blob/bee68f710da80411898ca20206f4489101532a8e/runner-results/quasimorph-preset-path/45849018302-1/native-api/MGSC.Data.txt) reads relative `GameData/BinaryPresetsMap.json`, disables the persistent-data-path prefix, and silently substitutes a new empty map when that file is absent. Session 005's launcher used its evidence directory as the process working directory. The follow-up session changes the launcher working directory to the game directory while keeping the exact mod candidate unchanged. This source evidence identifies the mechanism to test; session 006 supplies the later bounded runtime confirmation, while session 005 alone does not prove the working-directory correction resolves the failure.

First diagnostic delivery: `54ab6d67a0ed3bc8dcd44fe43ecd907eb3229578`; log blob: `cdc944a4407e48d3d3355e81fecf19f15a94d397`. The exact 57,002-byte log, including its beginning, was retained immediately with SHA-256 `d0f9ef1e94ed48adb8a7fcb15658b7e241862e1b324e9658bfd8f7c7216d31f4`. Later error output filled the bounded 128 KiB tail. No production-completion message was present in that tail, so completion is assessed from the UI observations above.

### Cleanup and evidence integrity

Final private delivery: `2eec3e6597f6049d47bcd63d94501338f0c072ed`, prefix `runner-results/quasimorph-headed/45841032741-1`. The operator stopped at action sequence 42 on 26 November 2026, 15:24:50.075 UTC; the last frame was 0039 from action 41.

- `original_package_restored=true`, `existing_campaign_files_unchanged=true`, `campaign_fixture.cleanup_complete=true`.
- All eleven original files remained unchanged. Six remaining disposable-slot/autosave files were archived locally, hash-verified and removed; archived and removed filename sets match exactly. No retained, unexpected or new campaign files remained.
- Final status Git blob: `f7f29318586518d933fb45dd85bb5dd6dd686f9f`; exact raw status is 5,429 bytes, SHA-256 `e300acffc33ed7a670832a52e86084155f815b513b810515a1c5cd6844a20c69`. Its original bytes and full 64-bit process timestamp were preserved, and the Git blob identity was recomputed.
- Final Player.log blob: `cabcd6f0b704d8ecfa2399720aca39811d495f32`; 131,072 bytes, SHA-256 `92cdcc8214ef2acac48809b914a38fae9c5b7b0c0523f6091522be2daf0ce743`.
- The acceptance ledger binds 22 evidence frames to exact delivery commits, source/controller identities, transport-chunk lengths/hashes, decoded PNG bytes and dimensions. Its custom-batch result remains partial because the cargo-delta check is unverified.

Selected screenshot checksums:

| Frame | Observation | PNG SHA-256 |
| --- | --- | --- |
| 0023 | Fit at 48% | `0d08af425fec7106254498e217c2dd93fb97719a66432906fe59d2706a699e5d` |
| 0024 | Selected process and hidden short-list bars | `07e98aa6f4ec9b4e19c0d1798058389fe836763ebb56933381eae957fb3f05de` |
| 0032 | One custom batch queued as four outputs | `eea2e1c657e1728c67031ce9a2683a87c964c77108bcc32b6cce15edee69ddc9` |
| 0037 | Conveyor idle after time advancement | `b42e0e8e0da9f8bab3a25a1624d7b82f3c0615e44397f33d727c14ef43105fd9` |
| 0039 | Post-test matching-icon stack of six | `95b708b40a13dbdc73f14f7b81c4480b568f3262486ef494a1221632b511d439` |
