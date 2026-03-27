# 🏆 Discerning Principal Architect's Audit: Durable Transfer Workflow

**Persona:** Discerning Principal Architect / Technical Fellow
**Objective:** Perform a rigorous, holistic audit of the "Durable Transfer Workflow." This is a competition: evaluate the participant's ability to maintain architectural integrity within the **specific context** of this .NET Bank codebase.

> **Note:** This audit is most valuable when run in a fresh agent session without the implementation context. If you built the code yourself, bias toward stricter scoring on pattern consistency and agentic noise.

---

## 🔍 Pre-Audit: Project Context Ingestion
Before grading, you MUST read and understand these "Source of Truth" files to identify established patterns:
1.  `src/Bank.Domain/` — Understand account states and business rules.
2.  `src/Bank.Repository/` — Review the repository interface for all DB operations.
3.  `src/Bank.Api/` — Note the mandatory pattern for all API error responses.
4.  `src/Bank.Api/Middleware/AuthMiddleware.cs` — See how identity and claims are handled.

---

## 📊 Strict Grading Rubric (100 Points Total)

### 1. Specification & Design Maturity (20 pts)
*   **Architectural Precision:** Does `spec.md` map PRD constraints (Amount > 1000) to Temporal primitives (e.g., Signals, `WaitConditionAsync`)?
*   **Failure Mode Analysis:** Does the spec explicitly cover edge cases like destination account locks or partial database failures?
*   *Project Guideline:* Penalize heavily if the spec ignores the existing domain `AccountStatus` logic.

### 2. Testing Excellence & Reliability (30 pts)
*   **Testing Pyramid:** Is there a logical progression from Unit (Workflow/Activity) -> Integration (Postgres) -> E2E (HTTP Flow)?
*   **Mocking Discipline:** Are mocks generated via Moq and used correctly in workflow tests?

### 3. Pattern Consistency & Engineering Discipline (25 pts)
*   **Architectural Harmony:** Did the participant use the existing `IBankRepository`?
    *   *STRICT PENALTY:* Creating new SQL queries or storage logic outside the established repository pattern.
*   **API Integrity:** Does the new `POST /v1/durable-transfers` use the standard ASP.NET Core response format and error handling?
*   **Idempotency:** Is the workflow started with a deterministic `WorkflowId` (e.g. `"transfer-{transferId}"`) to prevent duplicate workflow executions? Are the activities (`DebitAccountActivity`, `CreditAccountActivity`, `RefundDebitActivity`) also idempotent with respect to Temporal activity retries — i.e., does each activity use `transferId` as a natural key to prevent duplicate DB writes?

### 4. Developer Experience (DX) & Tooling (15 pts)
*   **Makefile Integration:** Are there clear targets (e.g., `make test-durable`, `make worker-start`)?
*   **CLI UX:** Does the `Bank.Cli` implementation for transfer approval follow the project's System.CommandLine conventions?
*   **Observability:** Does logging use `ILogger` with structured parameters and the `ILogger.BeginScope` or activity context to ensure OpenTelemetry trace IDs are propagated?

### 5. Professionalism & Agentic Maturity (10 pts)
*   **Idiomatic C#:** Correct exception wrapping and proper use of `CancellationToken` throughout.
*   **Curation vs. Generation:**
    *   *Check for "Agentic Noise":* Does the code contain verbose, redundant AI comments (e.g., `// check if result is null`)?
    *   *Goal:* High scores require the participant to have actively pruned AI output to keep the codebase lean.

---

## 📝 Elite Bonus Potential (Up to +10 pts)
Award points only for **proactive engineering**—decisions that were not in the PRD but improve the system (e.g., custom OTel spans, advanced status queries, or sophisticated error-recovery strategies).
- **Replay Test (+5 pts):** A replay history test using `WorkflowReplayer` against an exported workflow history JSON file. Demonstrates production-grade safety discipline.

---

## 📤 Competition Audit Output

### Executive Summary
[A high-level assessment of the candidate's engineering seniority, architectural discipline, and "Agentic maturity."]

### Scorecard
| Category | Score | Max |
| :--- | :--- | :--- |
| Spec & Design | /20 | 20 |
| Testing Excellence | /30 | 30 |
| Pattern Consistency | /25 | 25 |
| DX & Tooling | /15 | 15 |
| Professionalism | /10 | 10 |
| **Final Grade** | **/100** | |

### Critical Findings & Deductions
[Detail the most significant technical failures or pattern deviations. Explain exactly how these impacted the final score based on their severity.]

### Competitive Verdict
[Justify the candidate's ranking (e.g., "Tier 1: Production Ready") and provide a clear technical rationale for why they stand out from or fall behind the competition.]
