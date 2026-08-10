#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
from pathlib import Path

REQUIRED = {
    "README.md",
    "RELEASE_NOTES.md",
    "INSTALLATION_GUIDE.md",
    "UPGRADE_GUIDE.md",
    "KNOWN_ISSUES.md",
    "COMPATIBILITY_MATRIX.md",
    "SECURITY_REPORT.md",
    "PERFORMANCE_REPORT.md",
    "ACCESSIBILITY_REPORT.md",
    "TRACEABILITY_MATRIX.md",
    "BACKUP_RESTORE_EVIDENCE.md",
    "DISASTER_RECOVERY_EVIDENCE.md",
    "SBOM_STATUS.md",
    "CHECKSUMS.txt",
    "SIGNATURES.md",
    "CERTIFICATION.md",
    "manifest.json",
}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("release_dir", type=Path)
    parser.add_argument("--allow-draft", action="store_true")
    args = parser.parse_args()
    root = args.release_dir
    missing = sorted(name for name in REQUIRED if not (root / name).is_file())
    if missing:
        raise SystemExit("Missing release evidence: " + ", ".join(missing))
    manifest = json.loads((root / "manifest.json").read_text(encoding="utf-8"))
    certified = bool(manifest.get("certified"))
    if certified and manifest.get("status") != "certified":
        raise SystemExit("Certified manifest must have status=certified")
    if not args.allow_draft and not certified:
        raise SystemExit("Release is not certified")
    if certified:
        evidence = manifest.get("evidence", {})
        incomplete = sorted(key for key, value in evidence.items() if value is not True)
        if incomplete:
            raise SystemExit("Certified release has incomplete evidence: " + ", ".join(incomplete))
    print(f"Release evidence structure valid; certified={certified}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
