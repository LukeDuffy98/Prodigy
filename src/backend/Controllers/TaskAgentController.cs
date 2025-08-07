using Microsoft.AspNetCore.Mvc;
using Prodigy.Backend.Models;

namespace Prodigy.Backend.Controllers;

/// <summary>
/// Task Agent controller for creating tasks and generating detailed execution plans.
/// All task descriptions and plans are personalized using the user's PersonalizationProfile.
/// </summary>
[ApiController]
[Route("api/agents/tasks")]
[Produces("application/json")]
public class TaskAgentController : ControllerBase
{
    private readonly ILogger<TaskAgentController> _logger;

    public TaskAgentController(ILogger<TaskAgentController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new task with AI-generated execution plan.
    /// Task planning uses the user's PersonalizationProfile for consistent language and style.
    /// </summary>
    /// <param name="request">Task details including title, description, and optional due date</param>
    /// <returns>Created task with detailed step-by-step execution plan</returns>
    /// <response code="201">Task created successfully with execution plan</response>
    /// <response code="400">Invalid task request data</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskWithPlan), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskWithPlan>> CreateTask([FromBody] CreateTaskRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Implement AI-powered execution plan generation
            // TODO: Apply user's PersonalizationProfile to task descriptions and steps
            // TODO: Forward request to Azure Function for plan generation

            var taskId = Guid.NewGuid().ToString();
            var sampleExecutionPlan = new ExecutionStep[]
            {
                new() {
                    StepNumber = 1,
                    Description = "Analyze and break down the task requirements",
                    EstimatedTime = "30 minutes",
                    Dependencies = Array.Empty<string>(),
                    IsCompleted = false
                },
                new() {
                    StepNumber = 2,
                    Description = "Gather necessary resources and information",
                    EstimatedTime = "45 minutes",
                    Dependencies = new[] { "Step 1" },
                    IsCompleted = false
                },
                new() {
                    StepNumber = 3,
                    Description = "Execute the main task activities",
                    EstimatedTime = "2 hours",
                    Dependencies = new[] { "Step 2" },
                    IsCompleted = false
                },
                new() {
                    StepNumber = 4,
                    Description = "Review and finalize deliverables",
                    EstimatedTime = "30 minutes",
                    Dependencies = new[] { "Step 3" },
                    IsCompleted = false
                }
            };

            var taskWithPlan = new TaskWithPlan
            {
                Id = taskId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Priority = request.Priority,
                Tags = request.Tags,
                ExecutionPlan = sampleExecutionPlan,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Created task with ID: {TaskId} and {StepCount} execution steps", taskId, sampleExecutionPlan.Length);
            
            return CreatedAtAction(nameof(GetTask), new { id = taskId }, taskWithPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    /// <summary>
    /// Retrieves a specific task by ID including its execution plan
    /// </summary>
    /// <param name="id">Task unique identifier</param>
    /// <returns>Task details with execution plan</returns>
    /// <response code="200">Task found and returned</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskWithPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskWithPlan>> GetTask(string id)
    {
        try
        {
            // TODO: Implement task retrieval from database
            _logger.LogInformation("Retrieving task with ID: {TaskId}", id);
            
            // For now, return not found as we don't have persistence yet
            return NotFound($"Task with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID: {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    /// <summary>
    /// Updates the completion status of a specific execution step
    /// </summary>
    /// <param name="taskId">Task unique identifier</param>
    /// <param name="stepNumber">Step number to update</param>
    /// <param name="isCompleted">Whether the step is completed</param>
    /// <returns>Updated task with execution plan</returns>
    /// <response code="200">Step updated successfully</response>
    /// <response code="404">Task or step not found</response>
    [HttpPatch("{taskId}/steps/{stepNumber}")]
    [ProducesResponseType(typeof(TaskWithPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskWithPlan>> UpdateExecutionStep(string taskId, int stepNumber, [FromBody] bool isCompleted)
    {
        try
        {
            // TODO: Implement step update logic with database persistence
            _logger.LogInformation("Updating step {StepNumber} for task {TaskId} to completed: {IsCompleted}", 
                stepNumber, taskId, isCompleted);
            
            return NotFound($"Task with ID {taskId} or step {stepNumber} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating execution step");
            return StatusCode(500, "An error occurred while updating the execution step");
        }
    }

    /// <summary>
    /// Regenerates the execution plan for an existing task using updated AI insights
    /// </summary>
    /// <param name="taskId">Task unique identifier</param>
    /// <returns>Task with newly generated execution plan</returns>
    /// <response code="200">Execution plan regenerated successfully</response>
    /// <response code="404">Task not found</response>
    [HttpPost("{taskId}/regenerate-plan")]
    [ProducesResponseType(typeof(TaskWithPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskWithPlan>> RegenerateExecutionPlan(string taskId)
    {
        try
        {
            // TODO: Implement plan regeneration with AI
            // TODO: Apply current user's PersonalizationProfile
            _logger.LogInformation("Regenerating execution plan for task: {TaskId}", taskId);
            
            return NotFound($"Task with ID {taskId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating execution plan for task: {TaskId}", taskId);
            return StatusCode(500, "An error occurred while regenerating the execution plan");
        }
    }
}