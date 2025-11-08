using InvTracker.Models;

namespace InvTracker.Tests
{
    public class DataCreationMethods
    {
        public List<Account> CreateAccounts(string name1, string name2, string name3)
        {
            var account1 = new Account()
            {
                Id = 1,
                Name = name1
            };

            var account2 = new Account()
            {
                Id = 2,
                Name = name2
            };

            var account3 = new Account()
            {
                Id = 3,
                Name = name3
            };

            return new List<Account>()
            {
                account1, account2, account3
            };
        }

        //public List<Balance> CreateBalances()
        //{
        //    var balance1 = new Balance()
        //    {
        //        Id = 1,
        //        AccountId = 1,
        //        Year = 2022,
        //        StartingBalance = 10000,
        //        EndBalance = 12000,
        //        Month = 12,
        //        Day = 31
        //    };

        //    var balance2 = new Balance()
        //    {
        //        Id = 2,
        //        AccountId = 2,
        //        Year = 2022,
        //        StartingBalance = 15000,
        //        EndBalance = 17000,
        //        Month = 12,
        //        Day = 31
        //    };

        //    var balance3 = new Balance()
        //    {
        //        Id = 3,
        //        AccountId = 3,
        //        Year = 2022,
        //        StartingBalance = 25000,
        //        EndBalance = 37000,
        //        Month = 12,
        //        Day = 31
        //    };

        //    var balance4 = new Balance()
        //    {
        //        Id = 4,
        //        AccountId = 1,
        //        Year = 2023,
        //        StartingBalance = 12000,
        //        EndBalance = 12500,
        //        Month = 12,
        //        Day = 31
        //    };

        //    var balance5 = new Balance()
        //    {
        //        Id = 4,
        //        AccountId = 2,
        //        Year = 2023,
        //        StartingBalance = 17000,
        //        EndBalance = 17500,
        //        Month = 12,
        //        Day = 31
        //    };

        //    var balance6 = new Balance()
        //    {
        //        Id = 4,
        //        AccountId = 3,
        //        Year = 2023,
        //        StartingBalance = 37000,
        //        EndBalance = 28000,
        //        Month = 12,
        //        Day = 31
        //    };
        //}
    }
}
