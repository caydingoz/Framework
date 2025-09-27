using Framework.AuthServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer
{
    public class AuthServerDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<UserToken> UserTokens => Set<UserToken>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationUser> NotificationUsers => Set<NotificationUser>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<ApplicantDocument> ApplicantDocuments => Set<ApplicantDocument>();
        public DbSet<Interview> Interviews => Set<Interview>();
        public DbSet<Scorecard> Scorecards => Set<Scorecard>();
        public AuthServerDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);

            builder.Entity<User>().HasMany(x => x.Roles).WithMany(x => x.Users);
            builder.Entity<Role>().OwnsMany(x => x.Permissions);

            builder.Entity<User>().HasMany(x => x.WorkItems).WithMany(x => x.Users);
            builder.Entity<User>().HasMany(x => x.Activities).WithOne(x => x.User);
            builder.Entity<Activity>().HasOne(x => x.WorkItem).WithMany(x => x.Activities);

            builder.Entity<User>().HasMany(x => x.NotificationUsers).WithOne(x => x.User);
            builder.Entity<Notification>().HasMany(x => x.NotificationUsers).WithOne(x => x.Notification);
            builder.Entity<Notification>().HasMany(x => x.NotificationRoles).WithOne(x => x.Notification);

            // HR entities configuration
            builder.Entity<Job>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Job>().HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById);
            builder.Entity<Job>().HasMany(x => x.Applicants).WithOne(x => x.Job).HasForeignKey(x => x.JobId);

            builder.Entity<Applicant>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Applicant>().HasOne(x => x.AssignedTo).WithMany().HasForeignKey(x => x.AssignedToId).IsRequired(false);
            builder.Entity<Applicant>().HasMany(x => x.Documents).WithOne(x => x.Applicant).HasForeignKey(x => x.ApplicantId);
            builder.Entity<Applicant>().HasMany(x => x.Interviews).WithOne(x => x.Applicant).HasForeignKey(x => x.ApplicantId);

            builder.Entity<ApplicantDocument>().HasOne(x => x.Applicant).WithMany(x => x.Documents).HasForeignKey(x => x.ApplicantId);

            builder.Entity<Interview>().HasOne(x => x.Interviewer).WithMany().HasForeignKey(x => x.InterviewerId);
            builder.Entity<Interview>().HasMany(x => x.Scorecards).WithOne(x => x.Interview).HasForeignKey(x => x.InterviewId);

            builder.Entity<Scorecard>().HasOne(x => x.Evaluator).WithMany().HasForeignKey(x => x.EvaluatorId);
        }
    }
}
