# InterviewMe

Public site where Hong Kong recruiters chat with an AI of Silas Wong, grounded in his resume.

Happy path needs no paid accounts. LangChain RAG (MarkdownHeaderTextSplitter + in-memory vector collection) with a local hashing embedder, grounded stub LLM. Chat completions use an OpenAI-compatible adapter (DeepSeek). Embedding calls never go to OpenAI.

## Run locally

Install .NET 8 SDK and Node 18+. Build the SPA in frontend/InterviewMe.Web, then start the API in src/InterviewMe.Api on port 5080 bound to all interfaces. Visit the local host on that port. Run the solution tests with the dotnet test command on InterviewMe.sln.

## Docker

Use the compose file in this folder. The API image bakes in the SPA. Bind 0.0.0.0:8080. Knowledge is copied to /app/knowledge. Facts under knowledge/facts are split, embedded, and searched with LangChain. Tone stays prompt-only. No Postgres required.

## Internet

The live host is HTTP on a public IP (and later swwdomain.hk). AllowedHosts is open so the IP and future hostnames both work. HTTPS is not required yet. CORS lists the IP and future domain origins; production is same-origin because the API serves wwwroot.

## Knowledge

See knowledge/README.md. Facts under knowledge/facts are embedded. Tone under knowledge/tone is prompt-only and never vectorized. Empty retrieval must not invent biography.

## Policy

Frontend never talks to an LLM. Tone stays in the system prompt. Visitor transcripts are not persisted.

Sample questions: HAECO, TradeLink, tech stack, University of Glasgow.

## Architecture maps

Open these Archify HTML files in a browser (self-contained):

- [Runtime architecture](docs/archify/interviewme.architecture.html)
- [Chat sequence](docs/archify/interviewme.sequence.html)
- [RAG data-flow](docs/archify/interviewme.dataflow.html)

JSON IR lives next to the HTML. GitHub Action `.github/workflows/archify.yml` re-renders HTML on every push to `main`.
