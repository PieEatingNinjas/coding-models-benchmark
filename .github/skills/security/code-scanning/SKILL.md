---
name: code-scanning
description: Checklist and report format for scanning .NET code for vulnerabilities. Used by SecurityAgent.
---

# Code Scanning (.NET)

## Scan checklist (OWASP-oriented)

- **Injection** — parameterized queries / EF Core; never string-concat in SQL. No `Process.Start` with user input.
- **Deserialization** — no insecure `BinaryFormatter`; validate JSON input.
- **AuthN/AuthZ** — `[Authorize]` on endpoints; no authorization-by-obscurity; role/policy checks correct.
- **Secrets** — no hardcoded keys/connection strings; use user-secrets / Key Vault / env.
- **Crypto** — no MD5/SHA1 for security; no fixed IV/salt; use `RandomNumberGenerator`.
- **XSS/CSRF** — output encoding; antiforgery tokens on state-changing forms.
- **SSRF / path traversal** — validate/whitelist URLs and path names.
- **Mass assignment** — use DTOs instead of binding entities to requests.
- **Dependencies** — `dotnet list package --vulnerable --include-transitive`.

## Report — `security-reports/security-report.md`

```markdown
# Security Report — <date>

## Executive Summary
Risk level: <Critical/High/Medium/Low>. <1-2 sentences.>

## Findings
### [CRITICAL] <title>
- File: `src/...:42`
- Description: <what and why>
- Remediation: <concrete fix>

### [HIGH] ...
### [MEDIUM] ...
### [LOW] ...

## Vulnerable dependencies
| Package | Current version | Safe version | Severity |

## False positives
<explicitly named>
```

## Rules
- Fix nothing — report only.
- Every finding: severity + location + concrete remediation.
