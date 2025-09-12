namespace ElysiaAPI.Application.DTOs.Hateoas
{
    public class VagaResponse
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public int Numero { get; set; }
        public string Patio { get; set; } = "";

        public List<Link> Links { get; set; } = new(); 
    }
}