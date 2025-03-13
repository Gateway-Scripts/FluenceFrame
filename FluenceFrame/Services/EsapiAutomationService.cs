using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: ESAPIScript(IsWriteable = true)]
namespace FluenceFrame.Services
{
    public static class EsapiAutomationService
    {
        public static ExternalPlanSetup Plan;

        public static Patient Patient { get; private set; }
        public static Course Course { get; private set; }


        public static void SetPatient(Patient patient)
        {
            Patient = patient;
        }
        public static void SetCourse (Course course)
        {
            Course = course;
        }
        public static void SetPlan(PlanSetup plan)
        {
            Plan = plan as ExternalPlanSetup;
        }
        public static void BeginModifications()
        {
            Patient.BeginModifications();
        }
        public static bool SetFluence(bool autoCalculate, float[,] fluence)
        {
            //get first field.
            Beam beam1 = Plan.Beams.First(b => !b.IsSetupField);
            //construct fluence.

            Fluence fieldfluence = new Fluence(fluence, 0, 0);
            //beam1.SetOptimalFluence()
            return true;
        }
        //public static bool GenerateCourse()
        //{
        //    string courseId = "FluenceArt";
        //    Course = null;
        //    if (Patient.Courses.Any(c => c.Id.Equals(courseId, StringComparison.OrdinalIgnoreCase)))
        //    {
        //        Course = Patient.Courses.First(c => c.Id.Equals(courseId, StringComparison.OrdinalIgnoreCase));
        //        return true;
        //    }
        //    else
        //    {
        //        Course = Patient.AddCourse();
        //        Course.Id = courseId;
        //        return true;
        //    }
        //    return false;
        //}
    }
}
