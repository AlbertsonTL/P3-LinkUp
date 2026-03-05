using System.ComponentModel.DataAnnotations;
using LinkUp.Application.DTOs.Response;
using LinkUp.Domain.Enums;

namespace LinkUp.Web.Models;

public class BattleshipIndexViewModel
{
    public IEnumerable<BattleshipGameResponseDto> ActiveGames { get; set; } = new List<BattleshipGameResponseDto>();
    public IEnumerable<BattleshipGameResponseDto> FinishedGames { get; set; } = new List<BattleshipGameResponseDto>();
    public int CurrentUserId { get; set; }
    public int TotalGames => FinishedGames.Count();
    public int GamesWon => FinishedGames.Count(g => g.WinnerId == CurrentUserId);
    public int GamesLost => FinishedGames.Count(g => g.WinnerId != CurrentUserId && g.WinnerId.HasValue);
}

public class SelectFriendForGameViewModel
{
    public IEnumerable<FriendResponseDto> Friends { get; set; } = new List<FriendResponseDto>();
    public int? SelectedFriendId { get; set; }
    public string? Search { get; set; }
}

public class SelectShipViewModel
{
    public int GameId { get; set; }
    public List<ShipRemainingDto> RemainingShips { get; set; } = new();
    public int? SelectedShipSize { get; set; }
}

public class PlacementBoardViewModel
{
    public int GameId { get; set; }
    public int ShipSize { get; set; }
    public List<int[]> PlacedCells { get; set; } = new();
}

public class SelectDirectionViewModel
{
    public int GameId { get; set; }
    public int ShipSize { get; set; }
    public int StartRow { get; set; }
    public int StartCol { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una dirección")]
    [Display(Name = "Dirección")]
    public ShipDirection Direction { get; set; } = ShipDirection.Down;

    public string? ErrorMessage { get; set; }
}

public class AttackBoardViewModel
{
    public int GameId { get; set; }
    public AttackBoardResponseDto Board { get; set; } = new();
}

public class WaitingBoardViewModel
{
    public int GameId { get; set; }
    public List<int[]> PlacedCells { get; set; } = new();
}

public class GameResultViewModel
{
    public int GameId { get; set; }
    public GameResultResponseDto Result { get; set; } = new();
    public bool ShowingMyBoard { get; set; } = true;
    public bool ShowingOpponentBoard { get; set; } = false;
    public bool ShowingMyPlacement { get; set; } = false;
}
