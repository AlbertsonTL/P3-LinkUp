using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkUp.Infrastructure.Persistence;

public static class DataSeeder
{
    private const string SeedPassword = "Pass-1111";

    public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        // 1. USUARIOS
        var userSeeds = new[]
        {
            new AppUser { UserName = "pruebas",     Email = "pruebas@linkup.com",     FirstName = "Pruebas",   LastName = "LinkUp",           Phone = "809-000-0000", IsActive = true },
            new AppUser { UserName = "alb3rtsontl",  Email = "alb3rtsontl@gmail.com",  FirstName = "Albertson", LastName = "Terrero López",    Phone = "809-111-1111", IsActive = true },
            new AppUser { UserName = "maria.gomez",  Email = "maria.gomez@linkup.com", FirstName = "María",     LastName = "Gómez",            Phone = "809-222-2222", IsActive = true },
            new AppUser { UserName = "carlos.reyes",  Email = "carlos.reyes@linkup.com", FirstName = "Carlos",   LastName = "Reyes",            Phone = "809-333-3333", IsActive = true },
            new AppUser { UserName = "sofia.diaz",   Email = "sofia.diaz@linkup.com",  FirstName = "Sofía",     LastName = "Díaz",             Phone = "809-444-4444", IsActive = true },
            new AppUser { UserName = "juan.perez",   Email = "juan.perez@linkup.com",  FirstName = "Juan",      LastName = "Pérez",            Phone = "809-555-5555", IsActive = true },
        };

        foreach (var user in userSeeds)
        {
            var result = await userManager.CreateAsync(user, SeedPassword);
            if (!result.Succeeded)
                throw new Exception($"No se pudo crear el usuario {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Recuperamos referencias con Id ya asignado por la BD
        var pruebas = await userManager.FindByNameAsync("pruebas") ?? throw new Exception("pruebas no encontrado");
        var albertson = await userManager.FindByNameAsync("alb3rtsontl") ?? throw new Exception("alb3rtsontl no encontrado");
        var maria = await userManager.FindByNameAsync("maria.gomez") ?? throw new Exception("maria.gomez no encontrado");
        var carlos = await userManager.FindByNameAsync("carlos.reyes") ?? throw new Exception("carlos.reyes no encontrado");
        var sofia = await userManager.FindByNameAsync("sofia.diaz") ?? throw new Exception("sofia.diaz no encontrado");
        var juan = await userManager.FindByNameAsync("juan.perez") ?? throw new Exception("juan.perez no encontrado");

        // 2. AMISTADES (ya aceptadas)
        // Guardamos también la FriendRequest histórica (Accepted) para que la sección
        // de "solicitudes" tenga sentido si el usuario navega al historial.
        var friendPairs = new (AppUser A, AppUser B)[]
        {
            (pruebas, albertson),
            (pruebas, maria),
            (albertson, maria),
            (albertson, carlos),
            (maria, sofia),
            (carlos, sofia),
            (sofia, juan),
        };

        var now = DateTime.UtcNow;
        var friendRequests = new List<FriendRequest>();
        var friendships = new List<Friendship>();

        foreach (var (a, b) in friendPairs)
        {
            friendRequests.Add(new FriendRequest
            {
                SenderId = a.Id,
                ReceiverId = b.Id,
                Status = FriendRequestStatus.Accepted,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-9)
            });

            friendships.Add(new Friendship
            {
                User1Id = a.Id,
                User2Id = b.Id,
                CreatedAt = now.AddDays(-9)
            });
        }

        // Una solicitud pendiente extra, para que la pantalla de "Solicitudes" no esté vacía
        friendRequests.Add(new FriendRequest
        {
            SenderId = juan.Id,
            ReceiverId = pruebas.Id,
            Status = FriendRequestStatus.Pending,
            CreatedAt = now.AddHours(-3)
        });

        context.FriendRequests.AddRange(friendRequests);
        context.Friendships.AddRange(friendships);

        // 3. PUBLICACIONES
        var posts = new List<Post>
        {
            new Post
            {
                UserId = pruebas.Id,
                Content = "¡Bienvenidos a LinkUp! Este es mi primer post en la plataforma 🎉",
                MediaType = PostMediaType.Image,
                CreatedAt = now.AddDays(-5)
            },
            new Post
            {
                UserId = albertson.Id,
                Content = "Terminando el rediseño de la interfaz, ¿qué les parece el nuevo look?",
                MediaType = PostMediaType.Image,
                CreatedAt = now.AddDays(-4)
            },
            new Post
            {
                UserId = maria.Id,
                Content = "Alguien se anima a una partida de Battleship esta noche? 🚢",
                MediaType = PostMediaType.Image,
                CreatedAt = now.AddDays(-3)
            },
            new Post
            {
                UserId = carlos.Id,
                Content = "Mirando este video mientras tomo café ☕",
                MediaType = PostMediaType.Video,
                YouTubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                CreatedAt = now.AddDays(-2)
            },
            new Post
            {
                UserId = sofia.Id,
                Content = "Feliz de haber conectado con tantas personas nuevas por aquí 💙",
                MediaType = PostMediaType.Image,
                CreatedAt = now.AddDays(-1)
            },
            new Post
            {
                UserId = juan.Id,
                Content = "Buenas tardes LinkUp, ¿cómo va todo por acá?",
                MediaType = PostMediaType.Image,
                CreatedAt = now.AddHours(-6)
            },
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync(); // necesitamos los Id de los posts para comentarios/reacciones

        // 4. REACCIONES Y COMENTARIOS
        var reactions = new List<PostReaction>
        {
            new PostReaction { PostId = posts[0].Id, UserId = albertson.Id, ReactionType = ReactionType.Like, CreatedAt = now.AddDays(-5).AddMinutes(30) },
            new PostReaction { PostId = posts[0].Id, UserId = maria.Id,     ReactionType = ReactionType.Like, CreatedAt = now.AddDays(-5).AddMinutes(45) },
            new PostReaction { PostId = posts[1].Id, UserId = pruebas.Id,   ReactionType = ReactionType.Like, CreatedAt = now.AddDays(-4).AddHours(1) },
            new PostReaction { PostId = posts[2].Id, UserId = carlos.Id,    ReactionType = ReactionType.Like, CreatedAt = now.AddDays(-3).AddHours(2) },
            new PostReaction { PostId = posts[2].Id, UserId = sofia.Id,     ReactionType = ReactionType.Dislike, CreatedAt = now.AddDays(-3).AddHours(3) },
            new PostReaction { PostId = posts[4].Id, UserId = juan.Id,      ReactionType = ReactionType.Like, CreatedAt = now.AddDays(-1).AddHours(1) },
        };

        var comments = new List<Comment>
        {
            new Comment { PostId = posts[0].Id, UserId = albertson.Id, Content = "¡Bienvenido/a! Se ve genial la plataforma 👏", CreatedAt = now.AddDays(-5).AddMinutes(20) },
            new Comment { PostId = posts[1].Id, UserId = maria.Id,     Content = "Me encanta el nuevo diseño, mucho más moderno.", CreatedAt = now.AddDays(-4).AddHours(2) },
            new Comment { PostId = posts[2].Id, UserId = albertson.Id, Content = "¡Cuenta conmigo! Ya tengo mis barcos listos 😄", CreatedAt = now.AddDays(-3).AddHours(1) },
            new Comment { PostId = posts[5].Id, UserId = carlos.Id,    Content = "Todo bien por acá, ¡bienvenido de vuelta!", CreatedAt = now.AddHours(-5) },
        };

        context.PostReactions.AddRange(reactions);
        context.Comments.AddRange(comments);

        // 5. PARTIDA DE BATTLESHIP EN CURSO
        // pruebas vs albertson: ambos ya colocaron sus barcos (Ready = true),
        // el juego está InProgress y ya se intercambiaron algunos disparos.
        var game = new BattleshipGame
        {
            Player1Id = pruebas.Id,
            Player2Id = albertson.Id,
            Status = GameStatus.InProgress,
            Player1Ready = true,
            Player2Ready = true,
            CurrentTurnUserId = pruebas.Id,
            CreatedAt = now.AddMinutes(-40)
        };
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync(); // necesitamos el Id del juego

        // Flota de "pruebas" (Player1) — tablero 10x10, sin solaparse
        var player1Ships = new List<ShipPlacement>
        {
            new ShipPlacement { GameId = game.Id, UserId = pruebas.Id, ShipSize = 5, StartRow = 0, StartCol = 0, Direction = ShipDirection.Right },
            new ShipPlacement { GameId = game.Id, UserId = pruebas.Id, ShipSize = 4, StartRow = 2, StartCol = 1, Direction = ShipDirection.Right },
            new ShipPlacement { GameId = game.Id, UserId = pruebas.Id, ShipSize = 3, StartRow = 4, StartCol = 3, Direction = ShipDirection.Down },
            new ShipPlacement { GameId = game.Id, UserId = pruebas.Id, ShipSize = 3, StartRow = 6, StartCol = 6, Direction = ShipDirection.Right },
            new ShipPlacement { GameId = game.Id, UserId = pruebas.Id, ShipSize = 2, StartRow = 8, StartCol = 0, Direction = ShipDirection.Right },
        };

        // Flota de "albertson" (Player2)
        var player2Ships = new List<ShipPlacement>
        {
            new ShipPlacement { GameId = game.Id, UserId = albertson.Id, ShipSize = 5, StartRow = 1, StartCol = 2, Direction = ShipDirection.Down },
            new ShipPlacement { GameId = game.Id, UserId = albertson.Id, ShipSize = 4, StartRow = 0, StartCol = 5, Direction = ShipDirection.Right },
            new ShipPlacement { GameId = game.Id, UserId = albertson.Id, ShipSize = 3, StartRow = 5, StartCol = 5, Direction = ShipDirection.Right },
            new ShipPlacement { GameId = game.Id, UserId = albertson.Id, ShipSize = 3, StartRow = 7, StartCol = 1, Direction = ShipDirection.Down },
            new ShipPlacement { GameId = game.Id, UserId = albertson.Id, ShipSize = 2, StartRow = 9, StartCol = 7, Direction = ShipDirection.Right },
        };

        context.ShipPlacements.AddRange(player1Ships);
        context.ShipPlacements.AddRange(player2Ships);

        // Algunos disparos ya realizados por ambos lados (coherentes con las flotas de arriba)
        var attacks = new List<Attack>
        {
            // Disparos de "pruebas" contra el tablero de "albertson"
            new Attack { GameId = game.Id, AttackerId = pruebas.Id, Row = 1, Col = 2, IsHit = true,  AttackedAt = now.AddMinutes(-35) }, // toca el barco de tamaño 5
            new Attack { GameId = game.Id, AttackerId = pruebas.Id, Row = 0, Col = 5, IsHit = true,  AttackedAt = now.AddMinutes(-30) }, // toca el barco de tamaño 4
            new Attack { GameId = game.Id, AttackerId = pruebas.Id, Row = 3, Col = 3, IsHit = false, AttackedAt = now.AddMinutes(-25) }, // agua

            // Disparos de "albertson" contra el tablero de "pruebas"
            new Attack { GameId = game.Id, AttackerId = albertson.Id, Row = 0, Col = 0, IsHit = true,  AttackedAt = now.AddMinutes(-32) }, // toca el barco de tamaño 5
            new Attack { GameId = game.Id, AttackerId = albertson.Id, Row = 5, Col = 5, IsHit = false, AttackedAt = now.AddMinutes(-28) }, // agua
            new Attack { GameId = game.Id, AttackerId = albertson.Id, Row = 2, Col = 1, IsHit = true,  AttackedAt = now.AddMinutes(-20) }, // toca el barco de tamaño 4
        };

        context.Attacks.AddRange(attacks);

        // 6. UNA PARTIDA YA FINALIZADA (para el historial)
        var finishedGame = new BattleshipGame
        {
            Player1Id = maria.Id,
            Player2Id = carlos.Id,
            Status = GameStatus.Finished,
            Player1Ready = true,
            Player2Ready = true,
            WinnerId = maria.Id,
            FinishedAt = now.AddDays(-2),
            CreatedAt = now.AddDays(-2).AddHours(-1)
        };
        context.BattleshipGames.Add(finishedGame);

        await context.SaveChangesAsync();
    }
}