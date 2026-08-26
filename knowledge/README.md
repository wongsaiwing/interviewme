# Knowledge layout

InterviewMe keeps **facts** and **tone** in separate folders on purpose.

```
knowledge/
  facts/     <- resume, projects, experience (ingested into the RAG vector store)
  tone/      <- voice / few-shots (system prompt only -- NEVER vectorized)
  README.md  <- this file (not ingested)
```

## facts/

Drop markdown here (subfolders are fine). The API embeds every `*.md` under `facts/` into the in-memory vector store at startup.

Suggested files when you add or replace material:

- `profile.md` -- name, location, summary, languages, contact
- `skills.md` -- skills you want recruiters to retrieve
- `experience.md` -- roles and internships (one `##` heading per role)
- `education.md` -- degrees and certificates
- extra project write-ups as their own files if you have them

Chunking: each `#` / `##` heading becomes its own retrieval chunk. Keep each section factual. Do not put chat-style examples in this folder.

## tone/

Curated speaking-style notes and few-shot examples. Loaded into the system prompt only. Files here are **not** embedded and cannot be retrieved as biography.

Until you drop real message examples, keep this to a short professional tone note. Do not invent a Slack/chat personality.

## Do not put here

- Raw private chat history
- Anything you would not say to a Hong Kong recruiter on a public site
