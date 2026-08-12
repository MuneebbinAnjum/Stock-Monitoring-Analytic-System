namespace SMAS.API.DTOs
{
    public class UserGuideDto
    {
        public string Role { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FeatureGuideDto> Features { get; set; } = new();
    }

    public class FeatureGuideDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> SubFeatures { get; set; } = new();
    }
}
