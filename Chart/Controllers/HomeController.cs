using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
//using ModelEntity;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using MIMSChart.Models;

namespace MIMSChart.Controllers
{
    public class HomeController : Controller
    {
        // private CFSDB db = new CFSDB();
        static MimsLists mimsdataSelected;

        static ChartProperties ChartPropertiesList = new ChartProperties();
        // GET': Chart

        // [Route("home/index/{StartDate}/{EndDate}")]
        public ActionResult Index(DateTime StartDate, DateTime EndDate)
        {

            //  var result = getChartData(DateTime.Now, DateTime.Now.Date.AddDays(-31));  //test what is returned with result



            /* public ActionResult Chart(string chartName, string ChartTitle)
             {
                 var chart = buildChart(chartName);
                 StringBuilder result = new StringBuilder();
                 result.Append(getChartImage(chart));
                 result.Append(chart.GetHtmlImageMap("ImageMap"));
                 return Content(result.ToString());
             }*/


            mimsdataSelected = seedData();
            var mimsdata = mimsdataSelected.mimsList.Where(tp => tp.Created >= StartDate && tp.Created <= EndDate)
                .Where(t => t.InformationNFA == false);

            ChartPropertiesList.CntOfActorTypeList = mimsdata
                  .GroupBy(e => e.ActorType)
                  .Select(r => new CntOfActorType
                  {
                      Actor = r.First().ActorType,
                      sum = r.Count(),
                  }); //.ToList();
            //ChartPropertiesList.Add(new ChartProperties("ActorList", "Count by Groups"));

            ChartPropertiesList.CntOfMinisterList = mimsdata
                 .GroupBy(e => e.Minister)
                 .Select(r => new CntMinister
                 {
                     MinisterName = r.First().Minister,
                     sum = r.Count()
                 }); //.ToList();

            ChartPropertiesList.CntIsTimeExtendedBoolList = mimsdata
                .GroupBy(e => e.IsTimeExtended)
                .Select(r => new CntIsTimeExtendedBool
                {
                    timeExtendedBool = r.First().IsTimeExtended.ToString(),
                    resultbool = r.Count()
                }); //.ToList();

            ChartPropertiesList.CntOfDateReceivedList = mimsdata
              .GroupBy(e => e.DateReceived)
              .Select(r => new CntOfDateReceived
              {
                  DateReceived = r.First().DateReceived.ToString(),
                  sum = r.Count()
              }); //.ToList();

            ChartPropertiesList.CntIsTimeExtendedList = mimsdata
                //.Where(a => a.IsCompleted == true)
                .Where(b => b.ActorType == "DLO" || b.ActorType == "MRT")
                .GroupBy(e => e.TimingForAction)
                .Select(r => new CntIsTimeExtended
                {
                    TimeExtended = r.First().TimingForAction.ToString(),
                    ActorType = r.Select(b => b.ActorType),
                    ActorSum = r.Select(b => b.ActorType).Distinct().Count(),
                    sum = r.Count()
                }); //.ToList();


            ChartPropertiesList.CntTimingForActionList = mimsdata
                .GroupBy(e => e.TimingForAction)
                .Select(r => new CntTimingForAction
                {
                    TimeExtended = r.First().TimingForAction.ToString(),
                    ActionType = r.Select(b => b.IsTimeExtended),
                    ActionSum = r.Select(b => b.IsTimeExtended).Distinct().Count(),
                    sum = r.Count()
                }); //.ToList();

            ChartPropertiesList.CntOfDivisionList = mimsdata
            .GroupBy(e => e.Division)
            .Select(r => new CntOfDivision
            {
                Divisions = r.First().Division.ToString(),
                sum = r.Count()
            }); //.ToList();


            ChartPropertiesList.CntOfPortfolioList = mimsdata
            .GroupBy(e => e.Portfolio)
            .Select(r => new CntOfPortfolio
            {
                Portfolios = r.First().Portfolio.ToString(),
                sum = r.Count()
            }); //.ToList();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_HomeCharts");
            }
            else
            {
                ViewBag.startdate = DateTime.Now.ToShortDateString();
                ViewBag.enddate = DateTime.Now.Date.AddDays(-31).ToShortDateString();
                return View();
            }
        }

        public ActionResult Chart(string chartName, string chartTitle)
        {
            // Build Chart
            var chart = new Chart();

            chart.Width = 500;
            chart.Height = 300;

            // Create chart here
            chart.Titles.Add(CreateTitle(chartTitle));
            chart.Legends.Add(CreateLegend());
            chart.ChartAreas.Add(CreateChartArea());
            //chart.Series.Add(CreateSeries(chartType, chartName));
            //chart.Series.Add(GetData(chartName));
            chart = GetData(chartName, chart);
            StringBuilder result = new StringBuilder();
            result.Append(getChartImage(chart));
            result.Append(chart.GetHtmlImageMap("ImageMap"));
            return Content(result.ToString());
        }

        private string getChartImage(Chart chart)
        {
            using (var stream = new MemoryStream())
            {
                string img = "<img src='data:image/png;base64,{0}' alt='' usemap='#ImageMap'>";
                chart.SaveImage(stream, ChartImageFormat.Png);
                string encoded = Convert.ToBase64String(stream.ToArray());
                return String.Format(img, encoded);
            }
        }

        private Title CreateTitle(string ChartTitle)
        {
            Title title = new Title();
            title.Text = ChartTitle;
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);
            return title;
        }

        private Legend CreateLegend()
        {
            Legend legend = new Legend();
            legend.Enabled = true;
            legend.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            legend.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            legend.ShadowOffset = 3;
            legend.ForeColor = Color.FromArgb(26, 59, 105);
            // legend.Title = "Legend";
            return legend;
        }

        private ChartArea CreateChartArea()
        {
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "Result Chart";
            chartArea.BackColor = Color.Transparent;
            chartArea.AxisX.IsLabelAutoFit = false;
            chartArea.AxisY.IsLabelAutoFit = false;
            chartArea.AxisX.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.Interval = 1;
            chartArea.Area3DStyle.Enable3D = false;
            return chartArea;
        }



        // private Series GetData(string chartName)
        private Chart GetData(string chartName, Chart chart)
        {

            if (chartName == "ActorList")
            {
                string pByGroup = "pByGroup";
                chart.Series.Add(pByGroup);

                //for (int i = 0; i < ChartPropertiesList.CntOfActorTypeList.Count(); i++)
                foreach (var i in ChartPropertiesList.CntOfActorTypeList)
                {
                    var p = chart.Series[pByGroup].Points.Add(i.sum);
                    p.Label = i.Actor.ToString(); // ChartPropertiesList.CntOfActorTypeList.ElementAt(i).sum.ToString(); // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = "onClick=\"alert('value: #VAL, series: #SER');\"";
                    // p.MapAreaAttributes = "onMouseOver=\"$('#tooltip').show();\"";
                 
                    p.AxisLabel = i.Actor.ToString();
                    p.LegendText = i.Actor.ToString();
                    p["BarLabelStyle"] = "Center";
                }
                //  chart.Series[pByGroup].Name = "Percent by Group";
                //   chart.Series[pByGroup].IsValueShownAsLabel = false;
                chart.Series[pByGroup].Color = Color.FromArgb(198, 99, 99);
                chart.Series[pByGroup].ChartType = SeriesChartType.Pie;
            }




            if (chartName == "CntOfDateReceivedList")
            {
                if (ChartPropertiesList.CntOfDateReceivedList.Count() > 0)
                {
                    string DateReceived = "CntOfDateReceivedList";

                    chart.Series.Add(DateReceived);
                    chart.Series[DateReceived].ChartType = SeriesChartType.Line;
                    chart.Series[DateReceived].LegendText = "Total";
                    chart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
                    //for (int i = 0; i < ChartPropertiesList.CntOfDateReceivedList.Count(); i++)
                    foreach (var i in ChartPropertiesList.CntOfDateReceivedList)
                    {
                        if (i.DateReceived != "")
                        {
                            var p = chart.Series[DateReceived].Points.Add(i.sum);
                            // p.Label = ChartPropertiesList.CntOfDateReceivedList.ElementAt(i).DateReceived.ToString() + " " + ChartPropertiesList.CntOfDateReceivedList.ElementAt(i).sum; // String.Format("Point {0}", i);
                            p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                            p.AxisLabel = i.DateReceived.ToString();
                            p.Font = new Font("Calibri", 15, FontStyle.Regular);
                            p["BarLabelStyle"] = "Center";
                            p["PixelPointWidth"] = "10";
                            // p.LabelAngle = 45;        
                        }
                    }
                }
            }
        
            if (chartName == "MinisterList")
            {
                int index = 0;
                string ministers = "";
                //for (int i = 0; i < ChartPropertiesList.CntOfMinisterList.Count(); i++)
                if (ChartPropertiesList.CntOfMinisterList.Count() > 0)
                {
                    foreach (var i in ChartPropertiesList.CntOfMinisterList)
                    {
                        ministers = "minister" + index;
                        chart.Series.Add(ministers);
                        chart.Series[ministers].LegendText = i.MinisterName.ToString();
                        chart.Series[ministers].Font = new Font("Calibri", 5, FontStyle.Regular);
                        chart.Series[ministers].Points.Add(i.sum);
                        //  p.Label = ChartPropertiesList.CntOfMinisterList.ElementAt(i).sum.ToString(); // String.Format("Point {0}", i);
                        chart.Series[ministers].LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                        chart.Series[ministers].LegendText = i.MinisterName.ToString();
                        chart.Series[ministers]["PixelPointWidth"] = "200";
                        index++;
                    }
                    //  chart.Series[ministers].IsValueShownAsLabel = false;
                    chart.Series[ministers].Color = Color.FromArgb(198, 99, 99);
                    chart.Series[ministers].ChartType = SeriesChartType.Column;
                    chart.Series[ministers].AxisLabel = "Total";
                    chart.Series[ministers].IsXValueIndexed = false;
                    chart.Series[ministers]["BarLabelStyle"] = "Center";
                }

            }


            if (chartName == "TimeExtendedBoolList")
            {
                string extendedBool = "TimeExtendedBoolList";

                chart.Series.Add(extendedBool);
                chart.Series[extendedBool].LegendText = "Total";
                //for (int i = 0; i < ChartPropertiesList.CntIsTimeExtendedBoolList.Count(); i++)

                foreach (var i in ChartPropertiesList.CntIsTimeExtendedBoolList)
                {
                    
                        //  extendedBool = ChartPropertiesList.CntIsTimeExtendedBoolList.ElementAt(i).timeExtendedBool.ToLower();
                        //  chart.Series.Add(extendedBool);  

                        var p = chart.Series[extendedBool].Points.Add(i.resultbool);
                        //   chart.Series[extendedBool].Points[ChartPropertiesList.CntIsTimeExtendedBoolList.Count()].Color = System.Drawing.Color.Green; //ChartPropertiesList.CntIsTimeExtendedBoolList.ElementAt(i).timeExtendedBool.ToLower() == "false" ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                        p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");

                        p.AxisLabel = i.timeExtendedBool.ToLower() == "false" ? "No" : "Yes";
                        p.Font = new Font("Calibri", 5, FontStyle.Regular);
                        p["PixelPointWidth"] = "200";
                        //  p.Name = "Negotiated Timeframes";
                        p["BarLabelStyle"] = "Center";

                    
                }

            }


            if (chartName == "CntTimingForActionList")
            {
                if (ChartPropertiesList.CntTimingForActionList.Count() > 0)
                {
                    chart.Series.Add("Yes");
                    chart.Series.Add("No");
                    var seriesNames = "";
                    //  chart.Series["Yes"].IsValueShownAsLabel = false;
                    chart.Series["Yes"].Color = Color.FromArgb(198, 99, 99);
                    // chart.Series["Yes"].IsXValueIndexed = true;
                    // chart.Series["Yes"]["BarLabelStyle"] = "Center";

                    foreach (var i in ChartPropertiesList.CntTimingForActionList)
                    {

                        if (i.TimeExtended == "General 10 Days" || i.TimeExtended == "General 5 Days" || i.TimeExtended == "Other" || i.TimeExtended == "VIP 3 Days")
                        {
                            foreach (var a in i.ActionType.Distinct())
                            {
                                seriesNames = a.Equals(true) ? "Yes" : "No";
                                var ss = i.ActionType.Count(m => m.Equals(a));

                                var p = chart.Series[seriesNames].Points.Add(ss);
                                p.LegendText = seriesNames;
                                p.AxisLabel = i.TimeExtended;
                                chart.Series[seriesNames].ChartType = SeriesChartType.Bar;
                                // chart.Series["Yes"]["PointWidth"] = "1";
                                // p["PixelPointWidth"] = "50";
                            }
                        }

                    }
                }

            }

            if (chartName == "CntIsTimeExtendedList")
            {
                if (ChartPropertiesList.CntIsTimeExtendedList.Count() > 0)
                {
                    chart.Series.Add("DLO");
                    chart.Series.Add("MRT");
                    var seriesNames = "";                   
                    chart.Series["DLO"].Color = Color.FromArgb(198, 99, 99);
                  
                    chart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
                    foreach (var i in ChartPropertiesList.CntIsTimeExtendedList)
                    {

                        if (i.TimeExtended == "General 10 Days" || i.TimeExtended == "General 5 Days" || i.TimeExtended == "Other" || i.TimeExtended == "VIP 3 Days")
                        {
                            foreach (var a in i.ActorType.Distinct())
                            {
                                if (a.Contains("DLO") || a.Contains("MRT"))
                                {
                                    seriesNames = a.Contains("DLO") ? "DLO" : "MRT";
                                    var ss = i.ActorType.Count(m => m.Contains(a));

                                    var p = chart.Series[seriesNames].Points.Add(ss);
                                    p.LegendText = seriesNames;
                                    p.AxisLabel = i.TimeExtended;
                                    chart.Series[seriesNames].ChartType = SeriesChartType.Column;
                                }
                            }
                        }

                    }
                }

            }


            if (chartName == "CntOfPortfolioList")
            {
                if (ChartPropertiesList.CntOfPortfolioList.Count() > 0)
                {
                    int index = 0;
                    string Portfolios = "";
                    //for (int i = 0; i < ChartPropertiesList.CntOfMinisterList.Count(); i++)
                    foreach (var i in ChartPropertiesList.CntOfPortfolioList)
                    {
                        Portfolios = "Portfolios" + index;
                        chart.Series.Add(Portfolios);
                        chart.Series[Portfolios].LegendText = i.Portfolios.ToString();
                        chart.Series[Portfolios].Font = new Font("Calibri", 5, FontStyle.Regular);
                        chart.Series[Portfolios].Points.Add(i.sum);
                        chart.Series[Portfolios].LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                        chart.Series[Portfolios].LegendText = i.Portfolios.ToString();
                        chart.Series[Portfolios]["PixelPointWidth"] = "200";
                        index++;
                    }
                    //  chart.Series[Portfolios].IsValueShownAsLabel = false;
                    chart.Series[Portfolios].Color = Color.FromArgb(198, 99, 99);
                    chart.Series[Portfolios].ChartType = SeriesChartType.Column;
                    chart.Series[Portfolios].AxisLabel = "Total";
                    chart.Series[Portfolios].IsXValueIndexed = false;
                    chart.Series[Portfolios]["BarLabelStyle"] = "Center";

                }
            }


            if (chartName == "CntOfDivisionList")
            {
                if (ChartPropertiesList.CntOfDivisionList.Count() > 0)
                {
                    int index = 0;
                    string Divisions = "";
                    //for (int i = 0; i < ChartPropertiesList.CntOfMinisterList.Count(); i++)
                    foreach (var i in ChartPropertiesList.CntOfDivisionList)
                    {
                        Divisions = "Divisions" + index;
                        chart.Series.Add(Divisions);
                        chart.Series[Divisions].LegendText = i.Divisions.ToString();
                        chart.Series[Divisions].Font = new Font("Calibri", 5, FontStyle.Regular);
                        chart.Series[Divisions].Points.Add(i.sum);
                        chart.Series[Divisions].LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                        chart.Series[Divisions].LegendText = i.Divisions.ToString();
                        chart.Series[Divisions]["PixelPointWidth"] = "200";

                        index++;
                    }
                    //  chart.Series[Divisions].IsValueShownAsLabel = false;
                    chart.Series[Divisions].Color = Color.FromArgb(198, 99, 99);
                    chart.Series[Divisions].ChartType = SeriesChartType.Column;
                    chart.Series[Divisions].AxisLabel = "Total";
                    chart.Series[Divisions].IsXValueIndexed = false;
                    chart.Series[Divisions]["BarLabelStyle"] = "Center";
                }

            }

          



            //return seriesDetail;
            return chart;
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }




        public MimsLists seedData()
        {


            string TestData =
        @"{""mimsList"":[{
        ""Created"": ""\/Date(1435821487000)\/"", 
        ""ActorType"": ""MRT"", 
        ""DateReceived"": ""\/Date(1436253487000)\/"", 
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1437635887000)\/"",
        ""CriticalDueDate"": ""\/Date(1437635887000)\/"",
        ""ActionRequired"": ""Briefing falsete,Direct reply from the Department"",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Environment Parks and Heritage"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": ""Ministers Incoming correspondence"",
        ""MRTDueDate"": ""\/Date(1437635887000)\/"",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
       },
       {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1437031087000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""General 10 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1437981487000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": ""Direct reply from the Department"",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Environment Parks and Heritage"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1437981487000)\/"",
        ""DivisionDueDate"": ""\/Date(1437981487000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },

    
     {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(1436426287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""true"",
        ""TimingForAction"": ""General 5 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1437376687000)\/"",
        ""CriticalDueDate"": ""\/Date(1438240687000)\/"",
        ""ActionRequired"": ""Prepared Reply,Direct reply from the Department,Event Package"",
        ""PreparedReply"": ""COS"",
        ""Minister"": ""The Hon Will Hodgman MP"",
        ""Portfolio"": ""Premier"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": ""\/Date(1438154287000)\/"",
        ""DateReceivedFromExec"": ""\/Date(1438327087000)\/"",
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": ""\/Date(143694468)\/"",
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": ""Premiers Incoming correspondence"",
        ""MRTDueDate"": ""\/Date(1437376687000)\/"",
        ""DivisionDueDate"": ""\/Date(1437376687000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": ""\/Date(1438327087000)\/"",
        ""DateReceivedFromPO"": ""\/Date(1438931887000)\/"",
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1436858287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""Other"",
        ""CorrespondenceDueDate"": ""\/Date(1437635887000)\/"",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": ""Briefing falsete,Cabinet Documents,Prepared Reply,Event Package"",
        ""PreparedReply"": ""Premier"",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": ""\/Date(1437031087000)\/"",
        ""DateReceivedFromExec"": ""\/Date(1437031087000)\/"",
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": ""\/Date(143694468)\/"",
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": ""Event Requests"",
        ""MRTDueDate"": ""\/Date(1437635887000)\/"",
        ""DivisionDueDate"": ""\/Date(1437635887000)\/"",
        ""DateSentToMO"": ""\/Date(1437635887000)\/"",
        ""DateReceivedFromMO"": ""\/Date(1437722287000)\/"",
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {  
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(143694468)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""true"",
        ""TimingForAction"": ""VIP 3 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1437117487000)\/"",
        ""CriticalDueDate"": ""\/Date(1437031087000)\/"",
        ""ActionRequired"": ""Briefing falsete,Prepared Reply,Other"",
        ""PreparedReply"": ""DCOS"",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Information Techfalselogy and Infalsevation"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1437117487000)\/"",
        ""DivisionDueDate"": ""\/Date(1437117487000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""true"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1437635887000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""VIP 3 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1439536687000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Parliamentary Secretary"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1438240687000)\/"",
        ""DivisionDueDate"": ""\/Date(1438240687000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": ""\/Date(1437117487000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1437635887000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": ""Premier"",
        ""Minister"": ""The Hon Will Hodgman MP"",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""true"",
        ""DateSentToExec"": ""\/Date(1437117487000)\/"",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Ministerial Request Team"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1438154287000)\/"",
        ""CriticalDueDate"": ""\/Date(1438240687000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": ""COS"",
        ""Minister"": ""The Hon Michael Ferguson MP"",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(1437376687000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""true"",
        ""TimingForAction"": ""General 10 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1438586287000)\/"",
        ""CriticalDueDate"": ""\/Date(1439536687000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Will Hodgman MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": ""Inquiry,Minute,Briefing falsete,Cabinet Documents,Prepared Reply,Direct reply from the Department,Event Package,Appropriate action,Funding Request,Other"",
        ""PreparedReply"": ""Minister"",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": ""Inquiry,Minute,Briefing falsete,Cabinet Documents,Prepared Reply,Direct reply from the Department,Event Package,Appropriate action,Funding Request,Other"",
        ""PreparedReply"": ""Premier"",
        ""Minister"": ""The Hon Matthew Groom MP"",
        ""Portfolio"": ""Environment Parks and Heritage"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""true"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1437722287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""VIP 3 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1435735087000)\/"",
        ""CriticalDueDate"": ""\/Date(1437117487000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": ""\/Date(1433575087000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1438327087000)\/"",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": ""\/Date(1433575087000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1438327087000)\/"",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {  
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(1437722287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""General 10 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1438759087000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": ""Inquiry,Minute"",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Matthew Groom MP"",
        ""Portfolio"": ""Departmental"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1438759087000)\/"",
        ""DivisionDueDate"": ""\/Date(1438759087000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Ministerial Requests Team"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    }
   ]}";


            var myString = TestData.Replace("\r\n", string.Empty);
            try
            {
                var serveRecOut = JsonConvert.DeserializeObject<MimsLists>(myString);
                return serveRecOut;
            }
            catch (Exception e) { return null; };

        }
    }

}

/*
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
//using ModelEntity;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using MIMSChart.Models;

namespace MIMSChart.Controllers
{
    public class HomeController : Controller
    {
        // private CFSDB db = new CFSDB();
        static MimsLists mimsdataSelected;
        static IEnumerable<CntOfActorType> actorlist;
        static IEnumerable<Minister> ministerList;
        static IEnumerable<CntIsTimeExtendedBool> timeExtendedBoolList;
        static IEnumerable<CntOfDateReceived> CntOfDateReceivedList;
        static IEnumerable<CntIsTimeExtended> CntIsTimeExtendedList;
        static IEnumerable<CntOfDivision> CntOfDivisionList;
        static IEnumerable<CntOfPortfolio> CntOfPortfolioList;
        List<ChartProperties> ChartPropertiesList;
        // GET': Chart
        public ActionResult Index()
        {
           
           mimsdataSelected = seedData();
           var mimsdata = mimsdataSelected.mimsList.Where(tp => tp.Created <= DateTime.Now && tp.Created >= DateTime.Now.Date.AddDays(-31));
           actorlist = mimsdata
                 .Where(tp => tp.Created <= DateTime.Now && tp.Created >= DateTime.Now.Date.AddDays(-31))
                 .GroupBy(e => e.ActorType)
                 .Select(r => new CntOfActorType
                        {
                            Actor = r.First().ActorType,
                            sum =  r.Count(),                           
                        }); //.ToList();
           //ChartPropertiesList.Add(new ChartProperties("ActorList", "Count by Groups"));

           ministerList = mimsdata                
                .GroupBy(e => e.Minister)
                .Select(r => new Minister
                {
                    MinisterName = r.First().Minister,
                    sum = r.Count()
                }); //.ToList();

           timeExtendedBoolList = mimsdata
               .GroupBy(e => e.IsTimeExtended)
               .Select(r => new CntIsTimeExtendedBool
               {
                   timeExtendedBool = r.First().IsTimeExtended.ToString(),
                   resultbool = r.Count()
               }); //.ToList();

           CntOfDateReceivedList = mimsdata
             .GroupBy(e => e.DateReceived)
             .Select(r => new CntOfDateReceived
             {
                 DateReceived = r.First().DateReceived.ToString(),
                 sum = r.Count()
             }); //.ToList();

           CntIsTimeExtendedList = mimsdata
           .GroupBy(e => e.TimingForAction)
           .Select(r => new CntIsTimeExtended
           {
               TimeExtended = r.First().TimingForAction.ToString(),
               sum = r.Count()
           }); //.ToList();

           CntOfDivisionList = mimsdata
           .GroupBy(e => e.Division)
           .Select(r => new CntOfDivision
           {
               Divisions = r.First().Division.ToString(),
               sum = r.Count()
           }); //.ToList();


           CntOfPortfolioList = mimsdata
           .GroupBy(e => e.Portfolio)
           .Select(r => new CntOfPortfolio
           {
               Portfolios = r.First().Portfolio.ToString(),
               sum = r.Count()
           }); //.ToList();
            

           // var chart = new System.Web.Helpers.Chart(width: 300, height: 200)
           //      .AddSeries(
           //                  chartType: "Bar",
           //                  legend: "Energy Level",
           //                  xValue: new[] { "Week 1", "Week 2", "Week 3", "Week 4" },
           //                  yValues: new[] { "70", "44", "78", "89" })
           //                  .GetBytes("png");
            
 
            // return File(chart, "image/bytes");

            return View();
        }

        public ActionResult Chart(string chartType, string chartName)
        {
            var chart = buildChart(chartType);
            StringBuilder result = new StringBuilder();
            result.Append(getChartImage(chart));
            result.Append(chart.GetHtmlImageMap("ImageMap"));
            return Content(result.ToString());
        }

        private Chart buildChart(string chartType)
        {
            // Build Chart
            var chart = new Chart();

            chart.Width = 500;
            chart.Height = 300;

            // Create chart here
            chart.Titles.Add(CreateTitle());
            chart.Legends.Add(CreateLegend());
            chart.ChartAreas.Add(CreateChartArea());
            chart.Series.Add(CreateSeries());

            return chart;
        }

        private string getChartImage(Chart chart)
        {
            using (var stream = new MemoryStream())
            {
                string img = "<img src='data:image/png;base64,{0}' alt='' usemap='#ImageMap'>";
                chart.SaveImage(stream, ChartImageFormat.Png);
                string encoded = Convert.ToBase64String(stream.ToArray());
                return String.Format(img, encoded);
            }
        }

        private Title CreateTitle()
        {
            Title title = new Title();
            title.Text = "Count by Group"; //"Result Chart";
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);
            return title;
        }

        private Legend CreateLegend()
        {
            Legend legend = new Legend();
            legend.Enabled = true;
            legend.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            legend.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            legend.ShadowOffset = 3;
            legend.ForeColor = Color.FromArgb(26, 59, 105);
            legend.Title = "Legend";
            return legend;
        }

        private ChartArea CreateChartArea()
        {
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "Result Chart";
            chartArea.BackColor = Color.Transparent;
            chartArea.AxisX.IsLabelAutoFit = false;
            chartArea.AxisY.IsLabelAutoFit = false;
            chartArea.AxisX.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font = new Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.Interval = 1;
            chartArea.Area3DStyle.Enable3D = true;
            return chartArea;
        }

        public Series CreateSeries(string chartType, string chartName)
        {
            Series seriesDetail = new Series();
            seriesDetail.Name = "Result Chart";
            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = Color.FromArgb(198, 99, 99);

            switch (chartType)
            {
                case "Pie":
                    seriesDetail.ChartType = SeriesChartType.Pie;
                    break;
                case "Bar":
                    seriesDetail.ChartType = SeriesChartType.Bar;
                    break;
                case "Line":
                    seriesDetail.ChartType = SeriesChartType.Line;
                    break;
                case "other":
                    seriesDetail.ChartType = SeriesChartType.Funnel;
                    break;
            }
            seriesDetail.BorderWidth = 2;

            //data

            GetData(seriesDetail, chartName);
            //for (int i = 1; i < 20; i++)
            //{
            //    var p = seriesDetail.Points.Add(i);
            //    p.Label = String.Format("Point {0}", i);
            //    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"myfunction('{0}')\"", i);
            //    p["BarLabelStyle"] = "Center";
            //}

            seriesDetail.ChartArea = "Result Chart";
            return seriesDetail;
        }

        private void GetData(Series seriesDetail, string chartName)
        {
            if (seriesDetail.ChartTypeName == "Pie" && chartName == "ActorList")
            {                
                for (int i = 0; i < actorlist.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(actorlist.ElementAt(i).sum);
                    p.Label = actorlist.ElementAt(i).sum.ToString(); // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p.AxisLabel = actorlist.ElementAt(i).Actor.ToString();
                    p.LegendText = actorlist.ElementAt(i).Actor.ToString();                   
                    p["BarLabelStyle"] = "Center";
                }
            }

            if (seriesDetail.ChartTypeName == "Bar" && chartName == "MinisterList")
            {
                for (int i = 0; i < ministerList.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(ministerList.ElementAt(i).sum);
                    p.Label = ministerList.ElementAt(i).sum.ToString(); // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p.AxisLabel = ministerList.ElementAt(i).MinisterName.ToString();
                    p.Name = "Ministers";
                    p["BarLabelStyle"] = "Center";
                }
            }
            if (seriesDetail.ChartTypeName == "Bar" && chartName == "TimeExtendedBoolList")
            {
                for (int i = 0; i < timeExtendedBoolList.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(timeExtendedBoolList.ElementAt(i).resultbool);
                    p.Label = timeExtendedBoolList.ElementAt(i).timeExtendedBool.ToString() + " " + timeExtendedBoolList.ElementAt(i).resultbool; // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p.AxisLabel = timeExtendedBoolList.ElementAt(i).timeExtendedBool == "false" ? "No" : "Yes";
                    p.Name = "Negotiated Timeframes";
                    p["BarLabelStyle"] = "Center";
                }
            }

            if (seriesDetail.ChartTypeName == "Line" && chartName == "CntOfDateReceivedList")
            {
                for (int i = 0; i < CntOfDateReceivedList.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(CntOfDateReceivedList.ElementAt(i).sum);
                    p.Label = CntOfDateReceivedList.ElementAt(i).DateReceived.ToString() + " " + CntOfDateReceivedList.ElementAt(i).sum; // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p["BarLabelStyle"] = "Center";
                }
            }

            if (seriesDetail.ChartTypeName == "Bar" && chartName == "CntIsTimeExtendedList")
            {
                for (int i = 0; i < CntIsTimeExtendedList.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(CntIsTimeExtendedList.ElementAt(i).sum);
                    p.Label = CntIsTimeExtendedList.ElementAt(i).TimeExtended.ToString() + " " + CntIsTimeExtendedList.ElementAt(i).sum; // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p["BarLabelStyle"] = "Center";
                }
            }

            if (seriesDetail.ChartTypeName == "Bar" && chartName == "CntOfDivisionList")
            {
                for (int i = 0; i < CntOfDivisionList.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(CntOfDivisionList.ElementAt(i).sum);
                    p.Label = CntOfDivisionList.ElementAt(i).Divisions.ToString() + " " + CntOfDivisionList.ElementAt(i).sum; // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p["BarLabelStyle"] = "Center";
                }
            }

            if (seriesDetail.ChartTypeName == "Bar" && chartName == "CntOfPortfolioList")
            {
                for (int i = 0; i < CntOfPortfolioList.Count(); i++)
                {
                    var p = seriesDetail.Points.Add(CntOfPortfolioList.ElementAt(i).sum);
                    p.Label = CntOfPortfolioList.ElementAt(i).Portfolios.ToString() + " " + CntOfPortfolioList.ElementAt(i).sum; // String.Format("Point {0}", i);
                    p.LabelMapAreaAttributes = String.Format("href=\"javascript:void(0)\" onclick=\"function(alert(p.label))");
                    p["BarLabelStyle"] = "Center";
                }
            }
            
        }
        // GET': Chart/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CFSActivity cFSActivity = await db.CFSActivities.FindAsync(id);
        //    if (cFSActivity == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(cFSActivity);
        //}




        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }




        public MimsLists seedData()
        {


            string TestData =
        @"{""mimsList"":[{
        ""Created"": ""\/Date(1435821487000)\/"", 
        ""ActorType"": ""MRT"", 
        ""DateReceived"": ""\/Date(1436253487000)\/"", 
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1437635887000)\/"",
        ""CriticalDueDate"": ""\/Date(1437635887000)\/"",
        ""ActionRequired"": ""Briefing falsete,Direct reply from the Department"",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Environment Parks and Heritage"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": ""Ministers Incoming correspondence"",
        ""MRTDueDate"": ""\/Date(1437635887000)\/"",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
       },
       {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1437031087000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""General 10 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1437981487000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": ""Direct reply from the Department"",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Environment Parks and Heritage"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1437981487000)\/"",
        ""DivisionDueDate"": ""\/Date(1437981487000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },

    
     {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(1436426287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""true"",
        ""TimingForAction"": ""General 5 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1437376687000)\/"",
        ""CriticalDueDate"": ""\/Date(1438240687000)\/"",
        ""ActionRequired"": ""Prepared Reply,Direct reply from the Department,Event Package"",
        ""PreparedReply"": ""COS"",
        ""Minister"": ""The Hon Will Hodgman MP"",
        ""Portfolio"": ""Premier"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": ""\/Date(1438154287000)\/"",
        ""DateReceivedFromExec"": ""\/Date(1438327087000)\/"",
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": ""\/Date(143694468)\/"",
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": ""Premiers Incoming correspondence"",
        ""MRTDueDate"": ""\/Date(1437376687000)\/"",
        ""DivisionDueDate"": ""\/Date(1437376687000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": ""\/Date(1438327087000)\/"",
        ""DateReceivedFromPO"": ""\/Date(1438931887000)\/"",
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1436858287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""Other"",
        ""CorrespondenceDueDate"": ""\/Date(1437635887000)\/"",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": ""Briefing falsete,Cabinet Documents,Prepared Reply,Event Package"",
        ""PreparedReply"": ""Premier"",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": ""\/Date(1437031087000)\/"",
        ""DateReceivedFromExec"": ""\/Date(1437031087000)\/"",
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": ""\/Date(143694468)\/"",
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": ""Event Requests"",
        ""MRTDueDate"": ""\/Date(1437635887000)\/"",
        ""DivisionDueDate"": ""\/Date(1437635887000)\/"",
        ""DateSentToMO"": ""\/Date(1437635887000)\/"",
        ""DateReceivedFromMO"": ""\/Date(1437722287000)\/"",
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {  
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(143694468)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""true"",
        ""TimingForAction"": ""VIP 3 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1437117487000)\/"",
        ""CriticalDueDate"": ""\/Date(1437031087000)\/"",
        ""ActionRequired"": ""Briefing falsete,Prepared Reply,Other"",
        ""PreparedReply"": ""DCOS"",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Information Techfalselogy and Infalsevation"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1437117487000)\/"",
        ""DivisionDueDate"": ""\/Date(1437117487000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""true"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1437635887000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""VIP 3 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1439536687000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Parliamentary Secretary"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1438240687000)\/"",
        ""DivisionDueDate"": ""\/Date(1438240687000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": ""\/Date(1437117487000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1437635887000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": ""Premier"",
        ""Minister"": ""The Hon Will Hodgman MP"",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""true"",
        ""DateSentToExec"": ""\/Date(1437117487000)\/"",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Ministerial Request Team"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1438154287000)\/"",
        ""CriticalDueDate"": ""\/Date(1438240687000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": ""COS"",
        ""Minister"": ""The Hon Michael Ferguson MP"",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(1437376687000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""true"",
        ""TimingForAction"": ""General 10 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1438586287000)\/"",
        ""CriticalDueDate"": ""\/Date(1439536687000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Will Hodgman MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""true"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": ""Inquiry,Minute,Briefing falsete,Cabinet Documents,Prepared Reply,Direct reply from the Department,Event Package,Appropriate action,Funding Request,Other"",
        ""PreparedReply"": ""Minister"",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": ""Inquiry,Minute,Briefing falsete,Cabinet Documents,Prepared Reply,Direct reply from the Department,Event Package,Appropriate action,Funding Request,Other"",
        ""PreparedReply"": ""Premier"",
        ""Minister"": ""The Hon Matthew Groom MP"",
        ""Portfolio"": ""Environment Parks and Heritage"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""true"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": ""\/Date(1437722287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""VIP 3 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1435735087000)\/"",
        ""CriticalDueDate"": ""\/Date(1437117487000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": ""\/Date(1433575087000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1438327087000)\/"",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""true""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": ""\/Date(1433575087000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": ""\/Date(1438327087000)\/"",
        ""CriticalDueDate"": ""\/Date(1438327087000)\/"",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": """",
        ""Portfolio"": """",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""false"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""AO"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Peter Gutwein MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {  
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""DLO"",
        ""DateReceived"": ""\/Date(1437722287000)\/"",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": ""General 10 Days"",
        ""CorrespondenceDueDate"": ""\/Date(1438759087000)\/"",
        ""CriticalDueDate"": """",
        ""ActionRequired"": ""Inquiry,Minute"",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Matthew Groom MP"",
        ""Portfolio"": ""Departmental"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": ""\/Date(1438759087000)\/"",
        ""DivisionDueDate"": ""\/Date(1438759087000)\/"",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Office of eGovernment"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    },  
    {   
        ""Created"": ""\/Date(1436771887000)\/"",
        ""ActorType"": ""MRT"",
        ""DateReceived"": """",
        ""AckfalsewledgedByPremiersOffice"": ""false"",
        ""TimingForAction"": """",
        ""CorrespondenceDueDate"": """",
        ""CriticalDueDate"": """",
        ""ActionRequired"": """",
        ""PreparedReply"": """",
        ""Minister"": ""The Hon Jeremy Rockliff MP"",
        ""Portfolio"": ""Education and Training"",
        ""CMUApprovalRequired"": ""false"",
        ""DateSentToExec"": """",
        ""DateReceivedFromExec"": null,
        ""DivisionToMailCorro"": ""false"",
        ""MailOutDate"": null,
        ""ApproveOnBehalf"": ""false"",
        ""InformationNFA"": ""false"",
        ""PSURequired"": ""true"",
        ""CorrespondenceType"": """",
        ""MRTDueDate"": """",
        ""DivisionDueDate"": """",
        ""DateSentToMO"": null,
        ""DateReceivedFromMO"": null,
        ""DateSentToPO"": null,
        ""DateReceivedFromPO"": null,
        ""DLOMailoutDate"": null,
        ""Division"": ""Ministerial Requests Team"",
        ""Branch"": null,
        ""IsDeleted"": ""false"",
        ""IsFasttracked"": ""false"",
        ""IsTimeExtended"": ""false"",
        ""IsCompleted"": ""false""
    }
   ]}";
            

            var myString = TestData.Replace("\r\n", string.Empty);
            try
            {
                var serveRecOut = JsonConvert.DeserializeObject<MimsLists>(myString);
                return serveRecOut;
            }
            catch (Exception e) { return null; };
            
        }
    }

}
*/