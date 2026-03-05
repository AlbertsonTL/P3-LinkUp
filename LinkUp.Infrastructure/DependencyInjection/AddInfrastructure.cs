using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Domain.Entities;
using LinkUp.Infrastructure.Email;
using LinkUp.Infrastructure.FileStorage;
using LinkUp.Infrastructure.Persistence;
using LinkUp.Infrastructure.Repositories;
using LinkUp.Infrastructure.Repositories.Base;
using LinkUp.Shared.Emails; // IEmailSender (servicio de correo - contrato en Shared)
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUp.Infrastructure.DependencyInjection;

public static class AddInfrastructure
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentity<AppUser, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IPostReactionRepository, PostReactionRepository>();
        services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IBattleshipGameRepository, BattleshipGameRepository>();
        services.AddScoped<IShipPlacementRepository, ShipPlacementRepository>();
        services.AddScoped<IAttackRepository, AttackRepository>();

        // Email
        services.Configure<EmailSenderOptions>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailSender, SmtpEmailService>();

        // File Storage
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
