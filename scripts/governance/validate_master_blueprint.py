#!/usr/bin/env python3
"""Validate the PeopleSyncD Enterprise Master Blueprint and governed indexes."""

from __future__ import annotations

import csv
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BLUEPRINT_PATH = ROOT / "master-blueprint.json"
MASTER_DOCUMENT = ROOT / "PEOPLESYNCD-ENTERPRISE-MASTER-BLUEPRINT-V1.0.md"
TRACEABILITY_PATH = ROOT / "docs/traceability/master-lifecycle.csv"

EXPECTED_BOARD = [
    "Jason Henderson",
    "Domonique Danielle Henderson",
    "Marietta Jessup",
]

EXPECTED_DOMAINS = [f"{index:02d}-{name}" for index, name in enumerate([
    "Governance",
    "Business",
    "Product",
    "AI",
    "Platform",
    "Data",
    "Security",
    "Engineering",
    "Customer",
    "Ecosystem",
    "Operations",
    "Legal",
    "Research",
    "Deployment",
])]

EXPECTED_SUPPORTING_INDEXES = [
    "architecture/README.md",
    "diagrams/README.md",
    "decisions/README.md",
    "examples/README.md",
    "code/README.md",
]

EXPECTED_PHASES = [
    "foundation",
    "mvp",
    "enterprise",
    "platform",
    "global",
    "intelligence",
]

REQUIRED_TRACEABILITY_COLUMNS = [
    "capability_id",
    "business_goal",
    "product_epic",
    "feature",
    "requirement",
    "specification",
    "architecture_decision",
    "service_or_component",
    "contract",
    "implementation",
    "test_evidence",
    "release_evidence",
    "customer_documentation",
    "status",
]


def fail(message: str) -> None:
    raise ValueError(message)


def require_file(path: Path) -> None:
    if not path.is_file():
        fail(f"Required file is missing: {path.relative_to(ROOT)}")


def validate_master_document() -> None:
    require_file(MASTER_DOCUMENT)
    content = MASTER_DOCUMENT.read_text(encoding="utf-8")
    required_markers = [
        "PSD-MASTER-001",
        "Version:** 1.0.0",
        "Single Authoritative Source",
        "Master Traceability Model",
        "Maturity Roadmap",
        "Definition of Done",
        "Release and Certification Truthfulness",
    ]
    for marker in required_markers:
        if marker not in content:
            fail(f"Master document is missing required marker: {marker}")


def validate_blueprint_json() -> dict[str, object]:
    require_file(BLUEPRINT_PATH)
    data = json.loads(BLUEPRINT_PATH.read_text(encoding="utf-8"))

    if data.get("documentId") != "PSD-MASTER-001":
        fail("master-blueprint.json must identify PSD-MASTER-001")
    if data.get("version") != "1.0.0":
        fail("master-blueprint.json must be version 1.0.0")
    if data.get("board") != EXPECTED_BOARD:
        fail("Authoritative board does not match the approved three-member board")

    domains = data.get("domains")
    if not isinstance(domains, list):
        fail("domains must be a list")
    domain_ids = [entry.get("id") for entry in domains if isinstance(entry, dict)]
    if domain_ids != EXPECTED_DOMAINS:
        fail(f"Numbered domains must exactly match: {EXPECTED_DOMAINS}")

    for entry in domains:
        if not isinstance(entry, dict):
            fail("Every domain entry must be an object")
        index_path = entry.get("index")
        if not isinstance(index_path, str):
            fail("Every domain must define an index path")
        require_file(ROOT / index_path)
        content = (ROOT / index_path).read_text(encoding="utf-8")
        for marker in ("**Domain ID:**", "## Canonical sources", "## Completion gate"):
            if marker not in content:
                fail(f"{index_path} is missing required marker: {marker}")

    supporting = data.get("supportingIndexes")
    if supporting != EXPECTED_SUPPORTING_INDEXES:
        fail("Supporting indexes do not match the controlled blueprint")
    for index_path in EXPECTED_SUPPORTING_INDEXES:
        require_file(ROOT / index_path)

    layers = data.get("layers")
    if not isinstance(layers, list) or len(layers) != 9:
        fail("The layered architecture must contain exactly nine governed layers")
    layer_ids = [entry.get("id") for entry in layers if isinstance(entry, dict)]
    if len(layer_ids) != len(set(layer_ids)):
        fail("Layer identifiers must be unique")

    phases = data.get("maturityPhases")
    if not isinstance(phases, list):
        fail("maturityPhases must be a list")
    phase_ids = [entry.get("id") for entry in phases if isinstance(entry, dict)]
    if phase_ids != EXPECTED_PHASES:
        fail(f"Maturity phases must be ordered as: {EXPECTED_PHASES}")

    boundary = data.get("currentReleaseBoundary")
    if not isinstance(boundary, dict):
        fail("currentReleaseBoundary must be an object")
    prohibited_true_claims = [
        "productionCertified",
        "customerDeploymentApproved",
        "signedDistribution",
    ]
    for claim in prohibited_true_claims:
        if boundary.get(claim) is not False:
            fail(f"Current pre-production boundary must keep {claim}=false")

    traceability = data.get("traceability")
    if not isinstance(traceability, dict):
        fail("traceability must be an object")
    if traceability.get("registry") != "docs/traceability/master-lifecycle.csv":
        fail("Machine-readable traceability registry path is incorrect")
    chain = traceability.get("chain")
    if not isinstance(chain, list) or len(chain) < 12:
        fail("Traceability chain is incomplete")

    return data


def validate_traceability() -> int:
    require_file(TRACEABILITY_PATH)
    with TRACEABILITY_PATH.open(newline="", encoding="utf-8") as handle:
        reader = csv.DictReader(handle)
        if reader.fieldnames != REQUIRED_TRACEABILITY_COLUMNS:
            fail("Master traceability columns do not match the required lifecycle schema")
        rows = list(reader)

    if not rows:
        fail("Master traceability registry must contain capabilities")

    identifiers: set[str] = set()
    for row_number, row in enumerate(rows, start=2):
        capability_id = row["capability_id"].strip()
        if not capability_id.startswith("PSD-CAP-"):
            fail(f"Row {row_number} has an invalid capability identifier")
        if capability_id in identifiers:
            fail(f"Duplicate capability identifier: {capability_id}")
        identifiers.add(capability_id)

        for column in REQUIRED_TRACEABILITY_COLUMNS:
            if not row[column].strip():
                fail(f"Row {row_number} is missing {column}")

    return len(rows)


def main() -> int:
    try:
        validate_master_document()
        blueprint = validate_blueprint_json()
        capability_count = validate_traceability()
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"Master Blueprint validation failed: {error}", file=sys.stderr)
        return 1

    print(
        "Master Blueprint validation passed: "
        f"version={blueprint['version']} "
        f"domains={len(blueprint['domains'])} "
        f"layers={len(blueprint['layers'])} "
        f"capabilities={capability_count}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
