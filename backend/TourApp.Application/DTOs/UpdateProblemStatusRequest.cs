using TourApp.Domain;

namespace TourApp.Application.DTOs;

public class UpdateProblemStatusRequest
{
    public Guid ProblemId { get; set; }
    public ProblemStatus NewStatus { get; set; }
    public string? Comment { get; set; }
} 