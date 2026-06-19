using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HTQL_DU_LICH.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Interest> Interests { get; set; }

        public DbSet<UserInterest> UserInterests { get; set; }

        public DbSet<TripGroup> TripGroups { get; set; }

        public DbSet<TripMember> TripMembers { get; set; }

        public DbSet<JoinRequest> JoinRequests { get; set; }

        public DbSet<TripInterest> TripInterests { get; set; }

        public DbSet<Expense> Expenses { get; set; }

        public DbSet<ExpenseSplit> ExpenseSplits { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Vendor> Vendors { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<ReviewReply> ReviewReplies { get; set; }

        public DbSet<ExpenseApproval> ExpenseApprovals { get ; set; }

        public DbSet<ExpenseExclusionRequest> ExpenseExclusionRequests { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }

        public DbSet<TripService> TripServices { get; set; }

        public DbSet<ServiceVote> ServiceVotes { get; set; }

        

        public DbSet<ServiceRequest>
            ServiceRequests { get; set; }

        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TripInterest>()
               .HasKey(x => new
               {
                    x.TripGroupId,
                      x.InterestId
               });
            builder.Entity<TripGroup>()
                .Property(x => x.Budget)
                .HasPrecision(18, 2);

            builder.Entity<TripMember>()
                .HasKey(x => new
               {
                    x.TripGroupId,
                    x.UserId
               });
            builder.Entity<UserInterest>()
                .HasKey(ui => new
                {
                    ui.UserId,
                    ui.InterestId
                });

            builder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany()
                .HasForeignKey(ui => ui.UserId);

            builder.Entity<UserInterest>()
                .HasOne(ui => ui.Interest)
                .WithMany()
                .HasForeignKey(ui => ui.InterestId);
        }
    }
}