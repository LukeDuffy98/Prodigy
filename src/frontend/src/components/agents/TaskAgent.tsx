import React, { useState } from 'react';
import axios from 'axios';

interface Task {
  id: string;
  title: string;
  description: string;
  dueDate?: string;
  priority: string;
  tags: string[];
  executionPlan: ExecutionStep[];
  createdAt: string;
}

interface ExecutionStep {
  stepNumber: number;
  description: string;
  estimatedTime: string;
  dependencies: string[];
  isCompleted: boolean;
}

const TaskAgent: React.FC = () => {
  const [taskForm, setTaskForm] = useState({
    title: '',
    description: '',
    dueDate: '',
    priority: 'Medium',
    tags: ''
  });
  const [isCreating, setIsCreating] = useState(false);
  const [createdTask, setCreatedTask] = useState<Task | null>(null);
  const [message, setMessage] = useState('');

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setIsCreating(true);
      const response = await axios.post('/api/agents/tasks', {
        title: taskForm.title,
        description: taskForm.description,
        dueDate: taskForm.dueDate ? new Date(taskForm.dueDate).toISOString() : null,
        priority: taskForm.priority,
        tags: taskForm.tags.split(',').map(t => t.trim()).filter(t => t)
      });
      
      setCreatedTask(response.data);
      setMessage('Task created with AI execution plan!');
      setTaskForm({ title: '', description: '', dueDate: '', priority: 'Medium', tags: '' });
    } catch (error) {
      console.error('Error creating task:', error);
      setMessage('Error creating task');
    } finally {
      setIsCreating(false);
      setTimeout(() => setMessage(''), 3000);
    }
  };

  const toggleStepCompletion = async (stepNumber: number) => {
    if (!createdTask) return;
    
    try {
      const step = createdTask.executionPlan.find(s => s.stepNumber === stepNumber);
      if (step) {
        await axios.patch(`/api/agents/tasks/${createdTask.id}/steps/${stepNumber}`, !step.isCompleted);
        
        setCreatedTask(prev => prev ? {
          ...prev,
          executionPlan: prev.executionPlan.map(s => 
            s.stepNumber === stepNumber ? { ...s, isCompleted: !s.isCompleted } : s
          )
        } : null);
      }
    } catch (error) {
      console.error('Error updating step:', error);
    }
  };

  return (
    <div className="agent-content">
      <div className="agent-header">
        <h2>📋 Task Agent</h2>
        <p>Create tasks with AI-generated execution plans</p>
      </div>

      {message && (
        <div className={`message ${message.includes('Error') ? 'error' : 'success'}`}>
          {message}
        </div>
      )}

      <form onSubmit={handleCreateTask} className="task-form">
        <div className="form-group">
          <label htmlFor="title">Task Title:</label>
          <input
            type="text"
            id="title"
            value={taskForm.title}
            onChange={(e) => setTaskForm(prev => ({ ...prev, title: e.target.value }))}
            placeholder="What needs to be done?"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="description">Description:</label>
          <textarea
            id="description"
            value={taskForm.description}
            onChange={(e) => setTaskForm(prev => ({ ...prev, description: e.target.value }))}
            placeholder="Provide details about the task..."
            rows={4}
            required
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="dueDate">Due Date (optional):</label>
            <input
              type="date"
              id="dueDate"
              value={taskForm.dueDate}
              onChange={(e) => setTaskForm(prev => ({ ...prev, dueDate: e.target.value }))}
            />
          </div>

          <div className="form-group">
            <label htmlFor="priority">Priority:</label>
            <select
              id="priority"
              value={taskForm.priority}
              onChange={(e) => setTaskForm(prev => ({ ...prev, priority: e.target.value }))}
            >
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="tags">Tags (comma-separated):</label>
          <input
            type="text"
            id="tags"
            value={taskForm.tags}
            onChange={(e) => setTaskForm(prev => ({ ...prev, tags: e.target.value }))}
            placeholder="work, urgent, project-a"
          />
        </div>

        <div className="form-actions">
          <button type="submit" disabled={isCreating} className="create-button">
            {isCreating ? 'Creating...' : '🤖 Create Task & Generate Plan'}
          </button>
        </div>
      </form>

      {createdTask && (
        <div className="created-task">
          <h3>✅ Task Created: {createdTask.title}</h3>
          <div className="task-details">
            <p><strong>Description:</strong> {createdTask.description}</p>
            {createdTask.dueDate && (
              <p><strong>Due:</strong> {new Date(createdTask.dueDate).toLocaleDateString()}</p>
            )}
            <p><strong>Priority:</strong> {createdTask.priority}</p>
            {createdTask.tags.length > 0 && (
              <p><strong>Tags:</strong> {createdTask.tags.join(', ')}</p>
            )}
          </div>

          <div className="execution-plan">
            <h4>🎯 AI-Generated Execution Plan</h4>
            <div className="steps-list">
              {createdTask.executionPlan.map((step) => (
                <div key={step.stepNumber} className={`step-item ${step.isCompleted ? 'completed' : ''}`}>
                  <div className="step-header">
                    <input
                      type="checkbox"
                      checked={step.isCompleted}
                      onChange={() => toggleStepCompletion(step.stepNumber)}
                    />
                    <span className="step-number">Step {step.stepNumber}</span>
                    <span className="step-time">{step.estimatedTime}</span>
                  </div>
                  <p className="step-description">{step.description}</p>
                  {step.dependencies.length > 0 && (
                    <p className="step-dependencies">
                      <strong>Depends on:</strong> {step.dependencies.join(', ')}
                    </p>
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      <div className="agent-features">
        <h3>Features</h3>
        <ul>
          <li>🤖 AI-powered execution plan generation</li>
          <li>📊 Step-by-step task breakdown</li>
          <li>⏱️ Time estimation for each step</li>
          <li>🔗 Dependency tracking between steps</li>
          <li>✅ Progress tracking and completion</li>
        </ul>
      </div>
    </div>
  );
};

export default TaskAgent;