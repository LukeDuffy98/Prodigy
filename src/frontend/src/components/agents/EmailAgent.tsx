import React, { useState, useEffect } from 'react';
import axios, { AxiosError } from 'axios';
import { useMsalAuth } from '../../hooks/useMsalAuth';
import { useSimpleAuth } from '../../hooks/useSimpleAuth';

interface ApiError {
  error?: string;
  message?: string;
}

interface EmailSummary {
  id: string;
  subject: string;
  from: {
    name: string;
    address: string;
  };
  receivedDateTime: string;
  isRead: boolean;
  bodyPreview?: string;
}

interface EmailDetail {
  id: string;
  subject: string;
  from: {
    name: string;
    address: string;
  };
  toRecipients: Array<{
    name: string;
    address: string;
  }>;
  ccRecipients: Array<{
    name: string;
    address: string;
  }>;
  receivedDateTime: string;
  body: {
    contentType: string;
    content: string;
  };
  isRead: boolean;
  importance: string;
}

const EmailAgent: React.FC = () => {
  // Choose authentication method based on preference
  const [authMethod, setAuthMethod] = useState<'msal' | 'simple' | null>(null);
  
  // MSAL authentication hook
  const msalAuth = useMsalAuth();
  
  // Simple auth hook
  const simpleAuth = useSimpleAuth();

  // UI state
  const [activeView, setActiveView] = useState<'compose' | 'inbox' | 'details' | 'reply'>('compose');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);
  
  // Email data
  const [emails, setEmails] = useState<EmailSummary[]>([]);
  const [selectedEmail, setSelectedEmail] = useState<EmailDetail | null>(null);
  
  // Form states
  const [emailForm, setEmailForm] = useState({
    recipients: '',
    subject: '',
    body: ''
  });
  
  const [replyForm, setReplyForm] = useState({
    userInput: '',
    replyBody: '',
    isReplyAll: false
  });

  // Simple auth form
  const [credentials, setCredentials] = useState({
    email: '',
    password: ''
  });

  useEffect(() => {
    // Prioritize MSAL authentication
    if (msalAuth.isAuthenticated) {
      setAuthMethod('msal');
      return;
    }
    
    // Check for stored simple auth as fallback
    const token = simpleAuth.checkStoredAuth();
    if (token) {
      setAuthMethod('simple');
      return;
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [msalAuth.isAuthenticated]);

  useEffect(() => {
    const isAuthenticated = authMethod === 'simple' ? simpleAuth.isAuthenticated : msalAuth.isAuthenticated;
    const accessToken = authMethod === 'simple' ? simpleAuth.accessToken : msalAuth.accessToken;
    
    if (isAuthenticated && accessToken) {
      loadInbox();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authMethod, simpleAuth.isAuthenticated, msalAuth.isAuthenticated, simpleAuth.accessToken, msalAuth.accessToken]);

  const handleMsalLogin = async () => {
    try {
      // Clear MSAL-specific cache that might cause cross-origin issues
      Object.keys(localStorage).forEach(key => {
        if (key.startsWith('msal.') || key.includes('clientId') || key.includes('token')) {
          localStorage.removeItem(key);
        }
      });
      
      await msalAuth.login();
      setAuthMethod('msal');
      showMessage('Successfully signed in!', 'success');
    } catch (error) {
      console.error('MSAL login failed:', error);
      showMessage('MSAL login failed. Try the simple login method.', 'error');
    }
  };

  const handleSimpleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await simpleAuth.loginWithCredentials(credentials.email, credentials.password);
      setAuthMethod('simple');
      setCredentials({ email: '', password: '' });
      showMessage('Successfully signed in!', 'success');
    } catch (error) {
      console.error('Simple login failed:', error);
      showMessage('Login failed. Please check your credentials.', 'error');
    }
  };

  const handleLogout = async () => {
    try {
      if (authMethod === 'msal') {
        await msalAuth.logout();
      } else {
        simpleAuth.logout();
      }
      setAuthMethod(null);
      setEmails([]);
      setSelectedEmail(null);
      setActiveView('compose');
      showMessage('Successfully signed out', 'success');
    } catch (error) {
      console.error('Logout failed:', error);
      showMessage('Logout failed', 'error');
    }
  };

  const loadInbox = async () => {
    const token = authMethod === 'simple' ? simpleAuth.getAccessToken() : await msalAuth.getAccessToken();
    if (!token) return;
    
    try {
      setLoading(true);
      const response = await axios.get('/api/agents/email/inbox?top=50', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      setEmails(response.data);
    } catch (error) {
      console.error('Error loading inbox:', error);
      showMessage('Error loading inbox emails', 'error');
    } finally {
      setLoading(false);
    }
  };

  const showEmailDetails = async (messageId: string) => {
    const token = authMethod === 'simple' ? simpleAuth.getAccessToken() : await msalAuth.getAccessToken();
    if (!token) return;
    
    try {
      setLoading(true);
      const response = await axios.get(`/api/agents/email/details/${messageId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      setSelectedEmail(response.data);
      setActiveView('details');
    } catch (error) {
      console.error('Error loading email details:', error);
      showMessage('Error loading email details', 'error');
    } finally {
      setLoading(false);
    }
  };

  const deleteEmail = async (messageId: string) => {
    const token = authMethod === 'simple' ? simpleAuth.getAccessToken() : await msalAuth.getAccessToken();
    if (!token || !confirm('Are you sure you want to delete this email?')) return;
    
    try {
      await axios.delete(`/api/agents/email/delete/${messageId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      showMessage('Email deleted successfully', 'success');
      setSelectedEmail(null);
      setActiveView('inbox');
      loadInbox();
    } catch (error) {
      console.error('Error deleting email:', error);
      showMessage('Error deleting email', 'error');
    }
  };

  const generateAIDraft = async () => {
    const token = authMethod === 'simple' ? simpleAuth.getAccessToken() : await msalAuth.getAccessToken();
    if (!token || !selectedEmail || !replyForm.userInput.trim()) {
      showMessage('Please enter your response intent', 'error');
      return;
    }
    
    try {
      setLoading(true);
      const response = await axios.post('/api/agents/email/generate-ai-draft', {
        originalBody: selectedEmail.body.content,
        userInput: replyForm.userInput,
        originalSubject: selectedEmail.subject,
        originalSender: selectedEmail.from.address
      }, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      setReplyForm(prev => ({ ...prev, replyBody: response.data.draft }));
      showMessage('AI draft generated successfully!', 'success');
    } catch (error) {
      console.error('Error generating AI draft:', error);
      showMessage('Error generating AI draft', 'error');
    } finally {
      setLoading(false);
    }
  };

  const sendReply = async () => {
    const token = authMethod === 'simple' ? simpleAuth.getAccessToken() : await msalAuth.getAccessToken();
    if (!token || !selectedEmail || !replyForm.replyBody.trim()) {
      showMessage('Please enter a reply message', 'error');
      return;
    }
    
    try {
      setLoading(true);
      const endpoint = replyForm.isReplyAll ? 'reply-all' : 'reply';
      await axios.post(`/api/agents/email/${endpoint}`, {
        originalMessageId: selectedEmail.id,
        body: replyForm.replyBody
      }, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      showMessage('Reply sent successfully!', 'success');
      setReplyForm({ userInput: '', replyBody: '', isReplyAll: false });
      setActiveView('inbox');
      loadInbox();
    } catch (error) {
      console.error('Error sending reply:', error);
      showMessage('Error sending reply', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleSendEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setLoading(true);
      await axios.post('/api/agents/email/send', {
        recipients: emailForm.recipients.split(',').map(r => r.trim()),
        subject: emailForm.subject,
        body: emailForm.body
      }, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      showMessage('Email sent successfully!', 'success');
      setEmailForm({ recipients: '', subject: '', body: '' });
    } catch (error) {
      console.error('Error sending email:', error);
      const axiosError = error as AxiosError<ApiError>;
      const errorMessage = axiosError?.response?.data?.error || 
                          axiosError?.response?.data?.message || 
                          'Error sending email';
      showMessage(`Error: ${errorMessage}`, 'error');
    } finally {
      setLoading(false);
    }
  };

  const showMessage = (msg: string, type: 'success' | 'error' = 'success') => {
    setMessage(`${type === 'error' ? '❌' : '✅'} ${msg}`);
    setTimeout(() => setMessage(''), 5000);
  };

  // Determine current authentication state
  const isAuthenticated = authMethod === 'simple' ? simpleAuth.isAuthenticated : msalAuth.isAuthenticated;
  const authLoading = authMethod === 'simple' ? simpleAuth.isLoading : msalAuth.isLoading;
  const authError = authMethod === 'simple' ? simpleAuth.error : msalAuth.error;
  const userName = authMethod === 'simple' ? simpleAuth.userName : msalAuth.account?.name;
  const userEmail = authMethod === 'simple' ? simpleAuth.userEmail : msalAuth.account?.username;

  // Render authentication section
  if (!isAuthenticated) {
    return (
      <div className="agent-content">
        <div className="agent-header">
          <h2>📧 Email Agent</h2>
          <p>Send personalized emails and replies with AI assistance</p>
        </div>

        {authError && (
          <div className="message error">
            ❌ {authError}
          </div>
        )}

        <div className="auth-section">
          <div className="auth-card">
            <h3>🔐 Microsoft Graph Authentication</h3>
            <p>Choose your preferred sign-in method:</p>
            
            {/* MSAL Redirect Login - Now using redirect flow to avoid cross-origin issues */}
            <div className="auth-method">
              <h4>🚀 Microsoft Authentication (Recommended)</h4>
              <p>Secure Microsoft authentication with MFA support (uses redirect flow)</p>
              <button 
                onClick={handleMsalLogin} 
                disabled={authLoading}
                className="login-button msal-button"
                style={{
                  background: 'linear-gradient(135deg, #0078d4, #106ebe)',
                  color: 'white',
                  border: 'none',
                  padding: '12px 24px',
                  borderRadius: '6px',
                  fontSize: '16px',
                  fontWeight: 'bold',
                  cursor: authLoading ? 'not-allowed' : 'pointer',
                  width: '100%',
                  marginBottom: '10px',
                  transition: 'all 0.3s ease',
                  boxShadow: '0 2px 8px rgba(0,120,212,0.3)'
                }}
                onMouseOver={(e) => {
                  if (!authLoading) {
                    (e.target as HTMLButtonElement).style.transform = 'translateY(-2px)';
                    (e.target as HTMLButtonElement).style.boxShadow = '0 4px 12px rgba(0,120,212,0.4)';
                  }
                }}
                onMouseOut={(e) => {
                  (e.target as HTMLButtonElement).style.transform = 'translateY(0)';
                  (e.target as HTMLButtonElement).style.boxShadow = '0 2px 8px rgba(0,120,212,0.3)';
                }}
              >
                {authLoading ? '⏳ Signing in...' : '🚀 Sign in with Microsoft'}
              </button>
              <small className="auth-note">
                ✅ Handles multi-factor authentication automatically<br/>
                ✅ Most secure option - no password storage needed<br/>
                ✅ Uses redirect flow to avoid cross-origin issues
              </small>
            </div>

            <div className="auth-divider" style={{ 
              margin: '20px 0', 
              textAlign: 'center', 
              position: 'relative',
              color: '#666'
            }}>
              <span style={{
                background: 'white',
                padding: '0 15px',
                fontSize: '14px',
                fontWeight: 'bold'
              }}>OR</span>
              <div style={{
                position: 'absolute',
                top: '50%',
                left: 0,
                right: 0,
                height: '1px',
                background: '#ddd',
                zIndex: -1
              }}></div>
            </div>

            {/* Simple Username/Password Login - Now as fallback */}
            <div className="auth-method">
              <h4>📝 Username & Password (Fallback)</h4>
              <form onSubmit={handleSimpleLogin} className="credentials-form">
                <div className="form-group">
                  <label htmlFor="email">Email:</label>
                  <input
                    type="email"
                    id="email"
                    value={credentials.email}
                    onChange={(e) => setCredentials(prev => ({ ...prev, email: e.target.value }))}
                    placeholder="your-email@company.com"
                    required
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="password">Password:</label>
                  <input
                    type="password"
                    id="password"
                    value={credentials.password}
                    onChange={(e) => setCredentials(prev => ({ ...prev, password: e.target.value }))}
                    placeholder="Your password"
                    required
                  />
                </div>
                <button 
                  type="submit" 
                  disabled={authLoading}
                  className="login-button"
                >
                  {authLoading ? '⏳ Signing in...' : '🔑 Sign in with Credentials'}
                </button>
              </form>
              <small className="auth-note">
                ⚠️ May require additional MFA setup if enforced by your organization
              </small>
            </div>

            <p className="auth-note">
              <small>
                <strong>Required permissions:</strong> Mail.ReadWrite, Mail.Send, User.Read<br/>
                Your credentials are handled securely by Microsoft's authentication system.
              </small>
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="agent-content">
      <div className="agent-header">
        <h2>📧 Email Agent</h2>
        <p>Send personalized emails and replies with AI assistance</p>
        <div className="user-info">
          Signed in as: {userName || userEmail || 'User'}
          <button onClick={handleLogout} className="logout-button">Logout</button>
        </div>
      </div>

      {message && (
        <div className={`message ${message.includes('❌') ? 'error' : 'success'}`}>
          {message}
        </div>
      )}

      <div className="email-tabs">
        <button 
          className={activeView === 'compose' ? 'tab active' : 'tab'}
          onClick={() => setActiveView('compose')}
        >
          ✍️ Compose
        </button>
        <button 
          className={activeView === 'inbox' ? 'tab active' : 'tab'}
          onClick={() => {
            setActiveView('inbox');
            loadInbox();
          }}
        >
          📬 Inbox ({emails.length})
        </button>
        {selectedEmail && (
          <button 
            className={activeView === 'details' ? 'tab active' : 'tab'}
            onClick={() => setActiveView('details')}
          >
            📄 Details
          </button>
        )}
        {selectedEmail && activeView === 'reply' && (
          <button className="tab active">
            ↩️ Reply
          </button>
        )}
      </div>

      {/* Compose View */}
      {activeView === 'compose' && (
        <div className="compose-section">
          <form onSubmit={handleSendEmail} className="email-form">
            <div className="form-group">
              <label htmlFor="recipients">To (comma-separated):</label>
              <input
                type="text"
                id="recipients"
                value={emailForm.recipients}
                onChange={(e) => setEmailForm(prev => ({ ...prev, recipients: e.target.value }))}
                placeholder="email1@example.com, email2@example.com"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="subject">Subject:</label>
              <input
                type="text"
                id="subject"
                value={emailForm.subject}
                onChange={(e) => setEmailForm(prev => ({ ...prev, subject: e.target.value }))}
                placeholder="Email subject"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="body">Message:</label>
              <textarea
                id="body"
                value={emailForm.body}
                onChange={(e) => setEmailForm(prev => ({ ...prev, body: e.target.value }))}
                placeholder="Email body content... (AI will personalize this using your profile)"
                rows={8}
                required
              />
            </div>

            <div className="form-actions">
              <button type="submit" disabled={loading} className="send-button">
                {loading ? 'Sending...' : '📤 Send Email'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Inbox View */}
      {activeView === 'inbox' && (
        <div className="inbox-section">
          <div className="inbox-header">
            <h3>📬 Inbox</h3>
            <button onClick={loadInbox} disabled={loading} className="refresh-button">
              {loading ? '⏳ Loading...' : '🔄 Refresh'}
            </button>
          </div>
          
          <div className="email-list">
            {emails.length === 0 ? (
              <div className="empty-inbox">
                {loading ? 'Loading emails...' : 'No emails found'}
              </div>
            ) : (
              emails.map(email => (
                <div 
                  key={email.id} 
                  className={`email-item ${!email.isRead ? 'unread' : ''}`}
                  onClick={() => showEmailDetails(email.id)}
                >
                  <div className="email-subject">
                    <strong>{email.subject || '(No subject)'}</strong>
                  </div>
                  <div className="email-from">
                    {email.from.name || email.from.address}
                  </div>
                  <div className="email-date">
                    {new Date(email.receivedDateTime).toLocaleString()}
                  </div>
                  {email.bodyPreview && (
                    <div className="email-preview">
                      {email.bodyPreview.substring(0, 100)}...
                    </div>
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      )}

      {/* Email Details View */}
      {activeView === 'details' && selectedEmail && (
        <div className="email-details">
          <div className="email-header-info">
            <h3>{selectedEmail.subject || '(No subject)'}</h3>
            <p><strong>From:</strong> {selectedEmail.from.name || selectedEmail.from.address}</p>
            <p><strong>Date:</strong> {new Date(selectedEmail.receivedDateTime).toLocaleString()}</p>
            {selectedEmail.toRecipients.length > 0 && (
              <p><strong>To:</strong> {selectedEmail.toRecipients.map(r => r.name || r.address).join(', ')}</p>
            )}
          </div>
          
          <div className="email-body" dangerouslySetInnerHTML={{ 
            __html: selectedEmail.body.content || '(No content)' 
          }} />
          
          <div className="email-actions">
            <button 
              onClick={() => deleteEmail(selectedEmail.id)} 
              className="delete-button"
            >
              🗑️ Delete
            </button>
            <button 
              onClick={() => {
                setReplyForm(prev => ({ ...prev, isReplyAll: false }));
                setActiveView('reply');
              }}
              className="reply-button"
            >
              ↩️ Reply
            </button>
            <button 
              onClick={() => {
                setReplyForm(prev => ({ ...prev, isReplyAll: true }));
                setActiveView('reply');
              }}
              className="reply-all-button"
            >
              ↩️📤 Reply All
            </button>
          </div>
        </div>
      )}

      {/* Reply View */}
      {activeView === 'reply' && selectedEmail && (
        <div className="reply-section">
          <h3>{replyForm.isReplyAll ? '↩️📤 Reply All' : '↩️ Reply'}</h3>
          <p><strong>Original:</strong> {selectedEmail.subject}</p>
          
          <div className="form-group">
            <label htmlFor="userInput">Your response intent:</label>
            <textarea
              id="userInput"
              value={replyForm.userInput}
              onChange={(e) => setReplyForm(prev => ({ ...prev, userInput: e.target.value }))}
              placeholder="Describe what you want to say in your reply..."
              rows={3}
            />
            <button 
              onClick={generateAIDraft} 
              disabled={loading || !replyForm.userInput.trim()}
              className="ai-draft-button"
            >
              {loading ? '⏳ Generating...' : '🤖 Generate AI Draft'}
            </button>
          </div>

          <div className="form-group">
            <label htmlFor="replyBody">Email content:</label>
            <textarea
              id="replyBody"
              value={replyForm.replyBody}
              onChange={(e) => setReplyForm(prev => ({ ...prev, replyBody: e.target.value }))}
              placeholder="Your reply will appear here..."
              rows={8}
            />
          </div>

          <div className="form-actions">
            <button 
              onClick={sendReply} 
              disabled={loading || !replyForm.replyBody.trim()}
              className="send-reply-button"
            >
              {loading ? 'Sending...' : `📤 Send ${replyForm.isReplyAll ? 'Reply All' : 'Reply'}`}
            </button>
            <button 
              onClick={() => setActiveView('details')} 
              className="cancel-button"
            >
              ❌ Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default EmailAgent;