const BASE = '/api/v1';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(BASE + path, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options?.headers },
  });
  if (!res.ok) throw new Error(await res.text().catch(() => res.statusText));
  if (res.status === 204) return undefined as T;
  return res.json();
}

/** Universal API response (Result pattern). Check isSuccess, then use data or error. */
export interface ApiResult<T> {
  isSuccess: boolean;
  data?: T;
  error?: string;
}

export const api = {
  agents: {
    list: (tenantId?: string) => request<Agent[]>(`/agents?tenantId=${tenantId ?? ''}`),
    get: (id: string, tenantId?: string) => request<Agent>(`/agents/${id}?tenantId=${tenantId ?? ''}`),
    create: (body: CreateAgentRequest) => request<Agent>('/agents', { method: 'POST', body: JSON.stringify(body) }),
    update: (id: string, body: UpdateAgentRequest) => request<Agent>(`/agents/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
    delete: (id: string, tenantId?: string) => request<void>(`/agents/${id}?tenantId=${tenantId ?? ''}`, { method: 'DELETE' }),
    run: (id: string, body: RunAgentRequest, tenantId?: string) =>
      request<ApiResult<AgentResponse>>(`/agents/${id}/run?tenantId=${tenantId ?? ''}`, { method: 'POST', body: JSON.stringify(body) }),
  },
  tools: {
    list: (tenantId?: string) => request<Tool[]>(`/tools?tenantId=${tenantId ?? ''}`),
    get: (id: string, tenantId?: string) => request<Tool>(`/tools/${id}?tenantId=${tenantId ?? ''}`),
    register: (body: RegisterToolRequest) => request<Tool>('/tools', { method: 'POST', body: JSON.stringify(body) }),
  },
  audit: {
    query: (params: AuditQueryParams) => {
      const q = new URLSearchParams();
      if (params.tenantId) q.set('tenantId', params.tenantId);
      if (params.agentId) q.set('agentId', params.agentId);
      if (params.eventType) q.set('eventType', params.eventType);
      if (params.from) q.set('from', params.from);
      if (params.to) q.set('to', params.to);
      q.set('skip', String(params.skip ?? 0));
      q.set('take', String(params.take ?? 50));
      return request<AuditEvent[]>(`/audit?${q}`);
    },
  },
  policies: {
    list: (tenantId?: string) => request<Policy[]>(`/policies?tenantId=${tenantId ?? ''}`),
    get: (id: string, tenantId?: string) => request<Policy>(`/policies/${id}?tenantId=${tenantId ?? ''}`),
    save: (body: SavePolicyRequest) => request<Policy>('/policies', { method: 'POST', body: JSON.stringify(body) }),
  },
  rag: {
    createCollection: (collectionId: string) => request<void>(`/rag/collections/${collectionId}`, { method: 'POST' }),
    deleteCollection: (collectionId: string) => request<void>(`/rag/collections/${collectionId}`, { method: 'DELETE' }),
    index: (collectionId: string, body: IndexDocumentRequest) =>
      request<void>(`/rag/collections/${collectionId}/documents`, { method: 'POST', body: JSON.stringify(body) }),
    search: (collectionId: string, q: string, topK = 5) =>
      request<RagSearchResult[]>(`/rag/collections/${collectionId}/search?q=${encodeURIComponent(q)}&topK=${topK}`),
  },
  controlPlane: {
    license: () => request<LicenseStatus>('/control-plane/license'),
    config: (key?: string) => request<{ key?: string; value?: string }>(`/control-plane/config${key ? `?key=${key}` : ''}`),
  },
};

export interface Agent {
  id: string;
  name: string;
  description: string;
  systemPrompt: string;
  toolIds: string[];
  ragCollectionId?: string;
  policyId?: string;
  llmProvider?: string;
  llmModel?: string;
  isEnabled: boolean;
  createdAt: string;
  updatedAt: string;
  tenantId?: string;
}

export interface CreateAgentRequest {
  name: string;
  description?: string;
  systemPrompt?: string;
  toolIds?: string[];
  ragCollectionId?: string;
  policyId?: string;
  llmProvider?: string;
  llmModel?: string;
  isEnabled?: boolean;
}

export interface UpdateAgentRequest extends CreateAgentRequest {}

export interface RunAgentRequest {
  message: string;
  userId?: string;
  context?: Record<string, string>;
}

export interface AgentResponse {
  message: string;
  toolCalls: { toolId: string; input: string; output?: string; success: boolean }[];
  completed: boolean;
  error?: string;
}

export interface Tool {
  id: string;
  name: string;
  description: string;
  inputSchema?: string;
  isEnabled: boolean;
}

export interface RegisterToolRequest {
  name: string;
  description?: string;
  inputSchema?: string;
  id?: string;
  isEnabled?: boolean;
}

export interface AuditEvent {
  id: string;
  eventType: string;
  agentId?: string;
  tenantId?: string;
  userId?: string;
  correlationId?: string;
  toolName?: string;
  payload?: string;
  result?: string;
  timestamp: string;
}

export interface AuditQueryParams {
  tenantId?: string;
  agentId?: string;
  eventType?: string;
  from?: string;
  to?: string;
  skip?: number;
  take?: number;
}

export interface Policy {
  id: string;
  name: string;
  description?: string;
  rules: { action: string; resource?: string; toolPattern?: string; conditions?: Record<string, string> }[];
  tenantId?: string;
}

export interface SavePolicyRequest {
  id?: string;
  name: string;
  description?: string;
  rules?: { action?: string; resource?: string; toolPattern?: string; conditions?: Record<string, string> }[];
}

export interface IndexDocumentRequest {
  id: string;
  content: string;
  metadata?: Record<string, string>;
}

export interface RagSearchResult {
  documentId: string;
  content: string;
  score: number;
  metadata?: Record<string, string>;
}

export interface LicenseStatus {
  valid: boolean;
  edition?: string;
  expiresAt?: string;
  maxAgents?: number;
  maxTenants?: number;
  features: string[];
}
