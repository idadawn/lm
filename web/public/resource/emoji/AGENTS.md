<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# emoji

## Purpose
Static emoji sprite library — animated GIFs numbered `100.gif` through ~`200+.gif` used by chat/IM and comment-style components. Asset-only content shipped with the SPA and served at `/resource/emoji/<n>.gif`.

## For AI Agents

### Working in this directory
- This is a binary asset directory. Do not write code files here.
- Filename convention: zero-padded? No — files are named `<index>.gif` starting at 100. The numeric index is what UI components look up; do not rename.
- When adding a new emoji, append the next sequential index and update the picker source list (likely under `src/components/` chat/IM module) to reference it.
- File sizes range from ~1KB to ~12KB — keep new entries similarly small.

## Dependencies
### Internal
- Referenced by IM/chat picker components in `src/`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
