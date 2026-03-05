namespace Archi.Library.Models
{
    public class ResourceQueryParams
    {
        public string? Name        { get; set; }
        public string? Type        { get; set; }
        public string? AnyStringField { get; set; }

        public string? Rating      { get; set; }
        public string? Price       { get; set; }

        public string? CreatedAt   { get; set; }

        public string? Sort        { get; set; }
        public string? Asc         { get; set; }
        public string? Desc        { get; set; }

        public string Range        { get; set; } = "0-25";

        public string? Fields      { get; set; }
    }
}
