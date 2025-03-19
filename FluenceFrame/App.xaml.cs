using FluenceFrame.ViewModels;
using FluenceFrame.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using esapi = VMS.TPS.Common.Model.API;

namespace FluenceFrame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _patientId;
        private string _courseId;

        public bool ESAPIMode { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (ConfigurationManager.AppSettings["RunMode"] == "ESAPI")
            {
                ESAPIMode = true;
            }
            if (ESAPIMode)
            {
                try
                {
                    if(e.Args.Count() > 0)
                    {
                        _patientId = e.Args.First();
                    }
                    using (esapi.Application ESAPIApp = esapi.Application.CreateApplication())
                    {
                        esapi.Patient patient = null;
                        if (!String.IsNullOrEmpty(_patientId))
                        {
                            patient = ESAPIApp.OpenPatientById(_patientId);
                        }
                        var mainView = new MainView();
                        var mainViewModel = new MainViewModel(ESAPIApp, patient);
                        mainView.DataContext = mainViewModel;
                        mainView.ShowDialog();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                var mainView = new MainView();
                var mainViewModel = new MainViewModel();
                mainView.DataContext = mainViewModel;
                mainView.ShowDialog();
            }
        }
    }
}
