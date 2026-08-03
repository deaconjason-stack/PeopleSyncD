# PEP-2020: Organization Service

- Status: Accepted

The Organization service owns tenant identity, hierarchy, settings, branding, locations, memberships, edition configuration, and active organization context. It is the root tenant boundary and must reject cross-organization identifiers before downstream processing. Initial requirement namespace: `REQ-ORG-*`.
