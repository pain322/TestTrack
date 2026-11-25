using System;
using System.Collections.Generic;
using System.Text;

namespace TestTrack.Models
{
    public class BugRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public string ReporterName { get; set; } = "";
        public string AssigneeName { get; set; } = "";
    }
}