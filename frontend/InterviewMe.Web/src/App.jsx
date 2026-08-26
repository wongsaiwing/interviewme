import { useEffect, useMemo, useRef, useState } from "react";
import { fetchProfile, streamChat } from "./api.js";

function newSessionId() {
  if (crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return `s-${Date.now()}`;
}

const t = {
  intro: "坐低先。背景、工作、技術，直接問就得。",
  placeholder: "問我背景、HAECO、技術棧…",
  send: "送出",
  you: "你",
  empty: "當面試咁問就得。揀一條建議，或者自己打。",
  footer: "回覆由 AI 根據履歷整理，對話不會存檔。",
  errorProfile: "無法載入資料。",
  errorChat: "對話串流失敗。",
  suggestions: [
    { label: "你在 HAECO 做什麼？", query: "What did you do at HAECO?" },
    { label: "TradeLink 的經驗？", query: "What did you do at TradeLink?" },
    { label: "你的技術棧？", query: "What is your tech stack?" },
    { label: "格拉斯哥大學讀什麼？", query: "What did you study at the University of Glasgow?" }
  ]
};

export default function App() {
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState("");
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
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

    try {
      await streamChat({
        message,
        sessionId,
        onEvent: (evt) => {
          setMessages((prev) => {
            const next = [...prev];
            const last = { ...next[next.length - 1] };
            if (evt.type === "token") {
              last.content = (last.content || "") + (evt.text || "");
            } else if (evt.type === "error") {
              last.content = evt.error || t.errorChat;
              last.streaming = false;
            } else if (evt.type === "done") {
              last.streaming = false;
            }
            next[next.length - 1] = last;
            return next;
          });
        }
      });
    } catch {
      setError(t.errorChat);
      setMessages((prev) => {
        const next = [...prev];
        const last = next[next.length - 1];
        if (last?.role === "assistant" && last.streaming) {
          next[next.length - 1] = { ...last, streaming: false, content: last.content || t.errorChat };
        }
        return next;
      });
    } finally {
      setBusy(false);
    }
  }

  const name = (profile?.name || "Silas Wong").toUpperCase();

  return (
    <div className="page">
      <header className="cv-header">
        <h1>{name}</h1>
        <p className="contact">silas.wong.saiwing@gmail.com | +852 6509 1653 | Hong Kong</p>
      </header>

      <main className="chat">
        <p className="chat-intro">{t.intro}</p>
        <div className="suggestions">
          {t.suggestions.map((q) => (
            <button key={q.query} type="button" disabled={busy} onClick={() => send(q.query)}>
              {q.label}
            </button>
          ))}
        </div>

        <div className="transcript" ref={scroller}>
          {messages.length === 0 && <div className="empty">{t.empty}</div>}
          {messages.map((m, i) => (
            <article key={i} className={`bubble ${m.role}`}>
              <div className="who">{m.role === "user" ? t.you : profile?.name || "Silas Wong"}</div>
              <div className="content">
                {m.content}
                {m.streaming ? <span className="caret" /> : null}
              </div>
            </article>
          ))}
        </div>

        {error ? <div className="error">{error}</div> : null}

        <form
          className="composer"
          onSubmit={(e) => {
            e.preventDefault();
            send();
          }}
        >
          <input
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder={t.placeholder}
            disabled={busy}
            maxLength={400}
          />
          <button type="submit" disabled={busy || !input.trim()}>
            {t.send}
          </button>
        </form>
      </main>

      <footer className="foot">{t.footer}</footer>
    </div>
  );
}
