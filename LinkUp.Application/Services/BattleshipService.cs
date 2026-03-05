using AutoMapper;
using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Application.DTOs.Response;
using LinkUp.Application.Results;
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using R = LinkUp.Application.Results.Result;

namespace LinkUp.Application.Services;

public class BattleshipService : IBattleshipService
{
    private readonly IBattleshipGameRepository _gameRepo;
    private readonly IShipPlacementRepository _shipRepo;
    private readonly IAttackRepository _attackRepo;
    private readonly IFriendshipRepository _friendshipRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    // Default ship config: sizes to place
    private static readonly int[] DefaultShipSizes = { 2, 3, 3, 4, 5 };
    private const int BoardSize = 12;

    public BattleshipService(IBattleshipGameRepository gameRepo, IShipPlacementRepository shipRepo,
        IAttackRepository attackRepo, IFriendshipRepository friendshipRepo,
        UserManager<AppUser> userManager, IMapper mapper)
    {
        _gameRepo = gameRepo;
        _shipRepo = shipRepo;
        _attackRepo = attackRepo;
        _friendshipRepo = friendshipRepo;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BattleshipGameResponseDto>> GetActiveGamesAsync(int userId)
    {
        var games = await _gameRepo.GetActiveGamesForUserAsync(userId);
        return games.Select(g => _mapper.Map<BattleshipGameResponseDto>(g));
    }

    public async Task<IEnumerable<BattleshipGameResponseDto>> GetFinishedGamesAsync(int userId)
    {
        var games = await _gameRepo.GetFinishedGamesForUserAsync(userId);
        return games.Select(g => _mapper.Map<BattleshipGameResponseDto>(g));
    }

    public async Task<IEnumerable<FriendResponseDto>> GetFriendsForNewGameAsync(int userId)
    {
        var friendIds = await _friendshipRepo.GetFriendIdsAsync(userId);
        var result = new List<FriendResponseDto>();

        foreach (var fId in friendIds)
        {
            var hasActiveGame = await _gameRepo.HasActiveGameWithFriendAsync(userId, fId);
            if (hasActiveGame) continue;

            var friend = await _userManager.FindByIdAsync(fId.ToString());
            if (friend == null || !friend.IsActive) continue;

            result.Add(_mapper.Map<FriendResponseDto>(friend));
        }

        return result;
    }

    public async Task<Result<int>> CreateGameAsync(int userId, int friendId)
    {
        var areFriends = await _friendshipRepo.AreFriendsAsync(userId, friendId);
        if (!areFriends) return Result<int>.Failure("No son amigos.");

        var hasActiveGame = await _gameRepo.HasActiveGameWithFriendAsync(userId, friendId);
        if (hasActiveGame) return Result<int>.Failure("Ya tienen una partida activa.");

        var game = new BattleshipGame
        {
            Player1Id = userId,
            Player2Id = friendId,
            Status = GameStatus.WaitingPlacement,
            CreatedAt = DateTime.UtcNow
        };

        await _gameRepo.AddAsync(game);
        return Result<int>.Success(game.Id);
    }

    public async Task<R> SurrenderAsync(int gameId, int userId)
    {
        var game = await _gameRepo.GetByIdAsync(gameId);
        if (game == null) return R.Failure("Partida no encontrada.");
        if (game.Player1Id != userId && game.Player2Id != userId) return R.Failure("No eres parte de esta partida.");

        var winnerId = game.Player1Id == userId ? game.Player2Id : game.Player1Id;
        game.Status = GameStatus.Finished;
        game.WinnerId = winnerId;
        game.FinishedAt = DateTime.UtcNow;

        await _gameRepo.UpdateAsync(game);
        return R.Success();
    }

    public async Task<PlacementBoardResponseDto> GetPlacementBoardAsync(int gameId, int userId)
    {
        var game = await _gameRepo.GetByIdAsync(gameId);
        var ships = await _shipRepo.GetUserShipsInGameAsync(gameId, userId);
        var placedCells = new List<int[]>();

        foreach (var ship in ships)
        {
            var cells = GetShipCells(ship.StartRow, ship.StartCol, ship.ShipSize, ship.Direction);
            placedCells.AddRange(cells);
        }

        var placedSizes = ships.Select(s => s.ShipSize).ToList();
        var remaining = new List<ShipRemainingDto>();
        var remainingCounts = new Dictionary<int, int>();

        foreach (var size in DefaultShipSizes)
        {
            if (!remainingCounts.ContainsKey(size)) remainingCounts[size] = 0;
            remainingCounts[size]++;
        }

        foreach (var size in placedSizes)
        {
            if (remainingCounts.ContainsKey(size)) remainingCounts[size]--;
        }

        foreach (var kvp in remainingCounts)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                remaining.Add(new ShipRemainingDto
                {
                    Size = kvp.Key,
                    Name = GetShipName(kvp.Key)
                });
            }
        }

        var opponentId = game!.Player1Id == userId ? game.Player2Id : game.Player1Id;
        var opponentReady = await _shipRepo.AreAllShipsPlacedAsync(gameId, opponentId);

        return new PlacementBoardResponseDto
        {
            GameId = gameId,
            UserId = userId,
            PlacedCells = placedCells,
            RemainingShips = remaining,
            OtherPlayerReady = opponentReady
        };
    }

    public async Task<R> PlaceShipAsync(int gameId, int userId, int shipSize, int startRow, int startCol, ShipDirection direction)
    {
        // Validate in bounds
        var cells = GetShipCells(startRow, startCol, shipSize, direction);
        if (cells.Any(c => c[0] < 0 || c[0] >= BoardSize || c[1] < 0 || c[1] >= BoardSize))
            return R.Failure("La combinación de celda y dirección coloca el barco fuera del tablero. Debes cambiar la celda o la dirección.");

        // Check overlap
        var existingShips = await _shipRepo.GetUserShipsInGameAsync(gameId, userId);
        var occupiedCells = new HashSet<string>();
        foreach (var ship in existingShips)
        {
            var sc = GetShipCells(ship.StartRow, ship.StartCol, ship.ShipSize, ship.Direction);
            foreach (var c in sc) occupiedCells.Add($"{c[0]},{c[1]}");
        }

        foreach (var c in cells)
        {
            if (occupiedCells.Contains($"{c[0]},{c[1]}"))
                return R.Failure("Debe cambiar la celda seleccionada o la dirección, ya que con la combinación actual el barco quedaría posicionado encima de otro barco.");
        }

        await _shipRepo.AddAsync(new ShipPlacement
        {
            GameId = gameId,
            UserId = userId,
            ShipSize = shipSize,
            StartRow = startRow,
            StartCol = startCol,
            Direction = direction
        });

        // Check if both players ready -> start game
        var game = await _gameRepo.GetByIdAsync(gameId);
        if (game != null)
        {
            var isP1 = game.Player1Id == userId;
            if (isP1) game.Player1Ready = await _shipRepo.AreAllShipsPlacedAsync(gameId, userId);
            else game.Player2Ready = await _shipRepo.AreAllShipsPlacedAsync(gameId, userId);

            if (game.Player1Ready && game.Player2Ready)
            {
                game.Status = GameStatus.InProgress;
                game.CurrentTurnUserId = game.Player1Id;
            }

            await _gameRepo.UpdateAsync(game);
        }

        return R.Success();
    }

    public async Task<AttackBoardResponseDto> GetAttackBoardAsync(int gameId, int userId)
    {
        var game = await _gameRepo.GetByIdAsync(gameId);
        if (game == null) return new AttackBoardResponseDto();

        var myAttacks = await _attackRepo.GetAttacksByGameAndAttackerAsync(gameId, userId);
        var currentTurnUser = game.CurrentTurnUserId.HasValue
            ? await _userManager.FindByIdAsync(game.CurrentTurnUserId.Value.ToString())
            : null;

        return new AttackBoardResponseDto
        {
            GameId = gameId,
            UserId = userId,
            IsMyTurn = game.CurrentTurnUserId == userId,
            CurrentTurnUserName = currentTurnUser?.UserName,
            AttackedCells = myAttacks.Select(a => new AttackCellDto { Row = a.Row, Col = a.Col, IsHit = a.IsHit }).ToList(),
            GameStatus = game.Status,
            WinnerId = game.WinnerId,
            WinnerUserName = game.Winner?.UserName
        };
    }

    public async Task<R> AttackAsync(int gameId, int userId, int row, int col)
    {
        var game = await _gameRepo.GetGameWithDetailsAsync(gameId);
        if (game == null) return R.Failure("Partida no encontrada.");
        if (game.Status != GameStatus.InProgress) return R.Failure("La partida no está en progreso.");
        if (game.CurrentTurnUserId != userId) return R.Failure("No es tu turno.");

        var alreadyAttacked = await _attackRepo.CellAlreadyAttackedAsync(gameId, userId, row, col);
        if (alreadyAttacked) return R.Failure("Esta celda ya fue atacada.");

        var opponentId = game.Player1Id == userId ? game.Player2Id : game.Player1Id;
        var opponentShips = await _shipRepo.GetUserShipsInGameAsync(gameId, opponentId);
        var opponentCells = new HashSet<string>();
        foreach (var ship in opponentShips)
        {
            var sc = GetShipCells(ship.StartRow, ship.StartCol, ship.ShipSize, ship.Direction);
            foreach (var c in sc) opponentCells.Add($"{c[0]},{c[1]}");
        }

        var isHit = opponentCells.Contains($"{row},{col}");

        await _attackRepo.AddAsync(new Attack
        {
            GameId = gameId,
            AttackerId = userId,
            Row = row,
            Col = col,
            IsHit = isHit,
            AttackedAt = DateTime.UtcNow
        });

        // Check if all opponent ships sunk
        var myAttacks = (await _attackRepo.GetAttacksByGameAndAttackerAsync(gameId, userId)).ToList();
        var hitCells = myAttacks.Where(a => a.IsHit).Select(a => $"{a.Row},{a.Col}").ToHashSet();
        var allSunk = opponentCells.All(c => hitCells.Contains(c));

        if (allSunk)
        {
            game.Status = GameStatus.Finished;
            game.WinnerId = userId;
            game.FinishedAt = DateTime.UtcNow;
        }
        else
        {
            game.CurrentTurnUserId = opponentId;
        }

        await _gameRepo.UpdateAsync(game);
        return R.Success();
    }

    public async Task<GameResultResponseDto> GetGameResultAsync(int gameId, int userId)
    {
        var game = await _gameRepo.GetGameWithDetailsAsync(gameId);
        if (game == null) return new GameResultResponseDto();

        var opponentId = game.Player1Id == userId ? game.Player2Id : game.Player1Id;
        var opponent = await _userManager.FindByIdAsync(opponentId.ToString());

        var myAttacks = await _attackRepo.GetAttacksByGameAndAttackerAsync(gameId, userId);
        var opponentAttacks = await _attackRepo.GetAttacksByGameAndAttackerAsync(gameId, opponentId);
        var myShips = await _shipRepo.GetUserShipsInGameAsync(gameId, userId);

        var myShipCells = new List<int[]>();
        foreach (var ship in myShips)
            myShipCells.AddRange(GetShipCells(ship.StartRow, ship.StartCol, ship.ShipSize, ship.Direction));

        return new GameResultResponseDto
        {
            GameId = gameId,
            RequestingUserId = userId,
            OpponentId = opponentId,
            OpponentUserName = opponent?.UserName ?? "Unknown",
            WinnerId = game.WinnerId,
            MyAttacks = myAttacks.Select(a => new AttackCellDto { Row = a.Row, Col = a.Col, IsHit = a.IsHit }).ToList(),
            OpponentAttacks = opponentAttacks.Select(a => new AttackCellDto { Row = a.Row, Col = a.Col, IsHit = a.IsHit }).ToList(),
            MyShipPlacements = myShipCells
        };
    }

    public async Task CheckAbandonedGamesAsync()
    {
        var cutoff = DateTime.UtcNow.AddHours(-48);
        var activeGames = await _gameRepo.FindAsync(g => g.Status == GameStatus.InProgress);

        foreach (var game in activeGames)
        {
            var allAttacks = (await _attackRepo.GetAllAttacksInGameAsync(game.Id)).ToList();
            var lastAttack = allAttacks.OrderByDescending(a => a.AttackedAt).FirstOrDefault();
            var lastActionTime = lastAttack?.AttackedAt ?? game.CreatedAt;

            if (lastActionTime < cutoff)
            {
                var winnerId = game.CurrentTurnUserId == game.Player1Id ? game.Player2Id : game.Player1Id;
                game.Status = GameStatus.Finished;
                game.WinnerId = winnerId;
                game.FinishedAt = DateTime.UtcNow;
                await _gameRepo.UpdateAsync(game);
            }
        }
    }

    private static List<int[]> GetShipCells(int startRow, int startCol, int size, ShipDirection direction)
    {
        var cells = new List<int[]>();
        for (int i = 0; i < size; i++)
        {
            var row = direction == ShipDirection.Up ? startRow - i :
                      direction == ShipDirection.Down ? startRow + i : startRow;
            var col = direction == ShipDirection.Left ? startCol - i :
                      direction == ShipDirection.Right ? startCol + i : startCol;
            cells.Add(new[] { row, col });
        }
        return cells;
    }

    private static string GetShipName(int size) => size switch
    {
        2 => "Lancha (2)",
        3 => "Destructor (3)",
        4 => "Acorazado (4)",
        5 => "Portaaviones (5)",
        _ => $"Barco ({size})"
    };
}
