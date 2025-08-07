import React, { useState } from 'react';
import EmailAgent from './agents/EmailAgent';
import TaskAgent from './agents/TaskAgent';
import LearningAgent from './agents/LearningAgent';
import QuoteAgent from './agents/QuoteAgent';
import CalendarAgent from './agents/CalendarAgent';
import GitHubAgent from './agents/GitHubAgent';

type AgentType = 'email' | 'task' | 'learning' | 'quote' | 'calendar' | 'github';

const AgentPanel: React.FC = () => {
  const [activeAgent, setActiveAgent] = useState<AgentType>('email');

  const agents = [
    { id: 'email', name: 'Email Agent', icon: '📧', description: 'Send and reply to emails with AI assistance' },
    { id: 'task', name: 'Task Agent', icon: '📋', description: 'Create tasks with detailed execution plans' },
    { id: 'learning', name: 'Learning Agent', icon: '📚', description: 'Generate structured learning materials' },
    { id: 'quote', name: 'Quote Agent', icon: '💼', description: 'Create professional quotes and estimates' },
    { id: 'calendar', name: 'Calendar Agent', icon: '📅', description: 'Find availability and schedule meetings' },
    { id: 'github', name: 'GitHub Agent', icon: '🔧', description: 'Create and manage feature requests' },
  ];

  const renderActiveAgent = () => {
    switch (activeAgent) {
      case 'email':
        return <EmailAgent />;
      case 'task':
        return <TaskAgent />;
      case 'learning':
        return <LearningAgent />;
      case 'quote':
        return <QuoteAgent />;
      case 'calendar':
        return <CalendarAgent />;
      case 'github':
        return <GitHubAgent />;
      default:
        return <EmailAgent />;
    }
  };

  return (
    <div className="agent-panel">
      <div className="agent-sidebar">
        <h3>Available Agents</h3>
        <div className="agent-list">
          {agents.map((agent) => (
            <button
              key={agent.id}
              className={`agent-button ${activeAgent === agent.id ? 'active' : ''}`}
              onClick={() => setActiveAgent(agent.id as AgentType)}
            >
              <span className="agent-icon">{agent.icon}</span>
              <div className="agent-info">
                <div className="agent-name">{agent.name}</div>
                <div className="agent-description">{agent.description}</div>
              </div>
            </button>
          ))}
        </div>
      </div>
      
      <div className="agent-content">
        {renderActiveAgent()}
      </div>
    </div>
  );
};

export default AgentPanel;