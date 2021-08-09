using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MIMSChart.Models
{
    public class MimsLists
    {
        public List<MIMS> mimsList { get; set; }
    }
    
    public class MIMS
    {
        public DateTime? Created { get; set; }
        public string ActorType { get; set; }
        public DateTime? DateReceived { get; set; }
        public string AcknowledgedByPremiersOffice { get; set; }
        public string TimingForAction { get; set; }
        public DateTime? CorrespondenceDueDate { get; set; }
        public DateTime? CriticalDueDate { get; set; }
        public string ActionRequired { get; set; }
        public string PreparedReply { get; set; }
        public string Minister { get; set; }
        public string Portfolio { get; set; }
        public bool CMUApprovalRequired { get; set; }
        public DateTime? DateSentToExec { get; set; }
        public DateTime? DateReceivedFromExec { get; set; }
        public bool DivisionToMailCorro { get; set; }
        public DateTime? MailOutDate { get; set; }
        public bool ApproveOnBehalf { get; set; }
        public bool InformationNFA { get; set; }
        public bool PSURequired { get; set; }
        public string CorrespondenceType { get; set; }
        public DateTime? MRTDueDate { get; set; }
        public DateTime? DivisionDueDate { get; set; }
        public DateTime? DateSentToMO { get; set; }
        public DateTime? DateReceivedFromMO { get; set; }
        public DateTime? DateSentToPO { get; set; }
        public DateTime? DateReceivedFromPO { get; set; }
        public DateTime? DLOMailoutDate { get; set; }
        public string Division { get; set; }
        public string Branch { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsFasttracked { get; set; }
        public bool IsTimeExtended { get; set; }
        public bool IsCompleted { get; set; }

    }

     
   
}