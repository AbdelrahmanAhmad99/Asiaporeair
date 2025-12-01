using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.FlightOperations
{
    // We need a better response object than just <int>
    public class GenerationReportDto
    {
        public int InstancesCreated { get; set; } = 0;
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Failures { get; set; } = new List<string>();
    }
}
