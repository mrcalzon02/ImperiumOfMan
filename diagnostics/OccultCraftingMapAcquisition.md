# Occult acquisition in the crafting map

Baseline: `40afaed7452511a1cb92ad399bf8c88e1c5a3f00` ([native validation](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/46076190711)). This continuation adds a read-only Sources view inside the existing selected-node detail panel. It does not add crafting edges from chips to manufactured items.

- Select an occult recipe or item with registered pattern/specimen suppliers and press Sources. Select an entry in the left scrollable list; inspect availability in the right list. Details returns to ordinary ingredients/products or producers/consumers.
- The snapshot distinguishes current faction tech, native reputation reward cap, faction status and reward eligibility. Neither stock nor eligibility guarantees a sale or reward.
- Physical stock matches item ID and saved DatadiskComponent.UnlockId. Mixed-pool chips with other selections are excluded. Ship counts are available cargo, not mercenary inventory. Station counts are internal storage, not an offer or quote.
- Specimens retain individual issuer routes. Other-owner chip stock is labeled. No knowledge, stock or reputation is granted.
- Index capture scans each station and available cargo once on open/refresh, not per gameplay frame. Graph and recipe registration are unchanged.

Acceptance: inspect an unlearned regional pattern with inactive, reputation-blocked and eligible issuers; select a specimen with alternate suppliers; verify mixed-chip counts against saved selections; toggle Details and Sources and check scrolling and selection; refresh following a trade. Native compile and headed visual/gameplay validation remain separate.
