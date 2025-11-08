using System.Collections;
using System.Text;
using System.Text.Json;
using InvTracker.Models;
using InvTracker.Services;
using InvTracker.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InvTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnsController : ControllerBase
    {
        private readonly IAccountsRepository _accountsRepo;
        private readonly IBalancesRepository _balancesRepo;
        private readonly IPaymentsRepository _paymentsRepo;
        private readonly IXIRRCalculator _xirrCalculator;


        public ReturnsController(IAccountsRepository accountsRepo, IBalancesRepository balancesRepo, IPaymentsRepository paymentsRepo, IXIRRCalculator xirrCalculator)
        {
            _accountsRepo = accountsRepo;
            _balancesRepo = balancesRepo;
            _paymentsRepo = paymentsRepo;
            _xirrCalculator = xirrCalculator;
        }

        // This is for the page that shows an account in one given year
        // This loads balance and payments data and XIRR and aggregate data like netdeposits
        [HttpGet("individualaccountdata/{accountId}/year/{year}")]
        public async Task<ActionResult<AccountYearlyDetail>> GetAccountYearlyDetails(int accountId, int year)
        {
            var balance = await _balancesRepo.GetBalanceByIdAndYear(accountId, year);

            if (balance is null)
            {
                return NotFound();
            }

            var payments = await _paymentsRepo.GetPaymentsByIdAndYear(accountId, year);

            var input = new XirrInput(balance.StartingBalance, balance.EndBalance, payments, balance.Month, balance.Day);

            var xirrReturn = _xirrCalculator.CalculateXirr(balance, payments, year);

            var accountYearlyDetail = new AccountYearlyDetail()
            {
                Balance = balance,
                Payments = payments,
                XIRR = xirrReturn
            };

            return Ok(accountYearlyDetail);
        }

        
        // This is for the home page
        // This loads everything for the home page...
        [HttpGet("accountreturns")]
        public async Task<ActionResult<HomePageData>> GetAccountReturns()
        {
            // GET ALL ACCOUNTS, BALANCES, PAYMENTS
            var years = Enumerable.Range(2021, DateTime.Today.Year - 2020);
            var accounts = await _accountsRepo.GetAccounts();
            var balances = await _balancesRepo.GetAllBalances();
            var payments = await _paymentsRepo.GetAllPayments();

            var homePageData = new HomePageData()
            {
                AggregateData = await GetAggregateData(years, accounts, balances, payments),
                IndividualAccountData = await GetIndividualAccountsData(years, accounts, balances, payments)
            };

            return Ok(homePageData);
        }

        [NonAction]
        public async Task<Dictionary<int, AggregateData>> GetAggregateData(IEnumerable<int> years, List<Account> accounts, List<Balance> balances, List<Payment> payments)
        {
            var balancesLookupByYear = await _balancesRepo.GetBalancesLookupByYearAsync();
            var paymentsLookupByYear = await _paymentsRepo.GetPaymentsLookupByYearAsync();
            // PROBABLY WANT TO SPLIT INTO TWO METHODS
            //  1 IS THE AGGREGATE ACCOUNT DATA SHOWN BELOW ON HOME PAGE

            var aggDataAllYears = new Dictionary<int, AggregateData>();

            foreach (var year in years)
            {
                var allBalancesForGivenYear = balancesLookupByYear[year];
                var startingBalanceForYear = allBalancesForGivenYear.Sum(x => x.StartingBalance);
                var endingBalanceForYear = allBalancesForGivenYear.Sum(x => x.EndBalance);

                // make an aggregate balance for that year
                var overallBalance = new MinimalBalance
                {
                    StartingBalance = startingBalanceForYear,
                    EndBalance = endingBalanceForYear,
                    Day = allBalancesForGivenYear.Min(x => x.Day),
                    Month = allBalancesForGivenYear.Min(x => x.Month),
                    Year = year
                };
                var allPaymentsByYear = paymentsLookupByYear[year];

                var aggDataEntry = new AggregateData()
                {
                    XIRR = _xirrCalculator.CalculateXirr(overallBalance, allPaymentsByYear, year),
                    NetDeposits = allPaymentsByYear.Sum(x => x.Amount),
                    TotalWithdrawals = allPaymentsByYear.Where(x => x.Amount < 0).Sum(x => x.Amount),
                    TotalDeposits = allPaymentsByYear.Where(x => x.Amount > 0).Sum(x => x.Amount),
                    StartBalance = startingBalanceForYear,
                    EndBalance = endingBalanceForYear
                };

                aggDataAllYears.Add(year, aggDataEntry);

            }
            return aggDataAllYears;
        }

        [NonAction]
        public async Task<Dictionary<int, AccountReturn>> GetIndividualAccountsData(IEnumerable<int> years, List<Account> accounts, List<Balance> balances, List<Payment> payments)
        {
            var individualAccountData = new Dictionary<int, AccountReturn>();

            var paymentsLookupByIdAndYear = await _paymentsRepo.GetPaymentsLookupByAccountIdAndYearAsync();
            var balancesLookupByIdAndYear = await _balancesRepo.GetBalancesLookupByIdAndYearAsync();


            var idArray = accounts.Select(x => x.Id).ToArray();
            foreach (var id in idArray)
            {
                var balanceAndXIRRByYear = new Dictionary<int, BalanceAndXIRR>();

                var accountName = accounts.FirstOrDefault(x => x.Id == id)?.Name;

                foreach (var year in years)
                {
                    var balanceByAccountAndYear = balancesLookupByIdAndYear[id][year];

                    if (!paymentsLookupByIdAndYear[id].TryGetValue(year, out var paymentsByAccountAndYear))
                    {
                        paymentsByAccountAndYear = new List<Payment>();
                    }

                    var xirr = _xirrCalculator.CalculateXirr(balanceByAccountAndYear, paymentsByAccountAndYear, year);
                    var balanceAndXIRR = new BalanceAndXIRR()
                    {
                        Year = year,
                        StartingBalance = balanceByAccountAndYear.StartingBalance,
                        EndBalance = balanceByAccountAndYear.EndBalance,
                        Month = balanceByAccountAndYear.Month,
                        Day = balanceByAccountAndYear.Day,
                        XIRR = xirr
                    };

                    balanceAndXIRRByYear.Add(year, balanceAndXIRR);
                }

                var accountReturn = new AccountReturn()
                {
                    AccountId = id,
                    AccountName = accountName,
                    BalancesAndReturnsByYear = balanceAndXIRRByYear
                };
                individualAccountData.Add(id, accountReturn);
            }
            return individualAccountData;
        }



        // This sends the returns to an excel file
        //[HttpGet("generateExcel")]
        //public async Task<ActionResult> SendReturnsToExcelFile()
        //{
        //    var filePath = "C:\\Users\\mattb\\Documents\\TestFiles";


        //    var fileName = $"InvestmentUpdate{DateTime.Now:HHmm-ddMMyyyy}.csv";
        //    var fullFileName = Path.Combine(filePath, fileName);

        //    using var writer = new StreamWriter(fullFileName, false, new UTF8Encoding(true));

        //    var accounts = await _accountsRepo.GetAccounts();

        //    var balances = await _balancesRepo.GetAllBalances();

        //    var payments = await _paymentsRepo.GetAllPayments();



        //    var accounts2 = await _accountsRepo.GetAccounts();

        //    // ***************

        //    writer.WriteLine("Account Name, Jan 2021, 2021, 2022, 2023, 2024, 2025, Cumulative Return");


        //    writer.WriteLine();

        //    foreach (var account in accounts2)
        //    {
        //        var row = new List<double>()
        //        {
        //            fullAccountReturn.AccountReturns[account.Id].Balances[2021].StartingBalance
        //        };

        //        var years = fullAccountReturn.AccountReturns[account.Id].Balances.Select(x => x.Key).ToList();

        //        foreach (var year in years)
        //        {
        //            row.Add(fullAccountReturn.AccountReturns[account.Id].Balances[year].EndBalance);
        //            row.Add(fullAccountReturn.AccountReturns[account.Id].YearlyReturns[year]);
        //        };

        //        var cumRet = fullAccountReturn.AccountReturns[account.Id].CumulativeReturn;

        //        writer.WriteLine($"{account.Name}, £{row[0]}, £{row[1]}, £{row[3]}, £{row[5]}, £{row[7]}, £{row[9]}");
        //        writer.WriteLine();
        //        writer.WriteLine($"XIRR, N/A, %{row[2]}, %{row[4]}, %{row[6]}, %{row[8]}, %{row[10]}, {cumRet}");
        //        writer.WriteLine();
        //    }

        //    var aggBalances = fullAccountReturn.AggregateBalances;
        //    var aggRet = fullAccountReturn.OverallReturns;

        //    writer.WriteLine($"Total, £{aggBalances[2021].StartBalance}, £{aggBalances[2021].EndBalance}, £{aggBalances[2022].EndBalance}, £{aggBalances[2023].EndBalance}, £{aggBalances[2024].EndBalance}, £{aggBalances[2025].EndBalance},");

        //    writer.WriteLine($"XIRR, N/A, %{aggRet[2021]}, %{aggRet[2022]}, %{aggRet[2023]}, %{aggRet[2024]}, %{aggRet[2025]},");

        //    // COULD THEN DO BALANCE PCT CHANGE AND NET DEPOSITS PERCENTAGE

        //    return Ok();
        //}
    }
}