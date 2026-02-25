import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export type Toast = {
  id: string;
  type: ToastType;
  message: string;
  title?: string;
  duration?: number;
  createdAt: number;
};

export type BarMessage = {
  id: string;
  type: ToastType;
  message: string;
  title?: string;
  dismissible?: boolean;
};

type NotificationContextValue = {
  toasts: Toast[];
  bar: BarMessage | null;
  addToast: (type: ToastType, message: string, options?: { title?: string; duration?: number }) => void;
  removeToast: (id: string) => void;
  addBar: (type: ToastType, message: string, options?: { title?: string; dismissible?: boolean }) => void;
  clearBar: () => void;
};

const DEFAULT_DURATION = 5000;
let toastId = 0;
let barId = 0;

const NotificationContext = createContext<NotificationContextValue | null>(null);

export function NotificationProvider(props: { readonly children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const [bar, setBar] = useState<BarMessage | null>(null);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const addToast = useCallback(
    (type: ToastType, message: string, options?: { title?: string; duration?: number }) => {
      const id = `toast-${++toastId}`;
      const duration = options?.duration ?? DEFAULT_DURATION;
      const toast: Toast = {
        id,
        type,
        message,
        title: options?.title,
        duration,
        createdAt: Date.now(),
      };
      setToasts((prev) => [...prev, toast]);
      if (duration > 0) {
        setTimeout(() => removeToast(id), duration);
      }
    },
    [removeToast]
  );

  const addBar = useCallback(
    (type: ToastType, message: string, options?: { title?: string; dismissible?: boolean }) => {
      setBar({
        id: `bar-${++barId}`,
        type,
        message,
        title: options?.title,
        dismissible: options?.dismissible ?? true,
      });
    },
    []
  );

  const clearBar = useCallback(() => setBar(null), []);

  const value = useMemo(
    () => ({ toasts, bar, addToast, removeToast, addBar, clearBar }),
    [toasts, bar, addToast, removeToast, addBar, clearBar]
  );

  return (
    <NotificationContext.Provider value={value}>
      {props.children}
    </NotificationContext.Provider>
  );
}

export function useNotifications() {
  const ctx = useContext(NotificationContext);
  if (!ctx) throw new Error('useNotifications must be used within NotificationProvider');
  return ctx;
}
