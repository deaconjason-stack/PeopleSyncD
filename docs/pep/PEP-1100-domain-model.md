# PEP-1100: Domain Model

- Status: Accepted

The canonical domain separates Person, Worker, Organization, Board Appointment, Document, Workflow, Approval, Notification, and Audit Event. Identity is not employment; employment is not governance; governance is not ordinary management. Cross-domain references use stable identifiers and tenant scope. Sensitive attributes are minimized, encrypted where required, and exposed only through authorized service contracts.
