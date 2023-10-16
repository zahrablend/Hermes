using HermesChat.Data.Models;
using HermesChat.Data.Models.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesChat.Data.Context;

public class HermesChatContext : IdentityDbContext<User>
{
    public HermesChatContext(DbContextOptions<HermesChatContext> options)
        : base(options)
    {
    }

    public DbSet<Chat>? Chat { get; set; }
    public DbSet<ChatUser>? ChatUser { get; set; }
    public DbSet<Message>? Message { get; set; }
    public DbSet<Profile>? Profile { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
        builder.ApplyConfiguration(new RoleConfiguration());
    }
}

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Name).IsRequired();
    }
}