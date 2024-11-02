namespace Contractors.Utilities.Helpers
{
    public static class UsernameHelper
    {
        public static string CreateFullUserName(string nCode,string requestNumber)
        {
            return string.IsNullOrEmpty(requestNumber) ? nCode : $"{nCode}-{requestNumber}";
        }
    }
}
