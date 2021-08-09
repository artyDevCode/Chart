using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MIMSChart.Models
{
    public class CntIsTimeExtended
    {
        public string TimeExtended { get; set; }
        public IEnumerable<string> ActorType { get; set; }
        public int ActorSum { get; set; }
        public int sum { get; set; }
    }


    public class ActorTypes
    {
        public IEnumerable<string> ActType { get; set; }
    }
}