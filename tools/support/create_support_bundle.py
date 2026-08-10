#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import re
import zipfile
from datetime import datetime, timezone
from pathlib import Path

BLOCKED = re.compile(r"(?i)(secret|password|token|private[_-]?key|authorization|cookie|session)")
ALLOW_NAMES = {"platform.yaml", "manifest.json", "CHECKSUMS.txt"}


def safe_text(path: Path) -> str:
    lines = []
    for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        lines.append("[REDACTED]" if BLOCKED.search(line) else line)
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path("."))
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = args.root.resolve()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    metadata = {
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "generator": "PeopleSyncD sanitized support bundle",
        "warning": "Review before sharing; no customer or workforce data should be included.",
    }
    with zipfile.ZipFile(args.output, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("bundle-metadata.json", json.dumps(metadata, indent=2) + "\n")
        for path in root.rglob("*"):
            if not path.is_file() or path.name not in ALLOW_NAMES:
                continue
            archive.writestr(path.relative_to(root).as_posix(), safe_text(path))
    print(f"Created sanitized support bundle: {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
