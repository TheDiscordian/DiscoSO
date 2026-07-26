# DiscoSO Changelog 📜

Player-facing changes to DiscoSO — the game client and server, the launcher, and the website.

## Server settings — July 2026 💪

Changes made live by server settings alone. They carry no version because they need no new client.

### Changed
- Needs can now overfill past the top of the bar — shown as a white bar — and how far depends on where you are. Romance lots push Social hardest, Entertainment lots push Fun and Social, Services lots push Hunger and Energy, and home lots give a solid top-up to the survival needs. Welcome and Community lots add a light boost to everything. 💪

## 1.0.37 — July 2026 🎃

### Fixed
- Closing the game takes you offline right away. The server was not acting on connections a departed client had already hung up on, so the session stayed open and you stayed on the online list until a ten-minute idle timer swept it away. 👻
- Sitting still no longer disconnects you. That idle timer was only ever there to clear up after the bug above, and it closed anyone who had been quiet for ten minutes — easily done sitting in the city view. 🪑

### Changed
- Holiday items are only in the catalogue during their holiday: Halloween Sep 15 – Nov 15 🎃, Christmas Nov 16 – Jan 31 🎁, Spring Mar 1 – May 31 🦋, Pride all of June 🌈, Summer Jul 1 – Aug 31 🌴. Out of season they are hidden from buy and build mode and cannot be bought. Ones you already own or have in inventory are unaffected — you can keep them out and place them any time of year.

## 1.0.36 — July 2026 🧹

### Fixed
- The maid now cleans objects at 450 dirt and above, down from 800. Applies in sandbox too. 🧹

## 1.0.35 — July 2026

No player-facing changes — groundwork for the maid fix in 1.0.36.

## 1.0.34 — July 2026 🏘️

### Fixed
- The city view no longer sits on the Welcome filter showing an empty list. If no welcome lots are online it now falls through to Community, even when the server has nothing at all to say about welcome lots. 🏘️

## 1.0.33 — July 2026 🧹

### Changed
- The maid and gardener stop loitering on community lots once the work is done. They used to stand around for about three minutes before heading off; now it is closer to twenty-five seconds. 🧹🌱

## 1.0.32 — July 2026 🧹

### Changed
- The two community-lot maids now work proper shifts: the 10am maid finishes at 6pm, and the 7pm maid stays through to 1am. 🧹

## 1.0.31 — July 2026 🧹

### Fixed
- The maid and gardener changes from 1.0.30 now apply to community lots only. They were going out to every property, so home lots were getting a daily gardener and a 7pm maid call they were never meant to have. 🏡
- The maid's 7pm visit actually sticks around. She knocks off at 6pm by default, so the evening call added in 1.0.30 arrived and turned straight back around. On community lots she now works until 10pm — and still leaves early once there is nothing left to do. 🧹

## 1.0.30 — July 2026 🧹

### Changed
- The maid now comes to community lots twice a day — her usual 10am round plus a second visit at 7pm — and the gardener turns up every day instead of every third. 🧹🌱

## 1.0.29 — July 2026 🧾

### Changed
- Metered charges (lights, stalls, stereos, TVs) now land on your bill at 7am in game, the same moment the paper carrier makes her round — instead of the instant you close your lot. Leaving your property no longer bills you on the way out, and a lot that is offline still settles up on schedule. 🧾

## 1.0.28 — July 2026 🏘️

### Changed
- The city view opens with the Community filter already selected, so the lots that are actually open show up without hunting for them. Sims in their first 14 days still get pointed at Welcome lots instead — but only while some are running, otherwise they land on Community as well. 🏘️

### Fixed
- City view lot bubbles no longer sit behind the UCP or the gizmo. They now treat the panels as obstacles the same way they already avoided each other. 🫧

## 1.0.27 — July 2026 📊

### Fixed
- Property statistics now count donated furniture. Donating an object gives up its resale value, and everything on a community lot is donated, so Furnishings, Yard, and Upkeep read off almost nothing however well furnished the property was. Objects with no resale value are now counted at their catalogue price. 📊

## 1.0.26 — July 2026 🚽

### Fixed
- Objects the game shipped without a catalogue picture now show their drawn icon in the action queue, not just in the catalogue. 🚽

## 1.0.25 — July 2026 🍽️

### Fixed
- Quick meals cost §8 in sandbox mode too. The lower price was a live-server setting, so offline play was still charging the old §10. 🍽️

## 1.0.24 — July 2026 🧾

### Added
- Sandbox mode now runs lot bills. Lights, open food stalls, playing stereos and switched-on TVs cost money by the game hour, the paper carrier delivers the bill to the mailbox, and Pay Bills settles it from your budget — the same rules the live server uses. The daily property fee stays online-only, since a sandbox lot has no size on record. 💸

### Changed
- Quick meals now cost §8 from the fridge's food stock, down from §10. 🍽️

## 1.0.23 — July 2026 💸

### Changed
- Sims can hand each other up to §30,000 at a time. 💸

## 1.0.22 — July 2026 📸

### Changed
- Staff can now refresh a community lot's city-view preview by opening and closing Buy or Build mode on the lot — previously a community lot without an owner could never update its snapshot. 📸
- Sims can hand each other up to §29,999 at a time, up from §999. 💸

### Fixed
- Objects the original game shipped without a catalogue picture — the Hygeia-O-Matic Toilet, the Flush Force 5 XLT, Hats Off to 2005 and a handful of others — now draw their own icon instead of showing an empty square. 🖼️

## 1.0.21 — July 2026 📬

### Changed
- The mailbox's Pay Bills option now appears the moment the paper carrier finishes tucking the bills into the box — not before she gets there. 📬

## 1.0.20 — July 2026 📭

### Changed
- Election residency requirements now scale with a neighbourhood's age, so young neighbourhoods can hold their first elections — long-time residents qualify even before the full 30 days exist. 🗳️
- The paper carrier now only visits the mailbox when there are actually bills to deliver — with a $0 balance she drops the paper and moves on, and the mailbox no longer offers Pay Bills. 📬

## 1.0.19 — July 2026 👻

### Changed
- TV-on hours now join the metered bills, at $1 per game hour. 📺
- Bills are now paid at your mailbox — the authentic walk to the kerb. Paying from the Budget Window is off by default, as a server option for those who want it. 📬

### Fixed
- Sims no longer show as online in the city long after their player disconnected — the client now fully exits when closed (no more background zombie processes), and the server drops dead connections after a few minutes. 👻
- Maids no longer give up and leave when they can't step inside a toilet stall to clean it — they clean it from the doorway instead. 🚽

## 1.0.18 — July 2026 💡

### Changed
- The Budget Window's Bills tab now shows your outstanding balance — $0 once everything is paid — instead of falling back to your bill history total. 💸

### Fixed
- Lamps no longer get permanently stuck ignoring their auto setting after being switched on or off by hand — toggling auto now fully resets the manual override. 💡

## 1.0.17 — July 2026 🔄

### Changed
- New Sims now start with $150. 💵
- The Budget Window now refreshes itself while open, so bills paid at the mailbox, and fresh metered charges, show up right away. 🔄

### Fixed
- Refilling a pet food bowl now counts under Refills and maintenance in the Budget Window, instead of From objects. 🐾

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
