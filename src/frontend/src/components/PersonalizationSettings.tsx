import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface PersonalizationProfile {
  tone: string;
  preferredGreetings: string[];
  signatureClosings: string[];
  favouritePhrases: string[];
  prohibitedWords: string[];
  sampleTexts: string[];
  aboutMe: string;
  customAgentHints: { [key: string]: string };
}

const PersonalizationSettings: React.FC = () => {
  const [profile, setProfile] = useState<PersonalizationProfile>({
    tone: 'professional',
    preferredGreetings: [],
    signatureClosings: [],
    favouritePhrases: [],
    prohibitedWords: [],
    sampleTexts: [],
    aboutMe: '',
    customAgentHints: {}
  });
  
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setIsLoading(true);
      const response = await axios.get('/api/user/personalization-profile');
      setProfile(response.data);
    } catch (error) {
      console.error('Error loading profile:', error);
      setMessage('Error loading personalization profile');
    } finally {
      setIsLoading(false);
    }
  };

  const saveProfile = async () => {
    try {
      setIsSaving(true);
      await axios.post('/api/user/personalization-profile', profile);
      setMessage('Profile saved successfully!');
      setTimeout(() => setMessage(''), 3000);
    } catch (error) {
      console.error('Error saving profile:', error);
      setMessage('Error saving profile');
    } finally {
      setIsSaving(false);
    }
  };

  const addArrayItem = (field: keyof PersonalizationProfile, value: string) => {
    if (value.trim() && Array.isArray(profile[field])) {
      setProfile(prev => ({
        ...prev,
        [field]: [...(prev[field] as string[]), value.trim()]
      }));
    }
  };

  const removeArrayItem = (field: keyof PersonalizationProfile, index: number) => {
    setProfile(prev => ({
      ...prev,
      [field]: (prev[field] as string[]).filter((_, i) => i !== index)
    }));
  };

  if (isLoading) {
    return <div className="loading">Loading personalization settings...</div>;
  }

  return (
    <div className="personalization-settings">
      <h2>🎨 Personalization Profile</h2>
      <p className="description">
        Configure how AI agents communicate on your behalf. This profile ensures all generated content matches your unique voice and style.
      </p>

      {message && (
        <div className={`message ${message.includes('Error') ? 'error' : 'success'}`}>
          {message}
        </div>
      )}

      <div className="settings-grid">
        <div className="setting-section">
          <h3>Communication Tone</h3>
          <select 
            value={profile.tone} 
            onChange={(e) => setProfile(prev => ({ ...prev, tone: e.target.value }))}
          >
            <option value="professional">Professional</option>
            <option value="friendly">Friendly</option>
            <option value="formal">Formal</option>
            <option value="casual">Casual</option>
            <option value="enthusiastic">Enthusiastic</option>
            <option value="concise">Concise</option>
          </select>
        </div>

        <div className="setting-section">
          <h3>About Me</h3>
          <textarea
            placeholder="Describe yourself and your communication style..."
            value={profile.aboutMe}
            onChange={(e) => setProfile(prev => ({ ...prev, aboutMe: e.target.value }))}
            rows={3}
          />
        </div>

        <div className="setting-section">
          <h3>Preferred Greetings</h3>
          <ArrayEditor 
            items={profile.preferredGreetings}
            onAdd={(value) => addArrayItem('preferredGreetings', value)}
            onRemove={(index) => removeArrayItem('preferredGreetings', index)}
            placeholder="Add greeting (e.g., Hi, Hello, Dear)"
          />
        </div>

        <div className="setting-section">
          <h3>Signature Closings</h3>
          <ArrayEditor 
            items={profile.signatureClosings}
            onAdd={(value) => addArrayItem('signatureClosings', value)}
            onRemove={(index) => removeArrayItem('signatureClosings', index)}
            placeholder="Add closing (e.g., Best regards, Thanks)"
          />
        </div>

        <div className="setting-section">
          <h3>Favorite Phrases</h3>
          <ArrayEditor 
            items={profile.favouritePhrases}
            onAdd={(value) => addArrayItem('favouritePhrases', value)}
            onRemove={(index) => removeArrayItem('favouritePhrases', index)}
            placeholder="Add phrase you like to use"
          />
        </div>

        <div className="setting-section">
          <h3>Words to Avoid</h3>
          <ArrayEditor 
            items={profile.prohibitedWords}
            onAdd={(value) => addArrayItem('prohibitedWords', value)}
            onRemove={(index) => removeArrayItem('prohibitedWords', index)}
            placeholder="Add word to avoid"
          />
        </div>

        <div className="setting-section full-width">
          <h3>Sample Texts</h3>
          <p className="help-text">Provide examples of your writing to help AI learn your style</p>
          <ArrayEditor 
            items={profile.sampleTexts}
            onAdd={(value) => addArrayItem('sampleTexts', value)}
            onRemove={(index) => removeArrayItem('sampleTexts', index)}
            placeholder="Add sample text that represents your writing style"
            isTextArea={true}
          />
        </div>
      </div>

      <div className="actions">
        <button 
          className="save-button" 
          onClick={saveProfile}
          disabled={isSaving}
        >
          {isSaving ? 'Saving...' : 'Save Profile'}
        </button>
      </div>
    </div>
  );
};

interface ArrayEditorProps {
  items: string[];
  onAdd: (value: string) => void;
  onRemove: (index: number) => void;
  placeholder: string;
  isTextArea?: boolean;
}

const ArrayEditor: React.FC<ArrayEditorProps> = ({ items, onAdd, onRemove, placeholder, isTextArea = false }) => {
  const [inputValue, setInputValue] = useState('');

  const handleAdd = () => {
    if (inputValue.trim()) {
      onAdd(inputValue);
      setInputValue('');
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleAdd();
    }
  };

  return (
    <div className="array-editor">
      <div className="input-group">
        {isTextArea ? (
          <textarea
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder={placeholder}
            rows={2}
          />
        ) : (
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder={placeholder}
          />
        )}
        <button type="button" onClick={handleAdd}>Add</button>
      </div>
      
      <div className="items-list">
        {items.map((item, index) => (
          <div key={index} className="item-tag">
            <span>{item}</span>
            <button type="button" onClick={() => onRemove(index)}>×</button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PersonalizationSettings;