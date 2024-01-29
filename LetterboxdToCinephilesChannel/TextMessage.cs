namespace LetterboxdToCinephilesChannel
{
    internal class TextMessage
    {
        //Title cant be empty
        public string FilmTitle { get; set; }
        public string FilmYear { get; set; }
        //Rating need not be posted
        public string  MemberRating { get; set; } = string.Empty;
        //if rating is not posted then no need for total rating
        public string TotalRating { get; set; } = string.Empty;
        public string ImgSrc  { get; set; }
        //review is not required
        public string Review { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
    }
}
