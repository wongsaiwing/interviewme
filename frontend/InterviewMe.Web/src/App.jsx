import { useEffect, useMemo, useRef, useState } from "react";
import { fetchProfile, streamChat } from "./api.js";

function newSessionId() {
  if (crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return `s-${Date.now()}`;
}

const t = {
  intro: "# Have a seat. Background, work, tech — just ask.",
  placeholder: "Ask me anything here",
  send: "enter",
  you: ">",
  me: "silas",
  footer: "# Replies are spoken as Silas. This chat is not saved.",
  errorProfile: "error: could not load the page",
  errorChat: "error: chat stream failed",
  suggestions: [
    { label: "what did you do at HAECO?", query: "What did you do at HAECO?" },
    { label: "what did you do at TradeLink?", query: "What did you do at TradeLink?" },
    { label: "what is your tech stack?", query: "What is your tech stack?" },
    { label: "what did you study at Glasgow?", query: "What did you study at the University of Glasgow?" }
  ]
};

const PIPE = [
  { id: "input", label: "INPUT" },
  { id: "retrieve", label: "RAG" },
  { id: "generate", label: "LLM" }
];


function splitSentences(text) {
  const parts = text.match(/[^.!?。！？]+[.!?。！？]+(?:["')\]]+)?\s*|[^.!?。！？]+$/g);
  return parts && parts.length ? parts : [text];
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function typeReply(full, setMessages) {
  const text = full || "";
  if (!text) {
    return;
  }
  const duration = Math.min(2000, Math.max(700, text.length * 10));
  const totalChars = Math.max(text.length, 1);
  const slice = duration / totalChars;
  let shown = "";
  for (const sentence of splitSentences(text)) {
    for (const ch of sentence) {
      shown += ch;
      const snapshot = shown;
      setMessages((prev) => {
        const copy = [...prev];
        const last = copy[copy.length - 1];
        if (last?.role === "assistant") {
          copy[copy.length - 1] = { ...last, content: snapshot, streaming: true };
        }
        return copy;
      });
      await sleep(slice);
    }
    await sleep(Math.min(120, duration * 0.06));
  }
}

function Pipeline({ stage }) {
  return (
    <div className="pipeline" aria-label={`flow ${stage}`}>
      {PIPE.map((node, i) => (
        <span key={node.id} className="pipe-item">
          {i > 0 ? <span className="pipe-link" aria-hidden="true" /> : null}
          <span className={`pipe-node${stage === node.id ? " is-lit" : ""}`}>
            <span className="pipe-box" />
            <span className="pipe-label">{node.label}</span>
          </span>
        </span>
      ))}
    </div>
  );
}

export default function App() {
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState("");
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const [stage, setStage] = useState("input");
  const [messages, setMessages] = useState([]);
  const sessionId = useMemo(newSessionId, []);
  const scroller = useRef(null);

  useEffect(() => {
    fetchProfile()
      .then(setProfile)
      .catch(() => setError(t.errorProfile));
  }, []);

  useEffect(() => {
    scroller.current?.scrollTo({ top: scroller.current.scrollHeight, behavior: "smooth" });
  }, [messages]);

  async function send(text) {
    const message = (text ?? input).trim().slice(0, 400);
    if (!message || busy) {
      return;
    }
    setInput("");
    setBusy(true);
    setError("");
    const userMsg = { role: "user", content: message };
    const assistant = { role: "assistant", content: "", sources: [], streaming: true };
    setMessages((prev) => [...prev, userMsg, assistant]);

    let pending = "";
    let failed = "";
    const RAG_HOLD = 400 * 5;
    let ragHold = Promise.resolve();
    try {
      await streamChat({
        message,
        sessionId,
        onEvent: (evt) => {
          if (evt.type === "status") {
            if (evt.text === "retrieve") {
              setStage("retrieve");
              ragHold = sleep(RAG_HOLD).then(() => setStage("generate"));
            }
            return;
          }
          if (evt.type === "token") {
            pending += evt.text || "";
            return;
          }
          if (evt.type === "error") {
            failed = evt.error || t.errorChat;
          }
        }
      });
      await ragHold;
      if (failed) {
        setMessages((prev) => {
          const copy = [...prev];
          const last = copy[copy.length - 1];
          if (last?.role === "assistant") {
            copy[copy.length - 1] = { ...last, content: failed, streaming: false };
          }
          return copy;
        });
      } else {
        await typeReply(pending, setMessages);
        setMessages((prev) => {
          const copy = [...prev];
          const last = copy[copy.length - 1];
          if (last?.role === "assistant") {
            copy[copy.length - 1] = { ...last, content: pending, streaming: false };
          }
          return copy;
        });
      }
    } catch {
      setError(t.errorChat);
      setMessages((prev) => {
        const copy = [...prev];
        const last = copy[copy.length - 1];
        if (last?.role === "assistant" && last.streaming) {
          copy[copy.length - 1] = { ...last, streaming: false, content: last.content || t.errorChat };
        }
        return copy;
      });
    } finally {
      setBusy(false);
      setStage("input");
    }
  }

  const name = (profile?.name || "Silas Wong").toUpperCase();

  return (
    <div className="page">
      <div className="scanlines" aria-hidden="true" />
      <div className="term">
        <div className="term-title">silas@hongkong:~$ interview</div>

        <header className="cv-header">
          <h1># {name}</h1>
          <p className="contact"># silas.wong.saiwing@gmail.com | +852 6509 1653 | Hong Kong</p>
        </header>

        <Pipeline stage={stage} />

        <main className="chat">
          <p className="chat-intro">{t.intro}</p>
          <div className="suggestions">
            {t.suggestions.map((q) => (
              <button key={q.query} type="button" disabled={busy} onClick={() => send(q.query)}>
                $ {q.label}
              </button>
            ))}
          </div>

          <div className="transcript" ref={scroller}>
            {messages.map((m, i) => (
              <article key={i} className={`line ${m.role}`}>
                <span className="prompt">{m.role === "user" ? ">" : "silas>"}</span>
                <span className="content">
                  {m.content}
                  {m.streaming ? <span className="caret" /> : null}
                </span>
              </article>
            ))}
          </div>

          {error ? <div className="error"># {error}</div> : null}

          <form
            className="composer"
            onSubmit={(e) => {
              e.preventDefault();
              send();
            }}
          >
            <span className="prompt">&gt;</span>
            <label className="input-wrap">
              {input.length === 0 ? <span className="caret composer-caret" aria-hidden="true" /> : null}
              <input
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder={t.placeholder}
                disabled={busy}
                maxLength={400}
                autoComplete="off"
                spellCheck="false"
                autoFocus
                className={input.length === 0 ? "is-empty" : undefined}
              />
            </label>
            <button type="submit" disabled={busy || !input.trim()}>
              [{t.send}]
            </button>
          </form>
        </main>

        <footer className="foot">{t.footer}</footer>
      </div>
    </div>
  );
}
