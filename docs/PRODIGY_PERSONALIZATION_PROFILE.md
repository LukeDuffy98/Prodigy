# Prodigy Personalization Profile – Copilot Implementation Guide

This document defines the **Personalization Profile** system for Prodigy's AI digital agents. Please use this specification as the authoritative source when designing, implementing, and documenting any functionality that generates user-facing content (emails, tasks, learning materials, quotes, etc.) in a way that matches individual user style and preferences.

---

## 1. Overview

**Goal:**  
Equip each Prodigy user with a configurable "Personalization Profile" that instructs AI agents on how to compose/generate text in the user's distinct voice. Output should reflect tone, vocabulary, stylistic choices, and professional persona as selected/defined by the user.

---

## 2. Key User Stories

- *As a user,* I want to configure how AI writes on my behalf: setting my preferred tone, greetings, closings, and style so that messages sound like me.
- *As a user,* I want to provide sample texts and "About Me" information for richer personalization.
- *As a user,* I want to save, review, and edit my Personalization Profile at any time.
- *As a developer/Copilot,* I want to ensure all AI-generated communication across agents leverages the user's profile.

---

## 3. Feature Specifications

### 3.1 Personalization Profile Properties

| Property             | Type              | Description                                                                             |
|----------------------|-------------------|-----------------------------------------------------------------------------------------|
| Tone                 | string            | e.g., "friendly", "formal", "concise", "enthusiastic"                                   |
| PreferredGreetings   | string[]          | e.g., ["Hi", "Hello", "Dear"]                                                          |
| SignatureClosings    | string[]          | e.g., ["Best regards", "Thanks"]                                                       |
| FavouritePhrases     | string[]          | e.g., ["Let's touch base", "Appreciate your input"]                                    |
| ProhibitedWords      | string[]          | e.g., ["ASAP", "Kindly"]                                                               |
| SampleTexts          | string[]          | Full example responses/messages submitted by the user                                   |
| AboutMe              | string            | Short user bio for context ("I'm an engineering manager who values direct, helpful talk")|
| CustomAgentHints     | Dictionary<string,string> | Per-agent specific guidelines (e.g., email: "always start with project name")       |

---

### 3.2 Data Model (DTO)

```csharp
/// <summary>
/// User's writing style and communication preferences for AI-generated content throughout Prodigy.
/// </summary>
public class PersonalizationProfile {
    public string Tone { get; set; }
    public string[] PreferredGreetings { get; set; }
    public string[] SignatureClosings { get; set; }
    public string[] FavouritePhrases { get; set; }
    public string[] ProhibitedWords { get; set; }
    public string[] SampleTexts { get; set; }
    public string AboutMe { get; set; }
    public Dictionary<string, string> CustomAgentHints { get; set; }
}
```
- **Copilot Guidance:** This type must be referenced in any AI-powered agent that generates user-facing prose.

---

### 3.3 API Contracts

#### 3.3.1 Save/Update Personalization Profile

`POST /api/user/personalization-profile`

```json
{
  "tone": "friendly",
  "preferredGreetings": ["Hi", "Hello"],
  "signatureClosings": ["Thanks!", "Best"],
  "favouritePhrases": ["Let's align", "Appreciate your feedback"],
  "prohibitedWords": ["Kindly"],
  "sampleTexts": [
    "Hi there, thanks for reaching out! Let me know how I can help."
  ],
  "aboutMe": "I'm a product manager who values clarity and collaboration.",
  "customAgentHints": {
    "quote": "Highlight pricing transparency",
    "learning": "Use bullet points and practical examples"
  }
}
```

#### 3.3.2 Retrieve Profile

`GET /api/user/personalization-profile`

- Returns `PersonalizationProfile`

---

### 3.4 AI Content Generation Contract

**All agent content generation requests (emails, tasks, quotes, etc) must include the appropriate PersonalizationProfile or reference the user profile.**

```csharp
public class AIGenerateContentRequest {
    public string ContextType { get; set; }            // e.g., "email", "task", "quote"
    public string ContextData { get; set; }            // Raw input to be transformed
    public PersonalizationProfile Profile { get; set; } // Enforced for all AI composition
}
```
- **Copilot Extension:** Suggest profile fields to AI prompts (e.g., "Compose friendly email – see SampleTexts").

---

## 4. UI/UX Guidelines

- Settings panel: Personalization Profile tab for user to edit all fields.
- Allow user to add/edit/remove greetings, closings, phrases, prohibited words.
- WYSIWYG input for sample message(s).
- In-context preview/testing: "See how your profile sounds in an example email/quote."
- Post-generation quick feedback: "Does this draft sound like you? [👍/👎] Adjust profile"

---

## 5. Integration Guidance for Copilot and Developers

- **ALWAYS** include the active PersonalizationProfile when invoking AI content generation, unless system/user explicitly disables personalization.
- **Document** in controller/service code—prefer XML/markdown doc comments stating:  
  "This function should use the user's PersonalizationProfile for all content generation. See /docs/PRODIGY_PERSONALIZATION_PROFILE.md."
- **Make all new agent modules support personalization** in both DTO and service layers.
- **Sample Payload/Response:** In comments, provide real-world before/after samples.

---

## 6. Example Usage

#### Example: AI Email Draft Generation

**Request:**
```json
{
  "contextType": "email",
  "contextData": "Reschedule tomorrow's meeting",
  "profile": {
      "tone": "friendly",
      "preferredGreetings": ["Hi"],
      "signatureClosings": ["Thanks!"],
      "favouritePhrases": ["Let's find a time that works for everyone"],
      "prohibitedWords": [],
      "sampleTexts": [ "Hi team, thanks for your flexibility. Let's work together to reschedule." ],
      "aboutMe": "I'm a team lead who values teamwork and open communication.",
      "customAgentHints": {}
  }
}
```
**AI Output:**
```
Hi Alex,

Thanks for your flexibility! Let's find a time that works for everyone and keep things moving.

Thanks!

[Your Name]
```

---

## 7. Directory Structure Example

```
/src/frontend/components/PersonalizationProfile/
  PersonalizationProfileEditor.tsx
  PersonalizationProfilePreview.tsx
/src/backend/Controllers/PersonalizationController.cs
/src/backend/Models/PersonalizationProfile.cs
/src/backend/Services/ContentGenerationService.cs
/docs/PRODIGY_PERSONALIZATION_PROFILE.md   # This file!
```

---

## 8. Documentation/Annotation Guidelines for Copilot

- When adding endpoints/DTOs for content generation, always annotate with:
  - Supported PersonalizationProfile fields
  - Realistic input/output samples
  - "See /docs/PRODIGY_PERSONALIZATION_PROFILE.md for full context."
- Use XML doc comments for DTOs and endpoints.
- Provide references in PRs/issues: "Implements Personalization Profile integration per /docs/PRODIGY_PERSONALIZATION_PROFILE.md"

---

## 9. Future Extensions (for Copilot to suggest)

- Voice print/emotion analysis for dynamic tone adjustment.
- Workspace-wide default profiles (organization/brand settings).
- Export/import profiles for rapid onboarding.

---

> **For questions or extensions, reference or PR against this file. Always ensure every user-facing AI message in Prodigy is personalized by default.**