namespace SubastaYa.Core.DTOs
{
    public class WalletResponseDto
    {
        public decimal TotalBalance { get; set; }
        public decimal RetainedBalance { get; set; }
        public decimal AvailableBalance { get; set; }

        public WalletResponseDto() { }

        public WalletResponseDto(decimal totalBalance, decimal retainedBalance, decimal availableBalance)
        {
            TotalBalance = totalBalance;
            RetainedBalance = retainedBalance;
            AvailableBalance = availableBalance;
        }
    }
}
