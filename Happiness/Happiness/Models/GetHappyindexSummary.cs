using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    public class GetHappyindexSummary
    {
        public string EmotionName { get; set; }
        public int counts { get; set; }
    }

    public class SummaryLineChart
    {
        public DateTime Dates { get; set; }
        public string emotion { get; set; }
        public int count { get; set; }

    }
}