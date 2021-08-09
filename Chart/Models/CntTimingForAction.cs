using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MIMSChart.Models
{
    public class CntTimingForAction
    {
        public string TimeExtended { get; set; }
        public IEnumerable<bool> ActionType { get; set; }
        public int ActionSum { get; set; }
        public int sum { get; set; }
    }


    public class ActionTypes
    {
        public IEnumerable<string> ActType { get; set; }   
    }
}