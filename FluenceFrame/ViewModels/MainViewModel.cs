using FluenceFrame.Models;
using FluenceFrame.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VMS.TPS.Common.Model.API;

namespace FluenceFrame.ViewModels
{
    public class MainViewModel:ViewModelBase
    {
        private VMS.TPS.Common.Model.API.Application _app;
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
            set 
            { 
                SetProperty(ref _originalSelected,value);
                if (OriginalSelected)
                {
                    ImageSource = _originalImage;
                }
            }
        }
        private bool _fluenceSelected;

        public bool FluenceSelected
        {
            get { return _fluenceSelected; }
            set 
            { 
                SetProperty(ref _fluenceSelected,value);
                if (FluenceSelected)
                {
                    ImageSource = _fluenceImage;
                }
            }
        }
        private string _patientId;

        public string PatientId
        {
            get { return _patientId; }
            set 
            {
                SetProperty(ref _patientId,value);
                PushToARIACommand.RaiseCanExecuteChanged();
                OpenPatientCommand.RaiseCanExecuteChanged();

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
                if(SelectedPlan != null)
                {
                    PushMessage = "Ready...";
                }
            }
        }
        private string _pushMessage;

        public string PushMessage
        {
            get { return _pushMessage; }
            set { SetProperty(ref _pushMessage,value); }
        }


        public List<PlanModel> Plans { get; private set; }

        public RelayCommand ImportImageCommand { get; private set; }
        public RelayCommand ExportToFileCommand { get; private set; }
        public RelayCommand PushToARIACommand { get; private set; }
        public float[,] fluence { get; private set; }
        public Point origin { get; private set; }
        public RelayCommand OpenPatientCommand { get; private set; }
        public MainViewModel(VMS.TPS.Common.Model.API.Application app, Patient patient)
        {
            _app = app;
            _patient = patient;

            Plans = new List<PlanModel>();
            OriginalSelected = true;
            ImportImageCommand = new RelayCommand(OnImportImage);
            ExportToFileCommand = new RelayCommand(OnExportImage, CanExportImage);
            PushToARIACommand = new RelayCommand(OnPushToARIA, CanPushToARIA);
            OpenPatientCommand = new RelayCommand(OnOpenPatient, CanOpenPatient);
            PushMessage = "Import image to convert";
            if (_app != null) { CanARIA = true; }
            if (_patient != null) { PatientId = _patient.Id; }
            GetPlans();
        }

        private bool CanOpenPatient(object arg)
        {
            return !String.IsNullOrEmpty(PatientId);
        }

        private void OnOpenPatient(object obj)
        {
            _patient = _app.OpenPatientById(PatientId);
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
                    EsapiAutomationService.SetFluence(false, fluence, origin);
                    _app.SaveModifications();
                    PushMessage = "Saved to ARIA.";
                }

            }
        }

        private bool CanPushToARIA(object arg)
        {
            return CanARIA && !String.IsNullOrEmpty(PatientId) && _fluenceImage != null && SelectedPlan != null;
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
                    if(bitmap.PixelWidth>80 || bitmap.PixelHeight > 80)
                    {
                        ImageDetails += $"\nImage dimensions too large.\nImage will be downsampled automatically.";
                    }
                    FluenceModel fluenceModel = FluenceConverter.ConvertImageToFluenceMap(bitmap);
                    fluence = fluenceModel.Fluence;
                    origin = fluenceModel.Origin;
                    _fluenceImage = FluenceConverter.ConvertFluenceMapToHeatMap();
                    ExportToFileCommand.RaiseCanExecuteChanged();


                    if (SelectedPlan == null)
                    {
                        PushMessage = "Select a plan to push to ARIA";
                    }
                }
                catch (Exception ex)
                {
                    // Update the details string with error information.
                    ImageDetails = $"Error loading image: {ex.Message}\n{ex.InnerException}";
                }
            }
        }

        
        /// <summary>
        /// Exports a fluence map to a text file with the required format.
        /// </summary>
        /// <param name="fluenceMap">2D float array of normalized fluence values.</param>
        /// <param name="filePath">Output file path.</param>
        /// <param name="spacingX">Pixel spacing in the X direction (e.g., 2.5 mm or 0.25 cm as desired).</param>
        /// <param name="spacingY">Pixel spacing in the Y direction.</param>
        /// <param name="originX">The computed origin X coordinate (in same units as spacing).</param>
        /// <param name="originY">The computed origin Y coordinate.</param>
        public void OnExportImage(object obj)
        {
            float spacingX  = 2.5f;
            float spacingY = 2.5f;
            if (fluence == null)
                throw new ArgumentNullException(nameof(fluence));

            int height = fluence.GetLength(0);
            int width = fluence.GetLength(1);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Optimal fluence file (*.optimal_fluence)|*.optimal_fluence";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = "Save fluence file";
            if (saveFileDialog.ShowDialog() == true)
            {
                // Create or overwrite the output file.
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Write header lines.
                    writer.WriteLine("# Field 1 - Fluence");
                    writer.WriteLine("optimalfluence");
                    writer.WriteLine($"sizex\t{width}");
                    writer.WriteLine($"sizey\t{height}");
                    writer.WriteLine($"spacingx\t{spacingX.ToString(CultureInfo.InvariantCulture)}");
                    writer.WriteLine($"spacingy\t{spacingY.ToString(CultureInfo.InvariantCulture)}");
                    writer.WriteLine($"originx\t{origin.X.ToString("F4", CultureInfo.InvariantCulture)}");
                    writer.WriteLine($"originy\t{origin.Y.ToString("F4", CultureInfo.InvariantCulture)}");
                    writer.WriteLine("values");

                    // Write the fluence values row by row.
                    for (int y = 0; y < height; y++)
                    {
                        string[] rowValues = new string[width];
                        for (int x = 0; x < width; x++)
                        {
                            // Format each value with a suitable numeric format.
                            rowValues[x] = fluence[y, x].ToString("G6", CultureInfo.InvariantCulture);
                        }
                        writer.WriteLine(string.Join("\t", rowValues));
                    }
                }
            }
        }
        
    }
}
