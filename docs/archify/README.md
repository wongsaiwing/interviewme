# Archify maps

JSON IR is the source of truth. HTML is generated and must stay in this folder so the GitHub repo always has openable maps.

| File | Type |
|---|---|
| [interviewme.architecture.html](./interviewme.architecture.html) | Runtime architecture |
| [interviewme.sequence.html](./interviewme.sequence.html) | One chat sequence (includes off-topic short-circuit) |
| [interviewme.dataflow.html](./interviewme.dataflow.html) | RAG / knowledge data-flow |

## Keep HTML current

`.github/workflows/archify.yml` runs on every push to `main`:

1. Download the Archify CLI (`archify.zip`).
2. `deliver` each JSON IR into its HTML (showcase quality).
3. Commit HTML if it changed (`[archify skip]` so the workflow does not loop).

Pull requests render and validate but do not commit.

## When the system changes

Edit the matching `.json` (nodes, edges, stories) to match the code, then push. The workflow rebuilds HTML. Do not hand-edit HTML.

Do not add invented infrastructure (Redis, Postgres, auth). InterviewMe is a same-origin ASP.NET BFF + in-memory hashing RAG + DeepSeek.
