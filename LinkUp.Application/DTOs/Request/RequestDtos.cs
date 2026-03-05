namespace LinkUp.Application.DTOs.Request;

public class RegisterRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Web layer saves the file and passes the path here
    public string? ProfilePicturePath { get; set; }
}

public class LoginRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    // Raw bytes from Web layer
    public byte[]? ProfilePictureData { get; set; }
    public string? ProfilePictureFileName { get; set; }
}

public class CreatePostRequestDto
{
    public string Content { get; set; } = string.Empty;
    public string MediaTypeStr { get; set; } = "image";
    public byte[]? ImageData { get; set; }
    public string? ImageFileName { get; set; }
    public string? YouTubeUrl { get; set; }
}

public class UpdatePostRequestDto
{
    public string Content { get; set; } = string.Empty;
    public string? YouTubeUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageFileName { get; set; }
}
