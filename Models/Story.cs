namespace HackerNewsApi.Models
{
    public class Story
    {
        public string? PostedBy { get; set; } 
        public int Score { get; set; } 
        public string? Title { get; set; } 
        public string? Uri { get; set; } 
        public int CommentCount { get; set; }
        public string? Time { get; set; } 

       
        public static string? UnixTimeStampToDateTime(double unixTimeStamp)
        {
            try
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
            }
            catch
            {
                return null; 
            }
        }

    }
}
