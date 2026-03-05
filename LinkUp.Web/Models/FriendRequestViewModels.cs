using System.ComponentModel.DataAnnotations;
using LinkUp.Application.DTOs.Response;

namespace LinkUp.Web.Models;

// ViewModel para la pantalla principal de Solicitudes de Amistad
public class FriendRequestsIndexViewModel
{
    public IEnumerable<FriendRequestResponseDto> PendingRequests { get; set; } = new List<FriendRequestResponseDto>();
    public IEnumerable<FriendRequestResponseDto> SentRequests { get; set; } = new List<FriendRequestResponseDto>();
    public int PendingCount => PendingRequests.Count();
}

// ViewModel para la pantalla de nueva solicitud de amistad
public class NewFriendRequestViewModel
{
    public IEnumerable<UserResponseDto> AvailableUsers { get; set; } = new List<UserResponseDto>();
    public string? Search { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un usuario")]
    public int? SelectedUserId { get; set; }
}

// ViewModel para confirmar aceptar solicitud
public class ConfirmAcceptRequestViewModel
{
    public int RequestId { get; set; }
    public string SenderUserName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
}

// ViewModel para confirmar rechazar solicitud
public class ConfirmRejectRequestViewModel
{
    public int RequestId { get; set; }
    public string SenderUserName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
}

// ViewModel para confirmar eliminar solicitud enviada
public class ConfirmDeleteRequestViewModel
{
    public int RequestId { get; set; }
    public string ReceiverUserName { get; set; } = string.Empty;
    public string? ReceiverProfilePicture { get; set; }
}
