using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace FluenceFrame.Models
{
    public class PlanModel
    {
        public string PlanId { get; set; }
        public string CourseId { get; set; }
        public string Description { get; set; }

        public PlanModel(PlanSetup plan)
        {
            PlanId = plan.Id;
            CourseId = plan.Course.Id;
            Description = $"{PlanId} [{CourseId}]";
        }
    }
}
