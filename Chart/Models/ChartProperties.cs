using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MIMSChart.Models
{
    public class ChartProperties
    {
        public IEnumerable<CntTimingForAction> CntTimingForActionList { get; set; }
        public IEnumerable<CntIsTimeExtended> CntIsTimeExtendedList { get; set; }
        public IEnumerable<CntIsTimeExtendedBool> CntIsTimeExtendedBoolList { get; set; }
        public IEnumerable<CntOfActorType> CntOfActorTypeList { get; set; }
        public IEnumerable<CntOfDateReceived> CntOfDateReceivedList { get; set; }
        public IEnumerable<CntOfDivision> CntOfDivisionList { get; set; }
        public IEnumerable<CntOfPortfolio> CntOfPortfolioList { get; set; }
        public IEnumerable<CntMinister> CntOfMinisterList { get; set; }
    }
}