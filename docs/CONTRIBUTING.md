# 🤝 Contributing Guide

Welcome to the Prodigy project! We're excited to have you contribute to our intelligent digital workspace. This guide will help you get started with contributing code, documentation, and ideas.

## 📋 Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contribution Types](#contribution-types)
- [Code Standards](#code-standards)
- [Pull Request Process](#pull-request-process)
- [Issue Guidelines](#issue-guidelines)
- [Community Guidelines](#community-guidelines)
- [Recognition](#recognition)

## 🚀 Getting Started

### Ways to Contribute
- **Code**: Bug fixes, new features, performance improvements
- **Documentation**: Guides, API documentation, tutorials
- **Testing**: Manual testing, automated tests, bug reports
- **Design**: UI/UX improvements, user experience enhancements
- **Ideas**: Feature requests, architectural suggestions

### Before You Start
1. **Read the Documentation**: Familiarize yourself with the project structure and architecture
2. **Check Existing Issues**: Look for existing issues or feature requests
3. **Join Discussions**: Participate in GitHub Discussions for questions and ideas
4. **Follow Code of Conduct**: Maintain respectful and inclusive interactions

## 🛠️ Development Setup

### Prerequisites
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **Git** - [Download](https://git-scm.com/)
- **Code Editor** - Visual Studio, VS Code, or your preferred editor

### Quick Setup
```bash
# 1. Fork the repository on GitHub
# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/Prodigy.git
cd Prodigy

# 3. Add upstream remote
git remote add upstream https://github.com/LukeDuffy98/Prodigy.git

# 4. Install dependencies
dotnet restore
cd src/frontend && npm install && cd ../..

# 5. Create environment configuration
cp .env.example .env
# Edit .env with your configuration (see Environment Setup guide)

# 6. Verify setup
dotnet build
cd src/frontend && npm run build && npm run lint
```

### Development Workflow
```bash
# Start development servers (use two terminals)
# Terminal 1 - Backend
cd src/backend && dotnet run

# Terminal 2 - Frontend  
cd src/frontend && npm run dev
```

### Verify Your Setup
- **Backend API**: http://localhost:5169/health
- **Swagger UI**: http://localhost:5169/swagger
- **Frontend**: http://localhost:5173

## 🎯 Contribution Types

### 🐛 Bug Fixes

**Process**:
1. **Create Issue**: Report the bug with reproduction steps
2. **Get Assignment**: Wait for maintainer assignment or ask to work on it
3. **Create Branch**: `git checkout -b fix/issue-number-description`
4. **Fix the Bug**: Make minimal changes to resolve the issue
5. **Test Thoroughly**: Verify the fix works and doesn't break other features
6. **Submit PR**: Follow the PR template and link to the issue

**Bug Fix Checklist**:
- [ ] Issue clearly describes the problem
- [ ] Fix addresses the root cause
- [ ] Solution is minimal and focused
- [ ] No new bugs introduced
- [ ] Code follows style guidelines

### ✨ New Features

**Process**:
1. **Discuss First**: Create a feature request issue or discussion
2. **Get Approval**: Wait for maintainer feedback before implementing
3. **Design Review**: For large features, share your design approach
4. **Implementation**: Follow the agreed-upon design
5. **Documentation**: Update relevant documentation
6. **Testing**: Ensure comprehensive testing

**Feature Development Checklist**:
- [ ] Feature request discussed and approved
- [ ] Implementation matches approved design
- [ ] API documentation updated
- [ ] User guide updated if needed
- [ ] Performance impact considered
- [ ] Security implications reviewed

### 📚 Documentation

**Areas for Contribution**:
- API documentation improvements
- User guide enhancements
- Developer documentation
- Code comments and examples
- README updates

**Documentation Standards**:
- Clear, concise writing
- Accurate and up-to-date information
- Code examples that work
- Proper cross-references
- Screenshots where helpful

### 🧪 Testing

**Testing Contributions**:
- Manual testing of new features
- Bug reproduction and verification
- Performance testing
- Security testing
- Usability testing

**Testing Guidelines**:
- Follow existing test patterns
- Include both positive and negative test cases
- Test edge cases and error conditions
- Verify cross-browser compatibility
- Test mobile responsiveness

## 📝 Code Standards

### Backend (.NET/C#)

#### Code Style
```csharp
// Use PascalCase for classes, methods, properties
public class EmailAgentController : ControllerBase
{
    private readonly IGraphEmailService _emailService;
    
    // Use camelCase for parameters and local variables
    public async Task<IActionResult> SendEmail(SendEmailRequest request)
    {
        // Use explicit types when clarity is important
        string recipientEmail = request.Recipients.FirstOrDefault();
        
        // Prefer async/await for I/O operations
        var result = await _emailService.SendEmailAsync(request);
        
        return Ok(result);
    }
}
```

#### Documentation Standards
```csharp
/// <summary>
/// Sends an email using Microsoft Graph API with AI personalization.
/// </summary>
/// <param name="request">Email details including recipients, subject, and body</param>
/// <returns>Response containing success status and message details</returns>
/// <example>
/// POST /api/agents/email/send
/// {
///   "recipients": ["user@example.com"],
///   "subject": "Meeting Follow-up",
///   "body": "Thanks for the great meeting today..."
/// }
/// </example>
[HttpPost("send")]
public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
```

#### Error Handling
```csharp
try
{
    var result = await _service.ProcessAsync(request);
    return Ok(result);
}
catch (ArgumentException ex)
{
    return BadRequest(new { error = ex.Message });
}
catch (UnauthorizedAccessException)
{
    return Unauthorized();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in {Method}", nameof(SendEmail));
    return StatusCode(500, new { error = "Internal server error" });
}
```

### Frontend (React/TypeScript)

#### Component Standards
```typescript
// Use TypeScript interfaces for props
interface EmailAgentProps {
  onEmailSent: (result: EmailResult) => void;
  defaultRecipients?: string[];
}

// Use functional components with hooks
const EmailAgent: React.FC<EmailAgentProps> = ({ 
  onEmailSent, 
  defaultRecipients = [] 
}) => {
  const [recipients, setRecipients] = useState<string[]>(defaultRecipients);
  const [subject, setSubject] = useState<string>('');
  const [body, setBody] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  
  // Custom hooks for API calls
  const { sendEmail } = useEmailApi();
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const result = await sendEmail({ recipients, subject, body });
      onEmailSent(result);
    } catch (error) {
      console.error('Failed to send email:', error);
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <form onSubmit={handleSubmit}>
      {/* Form implementation */}
    </form>
  );
};

export default EmailAgent;
```

#### API Integration Pattern
```typescript
// Custom hook for API calls
const useEmailApi = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const sendEmail = async (request: SendEmailRequest): Promise<EmailResult> => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await axios.post('/api/agents/email/send', request);
      return response.data;
    } catch (err) {
      const errorMessage = err.response?.data?.error || 'Failed to send email';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  };
  
  return { sendEmail, loading, error };
};
```

### Git Commit Standards

#### Commit Message Format
```bash
type(scope): brief description

Optional longer description explaining the change in more detail.

Closes #123
```

#### Commit Types
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, no logic changes)
- **refactor**: Code refactoring
- **test**: Adding or updating tests
- **chore**: Maintenance tasks

#### Examples
```bash
feat(email): add AI-powered email draft generation

- Integrate with personalization profile
- Add draft preview functionality
- Include confidence scoring for suggestions

Closes #45

fix(auth): resolve JWT token expiration handling

The frontend wasn't properly refreshing expired tokens,
causing authentication errors after extended sessions.

Closes #67

docs(api): update email agent endpoint documentation

- Add request/response examples
- Document error codes
- Include personalization profile usage

Co-authored-by: @username
```

## 🔄 Pull Request Process

### Before Creating a PR

```bash
# 1. Ensure your branch is up to date
git checkout main
git pull upstream main
git checkout your-feature-branch
git rebase main

# 2. Run all checks
dotnet build
cd src/frontend && npm run lint && npm run build

# 3. Test your changes
# Manual testing of your feature
# Verify existing functionality still works
```

### PR Creation

1. **Create Descriptive Title**:
   - `feat(email): add AI-powered email templates`
   - `fix(auth): resolve token refresh issue`
   - `docs(api): update calendar agent documentation`

2. **Use PR Template**:
   ```markdown
   ## Description
   Brief description of changes made.
   
   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Documentation update
   - [ ] Performance improvement
   
   ## Testing
   - [ ] Manual testing completed
   - [ ] Existing functionality verified
   - [ ] Edge cases tested
   
   ## Checklist
   - [ ] Code follows style guidelines
   - [ ] Self-review completed
   - [ ] Documentation updated
   - [ ] No breaking changes (or breaking changes documented)
   
   Closes #issue-number
   ```

3. **Link Related Issues**: Always reference related issues in the PR description

### PR Review Process

1. **Automated Checks**: All CI/CD checks must pass
2. **Code Review**: At least one maintainer review required
3. **Testing**: Manual testing may be requested
4. **Documentation**: Verify documentation is updated
5. **Approval**: Maintainer approval required before merge

### After PR Approval

```bash
# Delete feature branch after merge
git checkout main
git pull upstream main
git branch -d your-feature-branch
git push origin --delete your-feature-branch
```

## 🐛 Issue Guidelines

### Bug Reports

**Template**:
```markdown
## Bug Description
Clear and concise description of the bug.

## Steps to Reproduce
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

## Expected Behavior
Description of what you expected to happen.

## Actual Behavior
Description of what actually happened.

## Environment
- OS: [e.g. Windows 10, macOS 12, Ubuntu 20.04]
- Browser: [e.g. Chrome 96, Firefox 95, Safari 15]
- Version: [e.g. v1.2.3]

## Screenshots
If applicable, add screenshots to help explain the problem.

## Additional Context
Any other context about the problem.
```

### Feature Requests

**Template**:
```markdown
## Feature Summary
Brief, clear description of the feature you'd like to see.

## Problem Statement
What problem does this feature solve? Who would benefit?

## Proposed Solution
Detailed description of how you envision this working.

## Alternative Solutions
Other approaches you've considered.

## Additional Context
Any other context, mockups, or examples.
```

### Issue Labels

- **Type**: `bug`, `enhancement`, `documentation`, `question`
- **Priority**: `low`, `medium`, `high`, `critical`
- **Status**: `needs-investigation`, `in-progress`, `blocked`
- **Component**: `backend`, `frontend`, `docs`, `ci-cd`
- **Difficulty**: `good-first-issue`, `help-wanted`, `expert-needed`

## 🌟 Community Guidelines

### Code of Conduct

We are committed to providing a welcoming and inspiring community for all:

- **Be Respectful**: Treat everyone with respect and kindness
- **Be Inclusive**: Welcome people of all backgrounds and experience levels
- **Be Constructive**: Provide helpful, actionable feedback
- **Be Patient**: Help others learn and grow
- **Be Professional**: Maintain professional standards in all interactions

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions, ideas, and general discussion
- **Pull Requests**: Code review and collaboration
- **Documentation**: Written guides and API references

### Getting Help

1. **Check Documentation**: Start with the docs directory
2. **Search Issues**: Look for existing solutions
3. **Ask Questions**: Use GitHub Discussions for help
4. **Be Specific**: Provide clear details about your problem

## 🎉 Recognition

### Contributors

We recognize contributors in several ways:

- **Contributors File**: Listed in CONTRIBUTORS.md
- **Release Notes**: Mentioned in version release notes
- **GitHub Profile**: Contributions shown on your GitHub profile
- **Community Appreciation**: Recognition in project announcements

### Types of Recognition

- **Code Contributors**: Those who submit accepted pull requests
- **Documentation Contributors**: Those who improve project documentation
- **Community Contributors**: Those who help others and improve the community
- **Bug Reporters**: Those who identify and report issues

## 📚 Resources

### Learning Resources
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [React Documentation](https://reactjs.org/docs/)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)
- [Microsoft Graph Documentation](https://docs.microsoft.com/en-us/graph/)

### Project Documentation
- [Developer Guide](DEVELOPER_GUIDE.md) - Setup and development
- [Architecture Guide](ARCHITECTURE.md) - System design
- [API Reference](API_REFERENCE.md) - API documentation
- [User Guide](USER_GUIDE.md) - End-user documentation

### Development Tools
- [Visual Studio Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/) - API testing
- [GitHub CLI](https://cli.github.com/) - GitHub from command line
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/) - Azure services

---

## 🚀 Ready to Contribute?

1. **Start Small**: Look for `good-first-issue` labels
2. **Ask Questions**: Don't hesitate to ask for help
3. **Follow Guidelines**: Use this guide as your reference
4. **Have Fun**: Enjoy contributing to open source!

Thank you for contributing to Prodigy! Your efforts help make this project better for everyone. 🎉

---

*For questions about contributing, please open a GitHub Discussion or reach out to the maintainers.*