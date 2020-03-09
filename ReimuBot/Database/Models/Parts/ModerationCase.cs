namespace Reimu.Database.Models.Parts
{
    public class ModerationCase
    {
        /// <summary>
        /// Case number, referenced for updating case reasons
        /// </summary>
        public int CaseNumber { get; set; }

        /// <summary>
        /// Id of logged message
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// Id of offending user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Id of acting moderator
        /// </summary>
        public ulong ModId { get; set; }
        
        public ModCaseType CaseType { get; set; }
        
        /// <summary>
        /// Offense user committed
        /// </summary>
        public string Reason { get; set; }
    }
}
