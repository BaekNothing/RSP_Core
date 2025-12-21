# Confluence Spec Export Format (for automated checks)

This document defines the expected **Markdown export format** for Confluence specs so
other agents can validate whether a zipped export follows the required structure and purpose.

## Purpose
- Validate that a **zip archive** containing Confluence-exported Markdown mirrors the
  expected structure and includes the required human-authored fields.
- Ensure specs remain the **source of truth** and are aligned with the repository rules.

## Expected Space Scope
- Space: `RSPCore`
- Folders:
  - `Specs` (feature specifications)
  - `Architecture` (system design / decisions)

## Zip Layout Requirements
The zip is expected to contain a **single top-level folder** that represents the space.
Example:

```
RSPCore/
  Specs/
    FTR-0123 Feature Name/
      spec.md
      meta.json
  Architecture/
    ADR/
      ADR-0001 Title/
        spec.md
        meta.json
```

### Rules
1. **Exactly one top-level folder** (e.g., `RSPCore/`).
2. `Specs/` and `Architecture/` must exist under the top-level folder.
3. Each spec lives under its own folder and contains:
   - `spec.md` (required)
   - `meta.json` (optional but recommended)

## `spec.md` Format (Feature Specs)
`spec.md` must include the following **human-authored fields**:

```
# <Feature Title>

Feature-Key: FTR-XXXX

## 목적 / Purpose
<free text>

## 범위 / Scope
- In-scope:
- Out-of-scope:

## Acceptance Criteria (AC)
- [ ] AC-1
- [ ] AC-2

## 연관 페이지 링크 (Optional)
- <url>

## 영향 도메인 태그 (Optional)
- Combat, Quest, Data, UI, Networking, ...
```

### Mandatory fields checklist
- `Feature-Key` (must match `FTR-` prefix)
- `Purpose`
- `Scope` (in-scope/out-of-scope)
- `Acceptance Criteria`

### Optional fields
- Related links
- Domain tags

## `spec.md` Format (Architecture Docs)
Architecture docs may use this minimal structure:

```
# <Title>

Doc-Type: Architecture

## Summary
<free text>

## Decision / Design
<free text>

## Impact
<free text>
```

## `meta.json` (Optional)
If present, `meta.json` should be JSON and may include:

```json
{
  "page_id": "123456",
  "space_key": "RSPCore",
  "fetched_at_utc": "2025-01-01T00:00:00Z",
  "confluence_version": 12,
  "canonical_url": "https://baeknothing.atlassian.net/wiki/..."
}
```

## Validation Guidelines (for the zip checker)
An automated validator should confirm:
1. The zip contains the top-level folder and required subfolders.
2. Each spec folder has a `spec.md`.
3. `spec.md` includes all mandatory fields.
4. `Feature-Key` matches `FTR-[0-9]{4,}`.
5. If `meta.json` exists, it is valid JSON.

## Notes
- `SpecsMirror/` in this repo is generated output and not the source of truth.
- The Confluence sync workflow is restricted to the `RSPCore` space.
