namespace NISCDR2OracleDB
{
    public interface ICDRModel
    {
        string Anumber { get; set; }
        string Bnumber { get; set; }
        decimal BuyAmount { get; set; }
        decimal BuyRate { get; set; }
        decimal CallId { get; set; }
        string Cnumber { get; set; }
        long DestPrefix { get; set; }
        ushort Duration { get; set; }
        decimal Margin { get; set; }
        decimal Markup { get; set; }
        ushort PartnerId { get; set; }
        decimal SellAmount { get; set; }
        decimal SellRate { get; set; }
        byte Sipiax { get; set; }
        string StartTime { get; set; }
        byte TerminateCauseId { get; set; }
        string Trunk { get; set; }
    }
}