import { useState } from 'react';
import './App.css';
import Dashboard from './components/Dashboard';
import AgentPanel from './components/AgentPanel';
import PersonalizationSettings from './components/PersonalizationSettings';

function App() {
  const [activeView, setActiveView] = useState<'dashboard' | 'agents' | 'personalization'>('dashboard');

  return (
    <div className="App">
      <header className="App-header">
        <nav className="navigation">
          <div className="nav-brand">
            <h1>🤖 Prodigy</h1>
            <p>Your Intelligent Digital Workspace</p>
          </div>
          <div className="nav-links">
            <button 
              className={activeView === 'dashboard' ? 'active' : ''}
              onClick={() => setActiveView('dashboard')}
            >
              Dashboard
            </button>
            <button 
              className={activeView === 'agents' ? 'active' : ''}
              onClick={() => setActiveView('agents')}
            >
              Agents
            </button>
            <button 
              className={activeView === 'personalization' ? 'active' : ''}
              onClick={() => setActiveView('personalization')}
            >
              Personalization
            </button>
          </div>
        </nav>
      </header>

      <main className="main-content">
        {activeView === 'dashboard' && <Dashboard />}
        {activeView === 'agents' && <AgentPanel />}
        {activeView === 'personalization' && <PersonalizationSettings />}
      </main>

      <footer className="App-footer">
        <p>Prodigy - Digital agents at your command | Built with ❤️ for productivity</p>
      </footer>
    </div>
  );
}

export default App;
