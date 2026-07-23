# DiscoSO Changelog 📜

Player-facing changes to DiscoSO — the game client and server, the launcher, and the website. Game versions follow the server's version gate (shown in-game as e.g. `1.0.10-0`).

## 1.0.13 — July 2026 🖱️

### Fixed
- City screen: edge scrolling no longer fires while the mouse is over a UI window, and clicks that start on a UI element (e.g. dragging a window aside) no longer fall through to the city and zoom into a lot. 🖱️
- City screen: the bottom and right edge-scroll zones now sit at the actual screen edges — they previously covered a large part of the screen at higher resolutions. 📐

## 1.0.12 — July 2026 💾

### Changed
- Smarter update check: the client now updates only when it is older than the server's version, so optional client releases no longer force everyone to re-download.
- Client settings (graphics, audio, chat, and more) now live in your user profile (`AppData/DiscoSO` on Windows, `~/.config/DiscoSO` on Linux) and survive client updates and reinstalls. Existing settings migrate over automatically. ⚙️

### Fixed
- Top-100 property bonuses now pay only for days the lot actually opened — days where nobody visited the property at all no longer pay out. 🏠

## 1.0.11 — July 2026 🎨

### Added
- **Build-mode eyedropper** — middle-click in build mode samples the floor or wallpaper under the cursor and selects it in the catalog, ready to place. With the wallpaper category open, walls are sampled in preference to floors. Middle-drag camera rotation is unchanged. ([#1](https://github.com/TheDiscordian/DiscoSO/pull/1))

## 1.0.10 — July 2026 🐾

### Changed
- Pet social decay is now tuned separately from the quiet-server bonus, so pets want attention at a sensible rate no matter how the player bonuses shift: 2× decay when 1–2 players are online, 2× at 3–4, and 1.5× at 5–6.
- The 1–2 players online bonus tier is restored to 3× skill and money gain.

## 1.0.5 – 1.0.9 — July 2026 🔧

### Added
- A 1.5× bonus tier at 5–6 players online — bonus tiers now support fractional multipliers.
- A custom outdoor shower, the Pre-Chlorination Station, tuned and rated for its price point. 🚿

### Changed
- Server-sent strings rebranded: DiscoSO Server broadcasts, DiscoSO Staff mail senders, and mail templates pointing at tso.thedisco.zone.
- Shower upgrade ladders restored to stock tuning after an audit of object stats against real gameplay rates.

### Fixed
- Crash when studying skills on some clients (kept binary compatibility for the shipped client).
- Skill-object payouts now display the bonus-multiplied amount instead of the base amount.
- The in-client auto-update path repaired end-to-end — version-mismatched clients now download and install the current client cleanly.

## DiscoSO launch 🪩 — 2026

The initial DiscoSO feature set, on top of the FreeSO engine.

### Gameplay 🎮
- **Quiet-server bonuses**: 3× / 2× / 1.5× skill and money gain when 1–2 / 3–4 / 5–6 players are online (normal at 7+), with social decay slowed to match so lone Sims stay happy.
- **Skill locks that grow with you**: total skill points cap at 20 + a third of your days played.
- **Welcome lots pay double** for new players, stacking with the quiet-server bonus.
- **Live simoleon gifts** 💸 — staff gifts arrive in your budget instantly, no relog needed.
- **Community lots** — staff-maintained, always-open downtown-style lots.
- **Curated catalogue** with seasonal items, custom objects, and per-item availability.

### Client 💿
- Version-gated auto-update: outdated clients are prompted and updated in-client.
- DiscoSO branding throughout.

### Launcher 🚀 (v1.0.0 – v1.0.4)
- Full DiscoSO launcher for Windows, Linux, and Mac: installs the game and client in one flow.
- Linux dependency handling (including Arch), self-update notifications, and the DiscoSO news feed.

### Server 🛠️
- Graceful restarts: in-game countdown announcements, and every lot saves before shutdown.
- In-game events system, staff mail, and moderation tooling.

### Website 🌐
- [tso.thedisco.zone](https://tso.thedisco.zone) — homepage, downloads, and news (which also feeds the launcher's blog panel).

---

DiscoSO is powered by the [FreeSO](https://freeso.org) engine (MPL-2.0).
