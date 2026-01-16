using Garage_3._0.Models;

namespace Garage_3._0.Extensions
{
    public static class ApplicationUserExtensions
    {
        public static int GetAge(this ApplicationUser user)
        {
            int year = int.Parse(user.PersonalNumber[0..3]);
            int month = int.Parse(user.PersonalNumber[4..5]);
            int day = int.Parse(user.PersonalNumber[6..8]);
            DateTime date = new(year, month, day);

            return (int)((DateTime.Now - date).TotalDays % 365);
        }
    }
}
