using Framework.AuthServer.Enums;
using Framework.Domain.Entites;
using Framework.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class User : Entity<Guid>, ILogicalDelete
{
    [StringLength(100)]
    public required string Email { get; set; }
    [StringLength(100)]
    public required string FirstName { get; set; }
    [StringLength(100)]
    public required string LastName { get; set; }
    public required string Password { get; set; }
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    [StringLength(50)]
    public string Title { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    [StringLength(50)]
    public string Image { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public double TotalAbsenceEntitlement { get; set; }
    public DateTime EmploymentDate { get; set; }
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Activity> Activities { get; set; } = [];
    public ICollection<WorkItem> WorkItems { get; set; } = [];
    public ICollection<Absence> Absences { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}