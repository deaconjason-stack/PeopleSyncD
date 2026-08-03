# Kubernetes Production Architecture

The `base/` directory contains a secure template, not a certified production deployment. It establishes namespace isolation, service accounts, non-root containers, resource limits, disruption protection, health probes, configuration separation, and default-deny networking.

Production overlays must supply signed image digests, managed secret references, storage classes, ingress, certificates, backup integration, telemetry destinations, policy controls, and environment-specific evidence.
