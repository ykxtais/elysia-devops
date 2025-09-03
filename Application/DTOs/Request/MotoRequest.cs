using System.ComponentModel.DataAnnotations;

namespace ElysiaAPI.Application.DTOs.Request
{
    public class MotoRequest
    {
        [Required] public string Placa { get; set; } = "";
        [Required, MaxLength(50)] public string Marca { get; set; } = "";
        [Required, MaxLength(50)] public string Modelo { get; set; } = "";
        [Range(1885, 2100)] public int Ano { get; set; }
    }
}