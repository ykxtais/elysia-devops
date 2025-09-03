using System.ComponentModel.DataAnnotations;

namespace ElysiaAPI.Application.DTOs.Request
{
    public class VagaRequest
    {
        public string? Status { get; set; }
        [Range(1, int.MaxValue)] public int Numero { get; set; }
        [Required, MaxLength(50)] public string Patio { get; set; } = "";
    }
}