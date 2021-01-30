using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.CommentLikes;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.Conversations;
using Microsoft.Nnn.ApplicationCore.Entities.Messages;
using Microsoft.Nnn.ApplicationCore.Entities.ModeratorOperations;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Entities.PostCategories;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.PostTags;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Entities.ReplyLikes;
using Microsoft.Nnn.ApplicationCore.Entities.Suggesstions;
using Microsoft.Nnn.ApplicationCore.Entities.Users;

namespace Microsoft.Nnn.Infrastructure.Data
{
    //dotnet ef migrations add s_point --context NnnContext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj -o Data/Migrations
    public class NnnContext : DbContext
    {
        public NnnContext(DbContextOptions<NnnContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostVote> PostVotes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<ModeratorOperation> ModeratorOperations { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ReplyLike> ReplyLikes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityUser> CommunityUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PostTag>()
                .HasOne<Post>(sc => sc.Post)
                .WithMany(s => s.Tags)
                .HasForeignKey(sc => sc.PostId);
            
            builder.Entity<CommunityUser>()
                .HasOne<Community>(sc => sc.Community)
                .WithMany(s => s.Users)
                .HasForeignKey(sc => sc.CommunityId);
        }
        
    }
}
