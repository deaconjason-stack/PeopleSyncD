# CareSyncD Permanent — Spec Self-Review

**Date:** 2026-09-22  
**Design spec:** `docs/superpowers/specs/2026-09-22-caresyncd-permanent-design.md`  
**Branch:** `caresyncd-pages`

## Review result

The approved design is internally consistent on the core constraints: permanent HTTPS hosting, no PC dependency, no paid runtime requirement, browser-native simulation, IndexedDB persistence, offline/PWA behavior, preservation of all 8 missions and 3 shifts, and a future cloud adapter boundary.

No unresolved placeholders or unsupported present-tense claims were found.

## Hosting correction discovered during review

GitHub Pages supports one Pages site per repository. Because the current staging branch lives inside the `PeopleSyncD` repository, enabling production Pages there could consume or conflict with a future PeopleSyncD Pages site.

Therefore:

1. `caresyncd-pages` is the staging branch for the permanent conversion.
2. The production CareSyncD Pages deployment should live in a dedicated CareSyncD repository when repository creation/migration is available.
3. Do not enable PeopleSyncD's repository Pages for CareSyncD unless Jason explicitly chooses that tradeoff.
4. The static build must remain repository-subpath safe so it can be moved to a dedicated repo without code changes.
5. The original server-based CareSyncD v3 release remains preserved separately.

## Implementation gate

Implementation must not begin until Jason reviews and approves the written design specification and this self-review correction. After that approval, create the implementation plan before changing product code.
