namespace ElysiaAPI.Application.DTOs.Request
{
    public class UsuarioRequest
    {
        public string Nome  { get; set; } = "";
        public string Email { get; set; } = "";
        public string Senha { get; set; } = "";
        public string Cpf   { get; set; } = "";
    }
}