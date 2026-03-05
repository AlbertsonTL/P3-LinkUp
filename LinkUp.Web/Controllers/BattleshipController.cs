using LinkUp.Application.Abstractions.Services;
using LinkUp.Domain.Entities;
using LinkUp.Domain.Enums;
using LinkUp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUp.Web.Controllers;

[Authorize]
public class BattleshipController : Controller
{
    private readonly IBattleshipService _battleshipService;
    private readonly UserManager<AppUser> _userManager;

    public BattleshipController(IBattleshipService battleshipService, UserManager<AppUser> userManager)
    {
        _battleshipService = battleshipService;
        _userManager = userManager;
    }

    private int GetUserId() => int.Parse(_userManager.GetUserId(User)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        await _battleshipService.CheckAbandonedGamesAsync();

        return View(new BattleshipIndexViewModel
        {
            ActiveGames = await _battleshipService.GetActiveGamesAsync(userId),
            FinishedGames = await _battleshipService.GetFinishedGamesAsync(userId),
            CurrentUserId = userId
        });
    }

    [HttpGet]
    public async Task<IActionResult> NewGame(string? search)
    {
        var userId = GetUserId();
        var friends = await _battleshipService.GetFriendsForNewGameAsync(userId);

        return View(new SelectFriendForGameViewModel
        {
            Friends = search == null ? friends : friends.Where(f => f.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)),
            Search = search
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGame(int friendId)
    {
        var userId = GetUserId();
        var result = await _battleshipService.CreateGameAsync(userId, friendId);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Index");
        }

        return RedirectToAction("SelectShip", new { gameId = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> EnterGame(int gameId)
    {
        var userId = GetUserId();

        // First check if the game is already finished (e.g. opponent surrendered)
        var attackBoard = await _battleshipService.GetAttackBoardAsync(gameId, userId);
        if (attackBoard.GameStatus == Domain.Enums.GameStatus.Finished)
            return RedirectToAction("AttackBoard", new { gameId });

        var board = await _battleshipService.GetPlacementBoardAsync(gameId, userId);

        // User still has ships to place
        if (board.RemainingShips.Any())
            return RedirectToAction("SelectShip", new { gameId });

        // User placed all ships but opponent hasn't
        if (!board.OtherPlayerReady)
            return RedirectToAction("Waiting", new { gameId });

        // Both ready -> attack phase
        return RedirectToAction("AttackBoard", new { gameId });
    }

    [HttpGet]
    public async Task<IActionResult> SelectShip(int gameId)
    {
        var userId = GetUserId();
        var board = await _battleshipService.GetPlacementBoardAsync(gameId, userId);

        if (!board.RemainingShips.Any())
        {
            if (!board.OtherPlayerReady)
                return RedirectToAction("Waiting", new { gameId });
            return RedirectToAction("AttackBoard", new { gameId });
        }

        return View(new SelectShipViewModel { GameId = gameId, RemainingShips = board.RemainingShips });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SelectShipPost(SelectShipViewModel model)
    {
        if (model.SelectedShipSize == null)
        {
            TempData["Error"] = "Debe seleccionar un barco.";
            return RedirectToAction("SelectShip", new { gameId = model.GameId });
        }

        return RedirectToAction("PlacementBoard", new { gameId = model.GameId, shipSize = model.SelectedShipSize });
    }

    [HttpGet]
    public async Task<IActionResult> PlacementBoard(int gameId, int shipSize)
    {
        var userId = GetUserId();
        var board = await _battleshipService.GetPlacementBoardAsync(gameId, userId);

        return View(new PlacementBoardViewModel
        {
            GameId = gameId,
            ShipSize = shipSize,
            PlacedCells = board.PlacedCells
        });
    }

    [HttpGet]
    public IActionResult SelectDirection(int gameId, int shipSize, int row, int col, string? error = null)
    {
        return View(new SelectDirectionViewModel
        {
            GameId = gameId,
            ShipSize = shipSize,
            StartRow = row,
            StartCol = col,
            ErrorMessage = error
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceShip(SelectDirectionViewModel model)
    {
        if (!ModelState.IsValid)
            return View("SelectDirection", model);

        var userId = GetUserId();
        var result = await _battleshipService.PlaceShipAsync(
            model.GameId, userId, model.ShipSize, model.StartRow, model.StartCol, model.Direction);

        if (!result.IsSuccess)
        {
            model.ErrorMessage = result.Error;
            return View("SelectDirection", model);
        }

        return RedirectToAction("SelectShip", new { gameId = model.GameId });
    }

    [HttpGet]
    public async Task<IActionResult> Waiting(int gameId)
    {
        var userId = GetUserId();
        var board = await _battleshipService.GetPlacementBoardAsync(gameId, userId);

        if (board.OtherPlayerReady)
            return RedirectToAction("AttackBoard", new { gameId });

        return View(new WaitingBoardViewModel { GameId = gameId, PlacedCells = board.PlacedCells });
    }

    [HttpGet]
    public async Task<IActionResult> AttackBoard(int gameId)
    {
        var userId = GetUserId();
        var board = await _battleshipService.GetAttackBoardAsync(gameId, userId);

        if (board.GameStatus == Domain.Enums.GameStatus.Finished)
            return RedirectToAction("GameResult", new { gameId });

        return View(new AttackBoardViewModel { GameId = gameId, Board = board });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attack(int gameId, int row, int col)
    {
        var userId = GetUserId();
        await _battleshipService.AttackAsync(gameId, userId, row, col);
        return RedirectToAction("AttackBoard", new { gameId });
    }

    [HttpGet]
    public async Task<IActionResult> MyBoard(int gameId)
    {
        var userId = GetUserId();
        var board = await _battleshipService.GetPlacementBoardAsync(gameId, userId);
        ViewBag.GameId = gameId;
        return View(board.PlacedCells);
    }

    [HttpGet]
    public IActionResult ConfirmSurrender(int gameId)
    {
        ViewBag.GameId = gameId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Surrender(int gameId)
    {
        var userId = GetUserId();
        await _battleshipService.SurrenderAsync(gameId, userId);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GameResult(int gameId)
    {
        var userId = GetUserId();
        var result = await _battleshipService.GetGameResultAsync(gameId, userId);

        return View(new GameResultViewModel { GameId = gameId, Result = result });
    }

    [HttpGet]
    public async Task<IActionResult> OpponentBoard(int gameId)
    {
        var userId = GetUserId();
        var result = await _battleshipService.GetGameResultAsync(gameId, userId);
        ViewBag.GameId = gameId;
        ViewBag.OpponentName = result.OpponentUserName;
        return View(result.OpponentAttacks);
    }
}
