import { Routes, Route, NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { NotificationProvider } from './context/NotificationContext';
import Header from './components/Header';
import NotificationBar from './components/NotificationBar';
import ToastList from './components/ToastList';
import {
  IconDashboard,
  IconAgents,
  IconAudit,
  IconPolicies,
  IconRag,
  IconControlPlane,
} from './components/ui/Icons';
import Dashboard from './pages/Dashboard';
import Agents from './pages/Agents';
import AgentRun from './pages/AgentRun';
import Audit from './pages/Audit';
import Policies from './pages/Policies';
import Rag from './pages/Rag';
import ControlPlane from './pages/ControlPlane';

function NavLinkWithIcon(props: {
  readonly to: string;
  readonly end?: boolean;
  readonly icon: () => JSX.Element;
  readonly children: string;
}) {
  const { to, end, icon: Icon, children } = props;
  return (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
    >
      <span className="nav-link-icon"><Icon /></span>
      <span className="nav-link-label">{children}</span>
    </NavLink>
  );
}

function App() {
  const { t } = useTranslation();
  return (
    <NotificationProvider>
      <div className="app">
        <NotificationBar />
        <div className="app-body">
          <aside className="nav">
            <div className="nav-brand">
              <span className="nav-brand-text">{t('nav.brand')}</span>
            </div>
            <nav className="nav-links" aria-label={t('nav.ariaMenu')}>
              <NavLinkWithIcon to="/" end icon={IconDashboard}>{t('nav.dashboard')}</NavLinkWithIcon>
              <NavLinkWithIcon to="/agents" icon={IconAgents}>{t('nav.agents')}</NavLinkWithIcon>
              <NavLinkWithIcon to="/audit" icon={IconAudit}>{t('nav.audit')}</NavLinkWithIcon>
              <NavLinkWithIcon to="/policies" icon={IconPolicies}>{t('nav.policies')}</NavLinkWithIcon>
              <NavLinkWithIcon to="/rag" icon={IconRag}>{t('nav.rag')}</NavLinkWithIcon>
              <NavLinkWithIcon to="/control-plane" icon={IconControlPlane}>{t('nav.controlPlane')}</NavLinkWithIcon>
            </nav>
          </aside>
          <div className="content-wrap">
            <Header />
            <main className="main">
              <div className="main-inner">
              <Routes>
                <Route path="/" element={<Dashboard />} />
                <Route path="/agents" element={<Agents />} />
                <Route path="/agents/:id/run" element={<AgentRun />} />
                <Route path="/audit" element={<Audit />} />
                <Route path="/policies" element={<Policies />} />
                <Route path="/rag" element={<Rag />} />
                <Route path="/control-plane" element={<ControlPlane />} />
              </Routes>
              </div>
            </main>
          </div>
        </div>
      </div>
      <ToastList />
    </NotificationProvider>
  );
}

export default App;
