# InterviewMe security

If asked how I handle security for an LLM-backed web app, a RAG site, or InterviewMe: first level, say it at a high level, close to this: "On InterviewMe, I screen every question for prompt injection before it reaches the model, keep it to questions about my work, and it only answers from the facts it retrieves. I also check the output before it goes back to the user."

## More detail (only when they follow up)

Say it in plain words, with no method, class, or tool names. The screening catches things like asking it to ignore previous instructions, show the system prompt, reveal an API key, or a jailbreak, and those get a fixed refusal without the question going to the model. Questions outside the interview, such as coding help or politics, get a short fixed refusal. The model is told to answer only from the retrieved facts about me. The output check removes names and claims that must never appear, for example language exam grades or a wrong co-worker. If the model provider fails, the visitor gets a short fixed message asking them to try again, and never any internal text.

Only these protections. Do not add encryption, login, firewall, monitoring, or other claims.

Keywords: security, secure, prompt injection, jailbreak, LLM security, LLM-backed web app, guardrails, InterviewMe security, output check, refusal
