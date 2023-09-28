using Framework.Domain.Interfaces.Entities;
using Microsoft.AspNetCore.Identity;

namespace Framework.AuthServer.Models;

public class User : IdentityUser, IUpdated, ICreated
{
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}