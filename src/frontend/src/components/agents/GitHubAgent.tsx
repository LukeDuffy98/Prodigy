import React from 'react';

const GitHubAgent: React.FC = () => {
  return (
    <div className="agent-content">
      <div className="agent-header">
        <h2>🔧 GitHub Agent</h2>
        <p>Create and manage GitHub feature requests</p>
      </div>
      
      <div className="coming-soon">
        <h3>🚧 Coming Soon</h3>
        <p>The GitHub Agent will help you manage feature requests:</p>
        <ul>
          <li>📝 Create GitHub issues</li>
          <li>🏷️ Automatic labeling</li>
          <li>👤 Assignment to users</li>
          <li>📊 Issue tracking</li>
        </ul>
      </div>
    </div>
  );
};

export default GitHubAgent;