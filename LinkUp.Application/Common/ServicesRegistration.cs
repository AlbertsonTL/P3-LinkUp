using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.Mappings;
using LinkUp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUp.Application.Common;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IFriendRequestService, FriendRequestService>();
        services.AddScoped<IBattleshipService, BattleshipService>();

        return services;
    }
}
