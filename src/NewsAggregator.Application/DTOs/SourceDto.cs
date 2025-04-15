namespace NewsAggregator.Application.DTOs
{
    public class SourceDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public SourceDto()
        {
                
        }

        public SourceDto(string? id, string? name)
        {
            Id = id;
            Name = name;
        }
    }
}
