import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface FeatureRequest {
  title: string;
  description: string;
  labels: string[];
  assignedTo: string;
  priority: string;
  milestone: string;
}

interface FeatureRequestResponse {
  issueNumber: number;
  issueUrl: string;
  title: string;
  state: string;
  assignedTo: string;
  labels: string[];
  createdAt: string;
}

const GitHubAgent: React.FC = () => {
  const [isCreating, setIsCreating] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [featureRequests, setFeatureRequests] = useState<FeatureRequestResponse[]>([]);
  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'info'; text: string } | null>(null);
  
  const [newRequest, setNewRequest] = useState<FeatureRequest>({
    title: '',
    description: '',
    labels: [],
    assignedTo: '',
    priority: 'Medium',
    milestone: ''
  });

  const API_BASE = 'http://localhost:5000/api/agents/github';

  useEffect(() => {
    loadFeatureRequests();
  }, []);

  const loadFeatureRequests = async () => {
    try {
      setIsLoading(true);
      const response = await axios.get(`${API_BASE}/feature-requests`);
      setFeatureRequests(response.data);
    } catch (error) {
      console.error('Error loading feature requests:', error);
      setMessage({ type: 'info', text: 'Unable to load feature requests. Azure Functions may not be running.' });
    } finally {
      setIsLoading(false);
    }
  };

  const createFeatureRequest = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newRequest.title.trim() || !newRequest.description.trim()) {
      setMessage({ type: 'error', text: 'Title and description are required' });
      return;
    }

    setIsCreating(true);
    try {
      const response = await axios.post(`${API_BASE}/feature-request`, newRequest);
      
      setFeatureRequests(prev => [response.data, ...prev]);
      setNewRequest({
        title: '',
        description: '',
        labels: [],
        assignedTo: '',
        priority: 'Medium',
        milestone: ''
      });
      setMessage({ 
        type: 'success', 
        text: `Feature request created successfully! Issue #${response.data.issueNumber}` 
      });
    } catch (error: any) {
      console.error('Error creating feature request:', error);
      const errorMessage = error.response?.data || 'Failed to create feature request. Check if GitHub token is configured.';
      setMessage({ type: 'error', text: errorMessage });
    } finally {
      setIsCreating(false);
    }
  };

  const closeFeatureRequest = async (issueNumber: number) => {
    try {
      await axios.post(`${API_BASE}/feature-request/${issueNumber}/close`);
      setFeatureRequests(prev => 
        prev.map(req => 
          req.issueNumber === issueNumber 
            ? { ...req, state: 'closed' }
            : req
        )
      );
      setMessage({ type: 'success', text: `Feature request #${issueNumber} closed successfully` });
    } catch (error) {
      console.error('Error closing feature request:', error);
      setMessage({ type: 'error', text: 'Failed to close feature request' });
    }
  };

  const handleLabelsChange = (value: string) => {
    const labels = value.split(',').map(label => label.trim()).filter(label => label.length > 0);
    setNewRequest(prev => ({ ...prev, labels }));
  };

  return (
    <div className="agent-content">
      <div className="agent-header">
        <h2>🔧 GitHub Agent</h2>
        <p>Create and manage GitHub feature requests</p>
      </div>

      {message && (
        <div className={`message ${message.type}`}>
          {message.text}
          <button onClick={() => setMessage(null)}>×</button>
        </div>
      )}

      <div className="github-sections">
        {/* Create Feature Request Section */}
        <section className="feature-request-form">
          <h3>📝 Create Feature Request</h3>
          <form onSubmit={createFeatureRequest}>
            <div className="form-group">
              <label htmlFor="title">Title *</label>
              <input
                id="title"
                type="text"
                value={newRequest.title}
                onChange={(e) => setNewRequest(prev => ({ ...prev, title: e.target.value }))}
                placeholder="Brief description of the feature"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="description">Description *</label>
              <textarea
                id="description"
                rows={4}
                value={newRequest.description}
                onChange={(e) => setNewRequest(prev => ({ ...prev, description: e.target.value }))}
                placeholder="Detailed description of the feature request..."
                required
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="assignedTo">Assign To</label>
                <input
                  id="assignedTo"
                  type="text"
                  value={newRequest.assignedTo}
                  onChange={(e) => setNewRequest(prev => ({ ...prev, assignedTo: e.target.value }))}
                  placeholder="e.g., github-copilot"
                />
              </div>

              <div className="form-group">
                <label htmlFor="priority">Priority</label>
                <select
                  id="priority"
                  value={newRequest.priority}
                  onChange={(e) => setNewRequest(prev => ({ ...prev, priority: e.target.value }))}
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </select>
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="labels">Labels (comma-separated)</label>
              <input
                id="labels"
                type="text"
                value={newRequest.labels.join(', ')}
                onChange={(e) => handleLabelsChange(e.target.value)}
                placeholder="enhancement, feature, ui"
              />
            </div>

            <div className="form-group">
              <label htmlFor="milestone">Milestone</label>
              <input
                id="milestone"
                type="text"
                value={newRequest.milestone}
                onChange={(e) => setNewRequest(prev => ({ ...prev, milestone: e.target.value }))}
                placeholder="v1.0.0"
              />
            </div>

            <button type="submit" disabled={isCreating} className="submit-button">
              {isCreating ? 'Creating...' : 'Create Feature Request'}
            </button>
          </form>
        </section>

        {/* Feature Requests List Section */}
        <section className="feature-requests-list">
          <div className="section-header">
            <h3>📋 Feature Requests</h3>
            <button onClick={loadFeatureRequests} disabled={isLoading} className="refresh-button">
              {isLoading ? 'Loading...' : '🔄 Refresh'}
            </button>
          </div>

          {featureRequests.length === 0 ? (
            <div className="empty-state">
              <p>No feature requests found. Create your first one above!</p>
            </div>
          ) : (
            <div className="requests-grid">
              {featureRequests.map((request) => (
                <div key={request.issueNumber} className="request-card">
                  <div className="request-header">
                    <h4>#{request.issueNumber} {request.title}</h4>
                    <span className={`status ${request.state}`}>{request.state}</span>
                  </div>
                  
                  <div className="request-meta">
                    {request.assignedTo && (
                      <span className="assignee">👤 {request.assignedTo}</span>
                    )}
                    <span className="date">📅 {new Date(request.createdAt).toLocaleDateString()}</span>
                  </div>

                  {request.labels.length > 0 && (
                    <div className="labels">
                      {request.labels.map((label, index) => (
                        <span key={index} className="label">{label}</span>
                      ))}
                    </div>
                  )}

                  <div className="request-actions">
                    <a 
                      href={request.issueUrl} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="view-button"
                    >
                      View on GitHub
                    </a>
                    {request.state === 'open' && (
                      <button 
                        onClick={() => closeFeatureRequest(request.issueNumber)}
                        className="close-button"
                      >
                        Close
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
};

export default GitHubAgent;