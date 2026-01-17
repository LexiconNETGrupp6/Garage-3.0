using Garage_3._0.Models;

namespace Garage_3._0.Extensions
{
    public static class ApplicationUserExtensions
    {
        public static int GetAge(this ApplicationUser user)
        {
            string personalNumber = user.PersonalNumber;
            int year = int.Parse(personalNumber[..4]);
            int month = int.Parse(personalNumber[4..6]);
            int day = int.Parse(personalNumber[6..8]);
            DateTime date = new(year, month, day);

            return (int)((DateTime.Now - date).TotalDays / 365);
        }
    }
}
