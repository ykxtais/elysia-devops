using ElysiaAPI.Application.DTOs.Hateoas;

namespace ElysiaAPI.Application.DTOs.Response
{
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Nome  { get; set; } = "";
        public string Email { get; set; } = "";
        public string Cpf   { get; set; } = "";
        public List<Link> Links { get; set; } = new(); 
    }
}