using InvTracker.Models;
using Excel.FinancialFunctions;

namespace InvTracker.Utils
{
    public interface IXIRRCalculator
    {
        double CalculateXirr(MinimalBalance balance, IEnumerable<Payment> payments, int year);
    }
    public class XIRRCalculator : IXIRRCalculator
    {
        public double CalculateXirr(MinimalBalance balance, IEnumerable<Payment> payments, int year)
        {
            var input = new XirrInput(balance.StartingBalance, balance.EndBalance, payments, balance.Month, balance.Day);

            var cashFlows = new List<double>();

            cashFlows.Add(-input.StartingAmount);

            var dates = new List<DateTime>();

            var startDate = new DateTime(year, 1, 1);

            dates.Add(startDate);

            foreach (var payment in input.Payments)
            {
                cashFlows.Add(-payment.Amount);
                var date = new DateTime(year, payment.Date.Month, payment.Date.Day);
                dates.Add(date);
            }

            cashFlows.Add(input.EndAmount);

            var latestDate = new DateTime(year, input.EndMonth, input.EndDay);

            dates.Add(latestDate);

            var xirr = Financial.XIrr(cashFlows, dates);

            int daysElapsed = (latestDate - startDate).Days;

            var yearToDateReturn = Math.Pow(1 + xirr, daysElapsed / 365.0) - 1;

            return Math.Round(yearToDateReturn * 100, 2);
        }
    }
}
