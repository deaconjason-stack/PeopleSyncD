#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import re
import uuid
from datetime import datetime, timezone
from pathlib import Path


def component(name: str, version: str, kind: str = "library") -> dict:
    return {
        "type": kind,
        "bom-ref": f"pkg:generic/{name}@{version}",
        "name": name,
        "version": version,
    }


def discover(root: Path) -> list[dict]:
    found: dict[str, dict] = {}
    for package_json in root.rglob("package.json"):
        if "node_modules" in package_json.parts:
            continue
        try:
            data = json.loads(package_json.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        for group in ("dependencies", "devDependencies", "optionalDependencies"):
            for name, version in data.get(group, {}).items():
                found[f"npm:{name}"] = component(name, str(version))
    for requirements in root.rglob("requirements*.txt"):
        for raw in requirements.read_text(encoding="utf-8").splitlines():
            line = raw.strip()
            if not line or line.startswith("#"):
                continue
            match = re.match(r"([A-Za-z0-9_.-]+)(?:==|>=|~=|<=|>|<)?(.+)?", line)
            if match:
                found[f"pypi:{match.group(1)}"] = component(match.group(1), match.group(2) or "unspecified")
    return sorted(found.values(), key=lambda item: item["name"].lower())


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path("."))
    parser.add_argument("--version", required=True)
    parser.add_argument("--commit", required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = args.root.resolve()
    bom = {
        "bomFormat": "CycloneDX",
        "specVersion": "1.5",
        "serialNumber": f"urn:uuid:{uuid.uuid4()}",
        "version": 1,
        "metadata": {
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "component": component("PeopleSyncD", args.version, "application"),
            "properties": [
                {"name": "peoplesyncd:sourceCommit", "value": args.commit},
                {"name": "peoplesyncd:generator", "value": "scripts/certification/generate_sbom.py"},
            ],
        },
        "components": discover(root),
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(bom, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {len(bom['components'])} components to {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
