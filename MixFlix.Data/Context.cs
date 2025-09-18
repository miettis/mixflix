using Microsoft.EntityFrameworkCore;

namespace MixFlix.Data
{
    public class Context : DbContext
    {
        public DbSet<Service> Services { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentAvailability> ContentAvailabilities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }

        public Context(DbContextOptions options) : base(options)
        {

        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set singular table names explicitly
            builder.Entity<Content>().ToTable("content");
            builder.Entity<Content>()
                .HasMany(x => x.Categories)
                .WithMany(x => x.Contents)
                .UsingEntity<Dictionary<string, object>>(
                    "content_categories",
                    j => j
                        .HasOne<Category>()
                        .WithMany()
                        .HasForeignKey("category_id"),
                    j => j
                        .HasOne<Content>()
                        .WithMany()
                        .HasForeignKey("content_id"),
                    j =>
                    {
                        j.HasKey("content_id", "category_id");
                    }
                );

            builder.Entity<Category>().ToTable("category");

            builder.Entity<Service>().ToTable("service");

            builder.Entity<ContentAvailability>().ToTable("content_availability");

            builder.Entity<User>().ToTable("user");
            builder.Entity<User>()
                .HasMany(x => x.Groups)
                .WithOne(x => x.User);

            builder.Entity<Group>().ToTable("group");
            builder.Entity<Group>().HasOne(x => x.Creator);

            builder.Entity<Group>()
                .HasMany(x => x.Members)
                .WithOne(x => x.Group);

            builder.Entity<GroupMember>().ToTable("group_members");

            builder.Entity<UserRating>().ToTable("user_rating");

        }

        public static void ConfigureOptions(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }
}
