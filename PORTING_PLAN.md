# C# Port Gap Analysis & Fix Plan

Compares `original/legion_3_1.Asc` (AMOS Basic source, 9015 lines, 117
`Procedure`/`Function` blocks) against the C# port in
`src/AmigaNet.Legion/AmigaNet.Legion` and its supporting infra projects
(`AmigaNet.Amos`, `AmigaNet.IO`, `SharpMod.Core`).

**How to use this file:** each item is self-contained — file:line refs, root
cause, concrete steps — so it can be picked up without re-reading the AMOS
source or re-doing the audit. Only open items are kept here; finished work
is dropped once done rather than archived (see git diffs / prior
conversation history if the "why" behind a past fix is ever needed again).

**Audit baseline (still holds, no need to re-derive):** 107 of 117 AMOS
procedures have a direct 1:1 C# counterpart. The 10 without one are
intentionally, correctly omitted — see Appendix. Save/load
(`LegionArchive.cs`) and data loaders (`LegionDataLoader.cs`) were verified
correct against the AMOS source, no action needed there.

**Completed this pass (2026-07-10 to 2026-07-11):** palette `Fade`, intro
sword animation, SFX sample loading/playback (modes 8/9), shop background/
equipment-slot transparency, creature/Bob Y-depth draw order (visually
confirmed working by the user), an Amal-thread race condition crash (found
during play, not previously tracked here), and both previously-"uncertain"
spots below (`ODLEG`'s redundant `Abs()` and `GADKA`'s `LOSUJ2` goto
restructuring) — both traced against the AMOS source and confirmed to be
correct, faithful ports; no code changes needed beyond replacing the `TODO`
comments with notes explaining why.

## Open items

### 1. [ ] Mode 7: regional background music not loaded

`_LOAD` TRYB 7 (`src/AmigaNet.Legion/AmigaNet.Legion/Legion.cs`, in `_LOAD`)
is still a no-op `//TODO`. AMOS "Music" banks (`mus-las`, `mus-step`,
`mus-gory`, `mus-pustnia`, `mus-zima`, `mus-bagna`, `mus-jaskinia`,
`mus-grota` under `original/legion/dane/muzyka/`) are a **proprietary
AMOS tracker format**, confirmed structurally different from standard
MOD/XM/S3M (which `SharpMod.Core` already supports and plays fine for
`mod.intro`) — not just a header difference, the actual note/pattern data
is encoded differently: *"AMOS Professional Music is internally different
from the standard Soundtracker format... not coded in parallel but in a
more efficient track system... delays between each note are not fixed as in
Soundtracker, but coded in the note itself"* (AMOS Pro manual, Appendix E).

**What's confirmed about the format** (header-level only, NOT enough to
implement playback):
- Same 20-byte `AmBk` header as Samples banks (see `AmigaNet.IO/Audio/Amos/SampleBanksReader.cs`
  for that header format, already implemented and validated for modes 8/9),
  name `"Music   "`.
- Followed by a "main header": 4-byte offset to instruments data, 4-byte
  offset to songs data, 4-byte offset to patterns data, 4 bytes always 0
  (offsets measured from the start of this main header; the three sections
  may appear in any order).
- Instruments data: 2-byte instrument count, then 0x20 (32) bytes per
  instrument (offset to sample data + offset to repeat/loop section + more,
  exact field layout not confirmed), with raw sample data following — this
  part is likely reusable with tweaks to `SampleBanksReader`'s logic, since
  it's presumably the same "don't trust the length field, derive from
  offsets" situation confirmed there.
- Songs data: list of offsets to songs; a "playlist" is a stream of 2-byte
  words listing which patterns to play in order.
- **Pattern/note/effect encoding: not found.** This is the piece that
  actually matters for correct playback and no source reached during
  research spelled it out byte-by-byte. Don't guess at this — there's no way
  to verify correctness without hearing output, and garbled audio is worse
  than silence.

**Fix steps:**
1. Get the actual byte-level pattern encoding before writing any parsing
   code — try fetching `https://amospromanual.dev/14-appendix-e-memory-bank-structures.html`
   again (404'd repeatedly last session, might be transient) or
   `https://www.exotica.org.uk/wiki/AMOS_Music_Bank_format` (Cloudflare
   blocked the fetch tool last time — might work via a real browser), or
   check `kyz/amostools` on GitHub for full pattern-decoding logic
   (`dschwen/amosbank` was checked and doesn't have it — its README instead
   points to an AmigaOS-only tool, "Abk2Mod-II", to convert these to real
   MOD files, which is the community's own workaround, not a from-scratch
   parser to lean on).
2. Once the spec is solid, add a `MusicBankReader` alongside
   `SampleBanksReader` producing a `SongModule`-compatible structure (check
   `SharpMod.Core/Module.cs`/`Song/` for the shape `MODLoader.cs` builds) so
   it can go through the *existing*, already-working
   `ModulePlayer`/`gameEngine.LoadTrack` pipeline instead of writing a new
   player.
3. Call sites are in `LegionRysujScenerie.cs` (ported from AMOS
   `RYSUJ_SCENERIE`, ~lines 4369-4858) — 8 biomes, each currently silent.
