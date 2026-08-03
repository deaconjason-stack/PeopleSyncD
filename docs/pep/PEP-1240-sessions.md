# PEP-1240: Sessions

- Status: Accepted

Sessions are short-lived, revocable, tenant-aware, and protected against fixation and replay. Refresh credentials rotate and are invalidated on logout, password reset, risk events, role removal, or account disablement. Sensitive actions require recent authentication. Desktop storage may retain only encrypted session material and no unencrypted workforce records.
