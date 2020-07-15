namespace Reimu.Common.Data.Parts
{
    public class ModCase
    {
        public ulong ModId { get; set; }
        public ulong UserId { get; set; }
        public string Reason { get; set; }
        public int CaseNumber { get; set; }
        public ulong MessageId { get; set; }
        public CaseType CaseType { get; set; }
    }
}
