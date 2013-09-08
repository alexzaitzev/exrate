namespace Exrate.Infrastructure
{
    public sealed class Constants
    {
        public const string BANK_LOGO_PATH = "/images/bank_logos/";
    }

    public sealed class Currencies
    {
        public const string RUB = "RUB";
        public const string USD = "USD";
        public const string EUR = "EUR";
    }

    public sealed class Job
    {
        public const string City = "City";
        public const string Update = "UpdateJob";
        public const string RatesUpdGroup = "RatesUpdGroup";
        public const string KrskTrigger = "KrskTrigger";
        public const string CronEveryDayAt5 = "0 0 5 * * ?";
        public const string CronEveryDayAt5And11 = "0 0 5,11 * * ?";
    }

    public sealed class Banks
    {
        public const string AlfaBank = "AFB";
        public const string EniseyUnitedBank = "EUB";
        public const string AkBarsBank = "ABB";
        public const string Cberbank = "CBB";
        public const string Promsvyazbank = "PSB";
        public const string AsianPacificBank = "APB";
        public const string Legion = "LGN";
        public const string Rosbank = "RSB";
        public const string Levoberezhnii = "LBB";
        public const string ExpressBank = "EXB";
        public const string BankOfMoscow = "BMS";
    }
}