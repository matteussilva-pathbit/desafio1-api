namespace Application.DTOs.Auth;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // usados para criar o Customer e definir o tipo do usuário
    public string Name { get; set; } = string.Empty;        // nome do cliente
    public string Type { get; set; } = "CLIENTE";           // "ADMINISTRADOR" ou "CLIENTE"
}
