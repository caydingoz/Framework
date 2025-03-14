﻿using Framework.AuthServer.Enums;
using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models
{
    public class Notification : Entity<int>
    {
        [StringLength(300)]
        public required string Title { get; set; }
        [StringLength(500)]
        public required string Message { get; set; }
        public NotificationTypes Type { get; set; }
        public ICollection<NotificationUser> NotificationUsers { get; set; }
        [StringLength(500)]
        public string? Url { get; set; }
    }
}
