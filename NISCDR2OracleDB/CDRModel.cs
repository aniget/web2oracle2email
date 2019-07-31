using System;

namespace NISCDR2OracleDB
{
    public class CDRModel : ICDRModel
    {
        public string StartTime { get; set; } //timestamp
        public string Anumber { get; set; }
        public string Bnumber { get; set; }
        public string Cnumber { get; set; }
        public long DestPrefix { get; set; }
        public decimal BuyRate { get; set; }
        public decimal SellRate { get; set; }
        public ushort Duration { get; set; } //from 0 to 65535
        public ushort PartnerId { get; set; }
        public string Trunk { get; set; }
        public byte TerminateCauseId { get; set; }
        public byte Sipiax { get; set; }
        public decimal BuyAmount { get; set; }
        public decimal SellAmount { get; set; }
        public decimal Margin { get; set; }
        public decimal Markup { get; set; }
        public decimal CallId { get; set; }

    }
}
