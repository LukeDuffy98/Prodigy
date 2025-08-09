import React, { useState } from 'react';
import axios, { AxiosError } from 'axios';

interface ApiError {
  error?: string;
  message?: string;
}

const EmailAgent: React.FC = () => {
  const [emailForm, setEmailForm] = useState({
    recipients: '',
    subject: '',
    body: ''
  });
  const [isSending, setIsSending] = useState(false);
  const [message, setMessage] = useState('');

  const handleSendEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setIsSending(true);
      await axios.post('/api/agents/email/send', {
        recipients: emailForm.recipients.split(',').map(r => r.trim()),
        subject: emailForm.subject,
        body: emailForm.body
      }, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      setMessage('Email sent successfully!');
      setEmailForm({ recipients: '', subject: '', body: '' });
    } catch (error) {
      console.error('Error sending email:', error);
      const axiosError = error as AxiosError<ApiError>;
      const errorMessage = axiosError?.response?.data?.error || 
                          axiosError?.response?.data?.message || 
                          'Error sending email';
      setMessage(`Error: ${errorMessage}`);
    } finally {
      setIsSending(false);
      setTimeout(() => setMessage(''), 3000);
    }
  };

  const handleDraftEmail = async () => {
    try {
      const response = await axios.post('/api/agents/email/draft', {
        prompt: `Create an email about: ${emailForm.subject}`,
        context: emailForm.body ? `Additional context: ${emailForm.body}` : undefined
      }, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      setEmailForm(prev => ({
        ...prev,
        subject: response.data.subject,
        body: response.data.body
      }));
      
      setMessage('AI draft generated! Review and edit as needed.');
    } catch (error) {
      console.error('Error drafting email:', error);
      const axiosError = error as AxiosError<ApiError>;
      const errorMessage = axiosError?.response?.data?.error || 
                          axiosError?.response?.data?.message || 
                          'Error generating draft';
      setMessage(`Error: ${errorMessage}`);
    }
    setTimeout(() => setMessage(''), 3000);
  };

  return (
    <div className="agent-content">
      <div className="agent-header">
        <h2>📧 Email Agent</h2>
        <p>Send personalized emails and replies with AI assistance</p>
      </div>

      {message && (
        <div className={`message ${message.includes('Error') ? 'error' : 'success'}`}>
          {message}
        </div>
      )}

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
          <div className="subject-input-group">
            <input
              type="text"
              id="subject"
              value={emailForm.subject}
              onChange={(e) => setEmailForm(prev => ({ ...prev, subject: e.target.value }))}
              placeholder="Email subject"
              required
            />
            <button 
              type="button" 
              onClick={handleDraftEmail}
              className="draft-button"
              disabled={!emailForm.subject.trim()}
            >
              🤖 AI Draft
            </button>
          </div>
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
          <button type="submit" disabled={isSending} className="send-button">
            {isSending ? 'Sending...' : '📤 Send Email'}
          </button>
        </div>
      </form>

      <div className="agent-features">
        <h3>Features</h3>
        <ul>
          <li>✨ AI-powered email drafting with your personal style</li>
          <li>📝 Automatic personalization using your profile</li>
          <li>↩️ Smart reply generation for existing threads</li>
          <li>📎 Attachment support (coming soon)</li>
        </ul>
      </div>
    </div>
  );
};

export default EmailAgent;