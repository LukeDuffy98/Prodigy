import React from 'react';

const Dashboard: React.FC = () => {
  return (
    <div className="dashboard">
      <div className="welcome-section">
        <h2>Welcome to Prodigy</h2>
        <p>Your intelligent digital workspace with AI-powered agents</p>
      </div>
      
      <div className="stats-grid">
        <div className="stat-card">
          <h3>📧 Email Agent</h3>
          <p>Send personalized emails and replies</p>
          <div className="stat-number">0</div>
          <span>emails sent today</span>
        </div>
        
        <div className="stat-card">
          <h3>📋 Task Agent</h3>
          <p>Create tasks with AI execution plans</p>
          <div className="stat-number">0</div>
          <span>active tasks</span>
        </div>
        
        <div className="stat-card">
          <h3>📚 Learning Agent</h3>
          <p>Generate structured learning materials</p>
          <div className="stat-number">0</div>
          <span>materials created</span>
        </div>
        
        <div className="stat-card">
          <h3>💼 Quote Agent</h3>
          <p>Professional quote generation</p>
          <div className="stat-number">0</div>
          <span>quotes generated</span>
        </div>
        
        <div className="stat-card">
          <h3>📅 Calendar Agent</h3>
          <p>Advanced availability lookup</p>
          <div className="stat-number">0</div>
          <span>meetings scheduled</span>
        </div>
        
        <div className="stat-card">
          <h3>🔧 GitHub Agent</h3>
          <p>Feature request management</p>
          <div className="stat-number">0</div>
          <span>issues created</span>
        </div>
      </div>
      
      <div className="recent-activity">
        <h3>Recent Activity</h3>
        <div className="activity-list">
          <p className="no-activity">No recent activity. Start using your agents to see updates here!</p>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;