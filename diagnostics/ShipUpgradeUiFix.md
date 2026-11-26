# Ship upgrade presentation — 1.1.6

Security Intrusion, Entrance Override and Extraction Override use their own **Comms Node** technology window. Their existing perk IDs, parent links, prices and turret behavior are retained. Navigation no longer contains this custom branch.

**Ship Weapons** moves beneath the painted MAGNUM name. The Comms Node marker is 32 canvas units aft of the old Weapons position, beside the antenna in the supplied overview. Supply and troop-pod upgrades remain in **Drop Support**, whose marker moves above the cargo section at the center of the hull. All three follow the existing Engineering anchor and native module-camera selection.

| Marker | Offset from Engineering in canvas units | Approximate center in the supplied 3× overview |
| --- | --- | --- |
| Ship Weapons | (-80, 0) | (445, 339), beneath MAGNUM |
| Comms Node | (32, -46) | (781, 477), beside the antenna |
| Drop Support | (0, 64) | (685, 147), above Engineering |

Custom main systems use 36-pixel icons. Incremental upgrades use 18-pixel icons, with matching hit and hover areas and the normal-size yellow upgrade marker. Native pooled-node dimensions are restored before a cell is reused by another module. Locked custom icons use the native crimson palette while retaining the original artwork and transparency.

The Weapons tree shows the shared fitted-mount prerequisite with native grid connections. These drawing links do not add gameplay prerequisites: any fitted mount still enables the three independent fire-support upgrades.

The base-game tooltip and purchase behavior are unchanged. No tooltip mod or additional dependency is included.

## Verification

The source follows the 1.1.5 Crafting Map commit. Native compile/package validation is pending for this revision. The previous-source API inspection completed successfully against the owner's installed game through Dens-Mortis run `45789900877`, delivered at `71aae8b1d698fd581c5a59f0d72e2b9745344a89`. It confirms the current four-argument node initializer, native grid-line method, module anchor and camera APIs. It is not a gameplay or visual acceptance run. Actual in-game placement, hover geometry and camera framing still require a live capture.
