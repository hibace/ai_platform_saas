import { Component, type ErrorInfo, type ReactNode } from 'react';
import ErrorPage from './ErrorPage';

type Props = {
  children: ReactNode;
};

type State = {
  error: Error | null;
};

export default class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught:', error, errorInfo);
  }

  handleRetry = () => {
    this.setState({ error: null });
  };

  render() {
    if (this.state.error) {
      return (
        <ErrorPage
          error={this.state.error}
          onRetry={this.handleRetry}
          fullPage
        />
      );
    }
    return this.props.children;
  }
}
