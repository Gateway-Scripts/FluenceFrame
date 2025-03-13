using FluenceFrame.Models;
using FluenceFrame.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VMS.TPS.Common.Model.API;

namespace FluenceFrame.ViewModels
{
    public class MainViewModel:ViewModelBase
    {
        private Application _app;
        private Patient _patient;
        private BitmapSource _fluenceImage;
        private BitmapSource _originalImage;
        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status,value); }
        }
        private string _imageDetails;

        public string ImageDetails
        {
            get { return _imageDetails; }
            set { SetProperty(ref _imageDetails,value); }
        }
        private BitmapSource _imageSource;

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set {SetProperty(ref _imageSource,value); }
        }
        private bool _originalSelected;

        public bool OriginalSelected
        {
            get { return _originalSelected; }
            set { SetProperty(ref _originalSelected,value); }
        }
        private bool _fluenceSelected;

        public bool FluenceSelected
        {
            get { return _fluenceSelected; }
            set { SetProperty(ref _fluenceSelected,value); }
        }
        private string _patientId;

        public string PatientId
        {
            get { return _patientId; }
            set 
            {
                SetProperty(ref _patientId,value);
                PushToARIACommand.RaiseCanExecuteChanged();
            }
        }

        private bool _canARIA;

        public bool CanARIA
        {
            get { return _canARIA; }
            set 
            {
                SetProperty(ref _canARIA,value);
                PushToARIACommand.RaiseCanExecuteChanged();
            }
        }
        private PlanModel _selectedPlan;

        public PlanModel SelectedPlan
        {
            get { return _selectedPlan; }
            set { 
                SetProperty(ref _selectedPlan, value);
                PushToARIACommand.RaiseCanExecuteChanged();
            }
        }

        public List<PlanModel> Plans { get; private set; }

        public RelayCommand ImportImageCommand { get; private set; }
        public RelayCommand ExportToFileCommand { get; private set; }
        public RelayCommand PushToARIACommand { get; private set; }
        
        public MainViewModel(Application app, Patient patient)
        {
            _app = app;
            _patient = patient;

            Plans = new List<PlanModel>();

            ImportImageCommand = new RelayCommand(OnImportImage);
            ExportToFileCommand = new RelayCommand(OnExportImage, CanExportImage);
            PushToARIACommand = new RelayCommand(OnPushToARIA, CanPushToARIA);

            if (_app != null) { CanARIA = true; }
            if (_patient != null) { PatientId = _patient.Id; }
            GetPlans();
        }

        private void GetPlans()
        {
            if (_patient != null)
            {
                foreach(var course in _patient.Courses)
                {
                    foreach(var planSetup in course.PlanSetups)
                    {
                        Plans.Add(new PlanModel(planSetup));
                    }
                }
            }

        }

        private void OnPushToARIA(object obj)
        {
            //generate a new course.
            string courseId = "FluenceArt";
            Status = "Beginning push to ARIA";
            if (_patient != null)
            {
                EsapiAutomationService.SetPatient(_patient);
                EsapiAutomationService.BeginModifications();
                if (SelectedPlan != null)
                {
                    Course course = _patient.Courses.First(c => c.Id.Equals(SelectedPlan.CourseId));
                    EsapiAutomationService.SetCourse(course);
                    PlanSetup plan = course.PlanSetups.First(ps=>ps.Id.Equals(SelectedPlan.PlanId));
                    EsapiAutomationService.SetPlan(plan);

                }

                //generate course

                //this code below was to generate a plan, but since I won't know the users' machine configurations, the code will expect a plan.
                //bool bCourse = EsapiAutomationService.GenerateCourse();
                //if (bCourse)
                //{
                //    //generate plan
                //    Status += $"\nCourse generated: {EsapiAutomationService.Course.Id}";
                //    bool bPlan = EsapiAutomationService.GeneratePlan();
                //    if (bPlan)
                //    {
                //        Status += $"\nPlan generate: {EsapiAutomationService.Plan.Id}";
                //        //create field
                //    }
                //    else
                //    {
                //        Status += $"\nError Generating Plan...";
                //    }
                //}
                //else
                //{
                //    Status += $"\nError Generating Course...";
                //}
            }
        }

        private bool CanPushToARIA(object arg)
        {
            return CanARIA && !String.IsNullOrEmpty(PatientId) && _fluenceImage != null && SelectedPlan != null;
        }

        private void OnExportImage(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanExportImage(object arg)
        {
            return _fluenceImage != null;
        }

        private void OnImportImage(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Load the image into a BitmapImage.
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // ensures the file is closed after load
                    bitmap.EndInit();
                    bitmap.Freeze(); // makes it cross-thread accessible

                    ImageSource = bitmap;
                    _originalImage = bitmap;
                    // Retrieve file info and update details string.
                    FileInfo fileInfo = new FileInfo(filePath);
                    ImageDetails = $"Filename: {fileInfo.Name}\n" +
                                   $"Dimensions: {bitmap.PixelWidth} x {bitmap.PixelHeight}\n" +
                                   $"Size: {fileInfo.Length / 1024} KB";
                }
                catch (Exception ex)
                {
                    // Update the details string with error information.
                    ImageDetails = $"Error loading image: {ex.Message}";
                }
            }
        }
    }
}
