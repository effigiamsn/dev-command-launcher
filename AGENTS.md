# Global Agent Rules
This file contains global agent behavior rules shared across repositories.

Project-specific architecture, workflows, and implementation constraints should be documented in:

- `docs/RULES.md`
- `docs/modules/*.md`
- project-specific regression and TODO documents

Project rules extend and specialize these global rules for the current repository.

## Core Philosophy

- Prefer factual analysis over assumptions.
- Do not guess file locations, APIs, architecture, or root causes without inspection.
- Separate confirmed facts from hypotheses.
- Prefer small, reversible modifications over broad refactors.

---

# Reasoning Model Escalation Rule

When a task has high architectural, security, financial, data-integrity, compliance, or hard-to-reverse implementation risk, proactively recommend using a higher reasoning model before implementation.

Recommend a higher reasoning model especially for:

- architecture decisions with long-term consequences
- database schema, migration, or data provenance decisions
- credential, auth, security, privacy, or broker boundary decisions
- financial trading, order execution, automation, or risk guard design
- market data source, licensing, compliance, or redistribution decisions
- large PR reviews, stacked merge strategy, or complex refactors
- debugging where the root cause remains unclear after initial inspection

Do not recommend escalation for routine documentation, small code edits, simple Linear/GitHub sync, formatting, or low-risk mechanical changes.

If a task starts simple but becomes high-risk during execution, pause and recommend escalation before continuing.

## Next-Step Reasoning Recommendation

When proposing next steps, include a recommended reasoning model or reasoning level for each meaningful option.

Use this as lightweight guidance, not ceremony:

- `low`: simple lookup, formatting, small documentation cleanup, or low-risk mechanical edits.
- `medium`: ordinary feature work, focused debugging with a clear scope, routine documentation updates, or small implementation planning.
- `high`: architecture choices, database/schema decisions, security/auth/privacy work, agent command execution design, external integrations, migration planning, or unclear debugging where the root cause is not yet established.
- `xhigh` or higher: hard-to-reverse implementation, multi-system refactors, financial/order-execution logic, compliance-sensitive work, or scaffold/build steps that commit the project to a long-term architecture.

Do not over-recommend higher reasoning for routine tasks. If the next step is low-risk, explicitly say that `medium` or lower is enough.

---

# Linear Human Handoff Rule

When working in any project where Linear access is available or the work is connected to a Linear project/issue, handle human-only blockers through Linear.

If a task requires human action that Codex cannot perform directly, create or update a Linear issue. Examples include installing software, granting account access, approving payments, creating credentials, changing external service settings, making a business/legal decision, or performing physical/offline work.

The Linear issue must include:

- label: `needs-review`
- priority
- explicit status, usually `Todo` for human action
- assignee when the responsible human is known, usually `me` when the user is the responsible reviewer
- the required human action
- why it is needed
- completion criteria
- the next step after the human action is completed

Priority rules:

- If the current work cannot continue until the human action is completed, set the issue to the highest available Linear priority, usually `Urgent`.
- If work can continue but the human action is still needed soon, set priority based on impact and timing.
- Do not leave the issue without a priority.

Visibility and notification rules:

- Do not rely on Linear's default issue status for `needs-review` issues.
- Do not place new human-action `needs-review` issues in `Backlog` unless the user explicitly asks for backlog triage.
- Use `Todo` by default so the issue appears as an actionable item.
- If the team's workflow does not have `Todo`, use the nearest unstarted actionable status and report the chosen status.
- Make `needs-review` issues visible to the responsible human by assigning the issue to them when known, usually `me` for the user.
- Mention the responsible human in the issue description or a comment so the update is visible in their Linear Inbox. Use the user's Linear display name when known.
- When creating a `needs-review` issue, include the mention in the description when possible.
- When updating an existing `needs-review` issue with new required human action, add a comment with the mention instead of silently changing the issue.
- Do not rely on Slack, email, desktop, or mobile notifications unless the user has confirmed those Linear notification channels are enabled.

Grouping rules:

- Create one `needs-review` issue when multiple human actions are part of the same immediate unblock path for one task, can be completed by the same responsible person, and have the same priority/timing. Put the actions in a checklist in the issue body.
- Split into separate `needs-review` issues when actions can be completed independently, have different responsible people, different priorities, different deadlines, different projects/teams, or one action can unblock useful work before the others are done.
- For setup sequences such as "install A, B, C, D, E and then perform F", create one issue if all steps are required together before Codex can continue. Use a checklist and make the final step explicit.
- If any single step in a grouped issue becomes separately blocking or needs separate ownership, create a follow-up issue and link/reference the original issue.
- Do not create many small `needs-review` issues for one tightly coupled setup unless separate tracking would reduce real risk.

Completion signal rules:

- The preferred completion signal is for the user to move the Linear issue to `Done` or comment that the required human action is complete.
- If the user reports completion in chat, verify the relevant Linear issue when possible, then update it to reflect the completion state or add a comment.
- When resuming blocked work, check the linked `needs-review` issue status/comments before assuming the human action is still incomplete.
- If the issue is `Done` or has a clear completion comment, continue the blocked work and mention that the Linear handoff was completed.
- If completion is ambiguous, ask one concise clarification before proceeding with risky or irreversible work.

After creating or updating the issue:

- Continue with any work that is still unblocked.
- If progress is blocked, report the Linear issue key/link and clearly state that the task is blocked on human action.
- If Linear tools are unavailable, or the correct Linear team/project cannot be determined, provide a ready-to-create Linear issue draft with the same fields and ask for the missing Linear destination only if it blocks issue creation.

---

# Documentation-Driven Development

This workspace follows a documentation-first workflow.

Before implementing or modifying code:

1. Check project-level rules and maps:
   - `docs/RULES.md`
   - `docs/README.md`
   - `docs/PROJECT_MAP.md`
2. Check relevant TODO and regression documents.
3. Read related module documents under:
   - `docs/modules/*.md`

If documentation does not exist for a non-trivial system:

- propose creating it first.

After major changes:

- update the related documentation immediately.

Documentation is considered part of the implementation, not optional.

---

# Common Engineering Knowledge System

Some reusable engineering, design, UI, infra, data, security, or workflow rules should be preserved outside a single project when they are likely to apply across multiple repositories or workspaces.

When a workspace-level common knowledge folder exists, such as:

- `docs/common/`
- `<workspace-root>/docs/common/`
- another clearly documented shared knowledge folder

use it as the shared reference location for reusable rules.

Before implementing or reviewing React UI work, check for a shared React UI rules document when available, especially:

- `docs/common/react_ui_rules.md`
- `<workspace-root>/docs/common/react_ui_rules.md`

If the current workspace is `E:\Business`, the shared React UI rules document is:

- `E:\Business\docs\common\react_ui_rules.md`

Use shared common documents for recurring issues such as:

- React layout shift
- sidebar collapse text jump
- stable dimensions
- text overflow
- animation-safe UI layout
- Tauri window/monitor behavior
- TypeScript state/cache patterns
- API contract compatibility
- database migration safety
- security and credential boundaries
- CI/build/release workflow rules

When a new recurring issue or reusable implementation rule appears:

1. Decide whether it is project-specific or reusable across projects.
2. If reusable, propose adding it to `docs/common/<topic>_rules.md` or `docs/common/<topic>_issues.md`.
3. If no suitable common document exists, propose creating one.
4. Keep project-specific exceptions, product decisions, and module contracts inside the project docs.
5. Link the common document from relevant project docs so future work can find it.
6. Do not leave important reusable knowledge only in chat history.

Examples:

- `docs/common/react_ui_rules.md`
- `docs/common/tauri_window_rules.md`
- `docs/common/typescript_state_rules.md`
- `docs/common/api_contract_rules.md`
- `docs/common/postgres_migration_rules.md`
- `docs/common/security_boundary_rules.md`

---

# Project Memory System

The repository uses a persistent "project memory" structure.

## PROJECT_MAP.md

`docs/PROJECT_MAP.md` maps human-friendly module names to real files and directories.

Example:

- Browser module -> `src/renderer/pages/browser`
- DAG system -> `app/services/dag`
- Translation pipeline -> `app/services/translation`

When the user references a feature or module name:

- consult `docs/PROJECT_MAP.md` before asking for paths.

If file locations changed:

- update `docs/PROJECT_MAP.md`.

---

# Module Documentation Rules

Each major module should have a dedicated document:

`docs/modules/<module>.md`

Example:

- `docs/modules/browser.md`
- `docs/modules/dag_system.md`
- `docs/modules/translation_pipeline.md`

These documents are critical context sources.

Before modifying a module:

1. Read its module document.
2. Read related regression notes.
3. Preserve existing architectural constraints unless explicitly changing them.

After modifying a module:

- update its module document.

---

# Module Document Structure

Preferred structure:

```md
# Module Name

## Purpose
## Related Files
## Public APIs
## Internal Flow
## State/Data Flow
## Important Constraints
## Known Problems
## Regression Notes
## Rejected Approaches
## TODO
```

Document:

- architectural decisions
- implementation intent
- rejected approaches
- fragile areas
- historical bugs
- important warnings

Do not use module docs as marketing text.
Use them as engineering memory.

---

# Regression Tracking

Recurring bugs and fragile systems must be tracked in:

`docs/regression.md`

Before modifying fragile code:

- check regression history first.

After fixing important bugs:

- add regression notes.

Regression entries should include:

- cause
- symptoms
- affected modules
- prevention notes

Large projects may split regression tracking into:

- `docs/regressions/*.md`

Keep regression documents scoped and searchable.

---

# TODO Tracking

Use `docs/TODO.md` for:

- pending work
- deferred ideas
- unfinished refactors
- follow-up tasks

Do not rely only on chat history for pending tasks.

Large projects may split TODO tracking into:

- `docs/todo/*.md`

Avoid oversized catch-all TODO documents.

---

# Documentation Language Rules

Documentation should prioritize long-term maintainability and fast human understanding.

Preferred style:

- Write explanations primarily in Korean.
- Preserve technical keywords in English.
- Keep API names, function names, file names, and searchable technical terms in English.
- Use English for terms commonly searched in IDEs, GitHub, or web searches.

Examples:

- race condition
- stale cache
- state desync
- preload IPC boundary

Recommended:

- Korean for architectural intent, warnings, and historical context.
- English for technical identifiers and search-critical concepts.

Avoid:

- translating technical identifiers unnaturally
- replacing common engineering terms with obscure Korean equivalents

---

# Planning Rules

Before major modifications:

1. Explain the implementation plan.
2. Identify likely affected files/modules.
3. Explain architectural impact.
4. Mention possible risks or regressions.

Avoid large silent changes.

---

## Encoding / Korean Text Rule

- Do not mention encoding issues when Korean text in docs or comments is displayed correctly.
- If Korean text appears garbled in PowerShell or terminal output, first re-read the file with an explicit UTF-8 encoding, such as `Get-Content -Encoding UTF8`.
- Only report an encoding problem when the file content is still mojibake after UTF-8 verification.
- Do not assume the file itself is corrupted just because terminal output rendered Korean incorrectly.

---

# Debugging Rules

When debugging:

- explicitly identify inspected files/functions
- distinguish verified causes from guesses
- avoid claiming a fix is complete without verification

Never say:

- "definitely fixed"
- "root cause confirmed"

unless directly verified.

---

# Change Safety Rules

Do not:

- silently add dependencies
- rename large structures without approval
- change build systems casually
- remove logs or comments without reason
- perform broad rewrites during isolated bug fixes

Preserve existing behavior unless intentionally changing it.

---

# Knowledge Preservation

Important implementation knowledge should not exist only in chat history.

If important architectural knowledge appears during work:

- store it in docs/modules or regression.md.

The repository itself should retain long-term engineering memory.
