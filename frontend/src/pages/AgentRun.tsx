import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { api, type Agent, type AgentResponse } from '../api/client';
import { useNotifications } from '../context/NotificationContext';

/** Extract content from ```markdown ... ``` blocks in the message to render as Markdown instead of code. */
function unwrapMarkdownCodeBlocks(text: string): string {
  return text.replace(/```(?:markdown|md)?\s*\n?([\s\S]*?)```/g, (_, inner) => inner.trim());
}

export default function AgentRun() {
  const { t } = useTranslation();
  const { addToast } = useNotifications();
  const { id } = useParams<{ id: string }>();
  const [agent, setAgent] = useState<Agent | null>(null);
  const [message, setMessage] = useState('');
  const [response, setResponse] = useState<AgentResponse | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (id) api.agents.get(id).then(setAgent).catch(() => setAgent(null));
  }, [id]);

  const send = async () => {
    if (!id || !message.trim()) return;
    setLoading(true);
    setResponse(null);
    try {
      const result = await api.agents.run(id, { message: message.trim() });
      if (result.isSuccess && result.data) {
        setResponse(result.data);
        addToast('success', t('agentRun.responseReceived', { defaultValue: 'Ответ получен' }));
      } else {
        const errMsg = result.error ?? 'Unknown error';
        setResponse({
          message: '',
          toolCalls: [],
          completed: false,
          error: errMsg,
        });
        addToast('error', errMsg, { title: t('agentRun.errorTitle', { defaultValue: 'Ошибка агента' }) });
      }
    } catch (err) {
      const errMsg = (err as Error).message;
      setResponse({ message: '', toolCalls: [], completed: false, error: errMsg });
      addToast('error', errMsg, { title: t('agentRun.errorTitle', { defaultValue: 'Ошибка агента' }) });
    } finally {
      setLoading(false);
    }
  };

  if (!agent) {
    return (
      <div className="page">
        <div className="page-header"><h1 className="page-title">{t('common.loading')}</h1></div>
      </div>
    );
  }

  return (
    <div className="page agent-run-page">
      <header className="page-header">
        <h1 className="page-title">{agent.name}</h1>
      </header>
      <div className="page-body">
        <div className="agent-run-card">
          <h3 className="agent-run-chat-title">{t('agentRun.chat')}</h3>
          <div className="agent-run-input-row">
            <div className="form-group">
              <label className="visually-hidden" htmlFor="agent-message">{t('agentRun.messagePlaceholder')}</label>
              <textarea
                id="agent-message"
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                placeholder={t('agentRun.messagePlaceholder')}
                onKeyDown={(e) => e.key === 'Enter' && !e.shiftKey && (e.preventDefault(), send())}
              />
            </div>
            <button type="button" className="btn btn-primary" onClick={send} disabled={loading}>
              {loading ? t('agentRun.sending') : t('agentRun.send')}
            </button>
          </div>
          <div className="agent-run-response-wrap">
            {response ? (
              <>
                {response.error && <p className="response-error">{response.error}</p>}
                {response.message && (
                  <div className="response-markdown">
                    <ReactMarkdown remarkPlugins={[remarkGfm]}>{unwrapMarkdownCodeBlocks(String(response.message))}</ReactMarkdown>
                  </div>
                )}
                {response.toolCalls && response.toolCalls.length > 0 && (
                  <details>
                    <summary>{t('agentRun.toolCalls', { count: response.toolCalls.length })}</summary>
                    <ul>
                      {response.toolCalls.map((tc, i) => (
                        <li key={`${tc.toolId}-${i}`}>
                          <strong>{tc.toolId}</strong>: {tc.success ? tc.output : tc.output || t('agentRun.toolError')}
                        </li>
                      ))}
                    </ul>
                  </details>
                )}
              </>
            ) : (
              <p className="agent-run-placeholder">{t('agentRun.placeholder', { defaultValue: 'Отправьте сообщение агенту.' })}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
