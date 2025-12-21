# Repository Rules

## Source of Truth
- Confluence is the source of truth for specifications.
- `SpecsMirror/` is generated output and must not be edited manually.
- Confluence sync is restricted to the `RSPCore` space.
- Specs live under the `Specs` folder in Confluence and architecture docs live under the `Architecture` folder in Confluence.
  - Architecture folder example: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/folder/86737290

## Pull Requests
- All changes go through PRs.
- PR body must include:
  - Feature-Key (e.g., `FTR-0123`)
  - Confluence page ID or URL

## Migrations
- Any behavior change requires either:
  - a corresponding Confluence spec update, or
  - a migration document in `Docs/Migrations/`.

## CI Expectations
- Keep workflows green (restore, build, test).
- Do not introduce secrets in code or documentation; reference only secret names.
