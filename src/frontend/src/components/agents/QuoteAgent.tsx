import React from 'react';

const QuoteAgent: React.FC = () => {
  return (
    <div className="agent-content">
      <div className="agent-header">
        <h2>💼 Quote Agent</h2>
        <p>Create professional quotes and estimates</p>
      </div>
      
      <div className="coming-soon">
        <h3>🚧 Coming Soon</h3>
        <p>The Quote Agent will help you create professional quotes including:</p>
        <ul>
          <li>💰 Itemized pricing</li>
          <li>📄 PDF generation</li>
          <li>📧 Email delivery</li>
          <li>🏢 Client management</li>
        </ul>
      </div>
    </div>
  );
};

export default QuoteAgent;