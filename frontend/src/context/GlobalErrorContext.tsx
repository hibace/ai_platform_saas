import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import ErrorPage from '../components/ErrorPage';

type GlobalErrorContextValue = {
  error: Error | null;
  setError: (error: Error | null) => void;
};

const GlobalErrorContext = createContext<GlobalErrorContextValue | null>(null);

export function GlobalErrorProvider(props: { readonly children: ReactNode }) {
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
      const err = event.reason instanceof Error ? event.reason : new Error(String(event.reason));
      console.error('Unhandled rejection:', err);
      setError(err);
      event.preventDefault();
    };

    const handleError = (event: ErrorEvent) => {
      const err = event.error instanceof Error ? event.error : new Error(event.message || 'Unknown error');
      console.error('Global error:', err);
      setError(err);
      event.preventDefault();
      return true;
    };

    window.addEventListener('unhandledrejection', handleUnhandledRejection);
    window.addEventListener('error', handleError);
    return () => {
      window.removeEventListener('unhandledrejection', handleUnhandledRejection);
      window.removeEventListener('error', handleError);
    };
  }, []);

  const clearError = useCallback(() => setError(null), []);
  const value = useMemo(() => ({ error, setError }), [error]);

  return (
    <GlobalErrorContext.Provider value={value}>
      {error ? (
        <ErrorPage error={error} onRetry={clearError} fullPage />
      ) : (
        props.children
      )}
    </GlobalErrorContext.Provider>
  );
}

export function useGlobalError() {
  const ctx = useContext(GlobalErrorContext);
  if (!ctx) throw new Error('useGlobalError must be used within GlobalErrorProvider');
  return ctx;
}
