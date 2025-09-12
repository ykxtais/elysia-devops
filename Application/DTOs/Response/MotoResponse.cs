using ElysiaAPI.Application.DTOs.Hateoas;

namespace ElysiaAPI.Application.DTOs.Response
{
    public class MotoResponse
    {
        public int Id { get; set; }
        public string? Placa { get; set; }
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public int Ano { get; set; }
        public List<Link> Links { get; set; } = new(); 
    }
}