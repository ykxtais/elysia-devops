namespace ElysiaAPI.Application.DTOs.Hateoas

{
    public record Link(string Rel, string Href, string Method = "GET");
}
