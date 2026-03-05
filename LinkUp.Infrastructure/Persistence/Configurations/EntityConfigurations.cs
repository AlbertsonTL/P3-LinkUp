using LinkUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUp.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Content).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.ImagePath).HasMaxLength(500);
        builder.Property(p => p.YouTubeUrl).HasMaxLength(500);
        builder.HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);
        builder.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(c => c.ParentComment).WithMany(c => c.Replies).HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.PostId, r.UserId }).IsUnique();
        builder.HasOne(r => r.Post).WithMany(p => p.Reactions).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User).WithMany(u => u.Reactions).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);
        // Match the Post global query filter so EF does not return reactions for soft-deleted posts
        builder.HasQueryFilter(r => !r.Post.IsDeleted);
    }
}

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.HasKey(f => f.Id);
        builder.HasOne(f => f.Sender).WithMany(u => u.SentRequests).HasForeignKey(f => f.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(f => f.Receiver).WithMany(u => u.ReceivedRequests).HasForeignKey(f => f.ReceiverId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasKey(f => f.Id);
        builder.HasOne(f => f.User1).WithMany(u => u.FriendshipsAsUser1).HasForeignKey(f => f.User1Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(f => f.User2).WithMany(u => u.FriendshipsAsUser2).HasForeignKey(f => f.User2Id).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BattleshipGameConfiguration : IEntityTypeConfiguration<BattleshipGame>
{
    public void Configure(EntityTypeBuilder<BattleshipGame> builder)
    {
        builder.HasKey(g => g.Id);
        builder.HasOne(g => g.Player1).WithMany(u => u.GamesAsPlayer1).HasForeignKey(g => g.Player1Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.Player2).WithMany(u => u.GamesAsPlayer2).HasForeignKey(g => g.Player2Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.Winner).WithMany().HasForeignKey(g => g.WinnerId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
    }
}

public class ShipPlacementConfiguration : IEntityTypeConfiguration<ShipPlacement>
{
    public void Configure(EntityTypeBuilder<ShipPlacement> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Game).WithMany(g => g.ShipPlacements).HasForeignKey(s => s.GameId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class AttackConfiguration : IEntityTypeConfiguration<Attack>
{
    public void Configure(EntityTypeBuilder<Attack> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasOne(a => a.Game).WithMany(g => g.Attacks).HasForeignKey(a => a.GameId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Attacker).WithMany().HasForeignKey(a => a.AttackerId).OnDelete(DeleteBehavior.NoAction);
    }
}
