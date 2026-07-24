# DiscoSO Changelog 📜

Player-facing changes to DiscoSO — the game client and server, the launcher, and the website.

## 1.0.16 — July 2026 💸

### Added
- Food stalls on community lots now have a vendor ready to serve. 🌭

### Changed
- Bills are now switched on: daily property charges, and metered lamp, stall, and stereo hours, begin billing lot owners. 💸

## 1.0.15 — July 2026 📬

### Added
- **Property Statistics** — the house panel's Statistics button now works: interior area, bedrooms, bathrooms, floors, and lot size, plus live 0–10 ratings for Size, Furnishings, Yard, Upkeep (object wear), and Layout. 📊
- **Activity Log** — the house panel's Log button now works, for roommates: recent visitors with times, current roommates with move-in dates, and server events. 📋
- **Bills** — daily property bills, replicating original TSO. Bills charge the lot owner only for days the lot is actually used, scale with lot size and floors, and meter lamp-on, open-stall, and stereo-on hours. Metered charges land on the bill the moment the paper carrier tucks the bills into your mailbox on her morning rounds (using the original mail-NPC animation), with a final settle-up when the lot closes. Pay at your mailbox (owners and roommates) or in the Budget Window's Bills tab, where outstanding bills appear by day and paid bills show in Expenses. Overdue bills escalate: first visitors are locked out, then build and buy are disabled (selling back is still allowed), and eventually billing pauses until the balance is paid. Rates start at zero — bills only begin if the server enables them. 💸
- Food stalls (the hot dog cart, the ice cream cart, and friends) start open and stocked on community lots. 🌭

### Changed
- Ambience volume now defaults to 80% for new installations (it was nearly silent). 🔊
- The Budget Window's selected tab now connects seamlessly into its detail panel.

### Fixed
- Object expenses — pet food refills, and other refills, maintenance, and miscellaneous costs — now show up in the Budget Window's Expenses tab. They previously never reached the ledger. 🐾

## 1.0.14 — July 2026 💰

### Added
- **Budget Window** — the budget button in the UCP now opens a working window. 💰 Tabs cover your Cash (day-by-day money flow), Net Worth (cash, money in objects, and object value), Income, and Expenses, with income and expenses broken down by category over the last 30 days. The Bills tab is not implemented yet.
- The ledger behind the Budget Window now records catalogue purchases, sell-backs, object upgrades, build-mode spending, and lot expansions (from this update onward), each under its own category — purchases, upgrades, building, and lot expansion as expenses, and sell-backs as income.
- Daily Top-100 payouts now show in the Budget Window's Income tab as their own categories — visitor bonus, property bonus, and sim bonus — including your recent bonus history.

## 1.0.13 — July 2026 🖱️

### Fixed
- City screen: edge scrolling no longer fires while the mouse is over a UI window, and clicks that start on a UI element (e.g. dragging a window aside) no longer fall through to the city and zoom into a lot. 🖱️
- City screen: the bottom and right edge-scroll zones now sit at the actual screen edges — they previously covered a large part of the screen at higher resolutions. 📐

## 1.0.12 — July 2026 💾

### Changed
- Newer clients are now allowed in: the server's version is treated as a minimum, so a client ahead of the server connects and plays normally instead of being told to update. Older clients are still prompted to update.
- Client settings (graphics, audio, chat, and more) now live in your user profile (`AppData/DiscoSO` on Windows, `~/.config/DiscoSO` on Linux) and survive client updates and reinstalls. Existing settings migrate over automatically. ⚙️

### Fixed
- Top-100 property bonuses now pay only for days the lot actually opened — days where nobody visited the property at all no longer pay out. 🏠

## 1.0.11 — July 2026 🎨

### Added
- **Build-mode eyedropper** — middle-click in build mode samples the floor or wallpaper under the cursor and selects it in the catalog, ready to place. With the wallpaper category open, walls are sampled in preference to floors. Middle-drag camera rotation is unchanged. ([#1](https://github.com/TheDiscordian/DiscoSO/pull/1))

## 1.0.10 — July 2026 🐾

### Changed
- Pet social decay is now tuned separately from the quiet-server bonus, so pets want attention at a sensible rate no matter how the player bonuses shift: 2× decay when 1–4 players are online, and 1.5× at 5–6.

## 1.0.5 – 1.0.9 — July 2026 🔧

### Added
- A 1.5× bonus tier at 5–6 players online — bonus tiers now support fractional multipliers.

### Changed
- The Pre-Chlorination Station outdoor shower retuned: a sensible fill rate and a catalogue rating that matches its ability. 🚿
- Server-sent strings rebranded: DiscoSO Server broadcasts, DiscoSO Staff mail senders, and mail templates pointing at tso.thedisco.zone.

## DiscoSO launch 🪩 — 2026

What DiscoSO changed from stock FreeSO at launch.

### Gameplay 🎮
- **Quiet-server bonuses**: 3× / 2× / 1.5× skill and money gain when 1–2 / 3–4 / 5–6 players are online (normal at 7+), with social decay slowed to match so lone Sims stay happy. Applies on welcome lots too, stacking with their built-in double rate.
- **Skill locks that grow with you**: total skill points cap at 20 + a third of your days played.
- **Live simoleon gifts** 💸 — staff gifts arrive in your budget instantly, no relog needed.

### Client 💿
- DiscoSO branding throughout.

### Launcher 🚀 (v1.0.0 – v1.0.4)
- Forked from the upstream FreeSO launcher: rebranded and repointed at DiscoSO hosting and news, Linux dependency handling (including Arch), and Mac builds.

### Website 🌐
- [tso.thedisco.zone](https://tso.thedisco.zone) — homepage, downloads, and news (which also feeds the launcher's blog panel).

---

DiscoSO is powered by the [FreeSO](https://freeso.org) engine (MPL-2.0).
