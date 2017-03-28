namespace WPLovefilm.Models
{
    public class LFCatalogSearchMeta
    {
        public int TotalResults { get; set; }
        public int ItemsPerPage { get; set; }
        public int StartIndex { get; set; }
    }

    public enum LFCatalogSearchType
    {
        Search,
        Title,
        Film,
        TV,
        Games
    }

    public enum LFRefineType
    {
        Search,
        NewReleases,
        ComingSoon,
        MostPopular,
        Genres
    }

    public enum LFFormatType
    {
        Film,
        Game
    }
}
