Below is an example of a GitHub README.md file for your project. You can adjust the images and links as needed.

---

# FluenceArt

**FluenceArt** is a WPF-based Eclipse scripting application that converts user-imported images into normalized fluence maps. Designed for physicists and radiotherapy professionals, it allows users to visualize fluence maps as heat maps, export the data in a custom format, and even integrate directly with ARIA patient records.

![Logo or Main Screenshot](resources/TheoTransfer.PNG)

## Features

- **Image Import:** Supports various image formats (PNG, JPEG, BMP, GIF, TIFF).
- **Fluence Map Conversion:** Converts images into a normalized fluence map (2D float array).
- **Downsampling:** Automatically downsamples images that exceed a 20 cm physical width to conform to MLC field size limitations.
- **Heat Map Preview:** Displays a heat map visualization where the highest fluence values are shown in red and lower values in blue.
- **Export Functionality:** Exports the fluence map to a text file with a specific header and data format.
- **ARIA Integration:** (Optional) Pushes the generated fluence map into a patient record in ARIA.
- **MVVM Architecture:** Built on the MVVM pattern using WPF for clean separation of concerns.

## Getting Started

### Prerequisites

- This application is available in V15.6, V16.1, and V18.1 of Eclipse.
- Applications utilizes .NET Framework 4.5, 4.6.1 and 4.8 for the versions of Eclipse listed above.
- Patient ID may be passed in with input argument.

### Installation

1. **Clone the repository:**

   ```bash
   git clone https://github.com/yourusername/FluenceArt.git
   ```

2. **Open the Solution:**

   Open `FluenceArt.sln` in Visual Studio and restore any NuGet packages.

3. **Build the Application:**

   Build the solution using Visual Studio (Build -> Build Solution).

### Running the Application

- Launch the application (Debug -> Start Debugging or press F5).
- Use the UI to import an image, preview the fluence map, and perform the export or ARIA integration.

## Usage

1. **Import Image:**  
   Click the **Import Image** button to load an image file. The tool supports multiple file types such as PNG, JPEG, BMP, GIF, and TIFF.

2. **Preview and Conversion:**  
   The application converts the image to a normalized fluence map and displays it as a heat map. Use the toggle options to switch between the original image and the heat map view.

3. **Downsampling:**  
   If the imported image exceeds a physical width of 20 cm (based on a sampling of 2.5 mm per pixel), the tool automatically downsamples the image by half to ensure compatibility with MLC field size limits.

4. **Export Fluence Map:**  
   Click **Export Fluence Map** to save the generated fluence map. The file format is similar to the following:

   ```
   # Field 1 - Fluence
   optimalfluence
   sizex	32
   sizey	32
   spacingx	2.5
   spacingy	2.5
   originx	-38.75
   originy	38.75
   values
   0	0	0	...
   ```

5. **Push to ARIA:**  
   If desired, enter the patient details and click **Push to ARIA** to integrate the fluence map into the ARIA system.

![UI Screenshot](resources/FluenceArtUI.PNG)

## Code Structure

- **Views:** Contains XAML files for the UI (e.g., `MainView.xaml`).
- **ViewModels:** Implements MVVM logic and commands (e.g., image import, conversion, export).
- **Models:** Data models for fluence maps and patient details.
- **Helpers:** Utility classes for image processing, conversion (e.g., `FluenceConverter.cs`, `FluenceExporter.cs`, `ImageResizer.cs`).

## Example Code Snippets

### Image Import and Conversion

```csharp
private void ImportImage(object parameter)
{
    OpenFileDialog openFileDialog = new OpenFileDialog
    {
        Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|All Files (*.*)|*.*"
    };

    if (openFileDialog.ShowDialog() == true)
    {
        string filePath = openFileDialog.FileName;
        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(filePath);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        // Downsample if necessary
        BitmapSource processedImage = ImageResizer.DownsampleIfTooLarge(bitmap);

        // Set the image source for preview and convert to fluence map.
        ImageSource = processedImage;
        // Further processing to generate fluence map...
    }
}
```

### Exporting the Fluence Map

```csharp
public static void ExportFluenceMap(float[,] fluenceMap, string filePath, float spacingX, float spacingY, double originX, double originY)
{
    using (StreamWriter writer = new StreamWriter(filePath))
    {
        writer.WriteLine("# Field 1 - Fluence");
        writer.WriteLine("optimalfluence");
        writer.WriteLine($"sizex\t{fluenceMap.GetLength(1)}");
        writer.WriteLine($"sizey\t{fluenceMap.GetLength(0)}");
        writer.WriteLine($"spacingx\t{spacingX.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteLine($"spacingy\t{spacingY.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteLine($"originx\t{originX.ToString("F4", CultureInfo.InvariantCulture)}");
        writer.WriteLine($"originy\t{originY.ToString("F4", CultureInfo.InvariantCulture)}");
        writer.WriteLine("values");

        for (int y = 0; y < fluenceMap.GetLength(0); y++)
        {
            string[] rowValues = new string[fluenceMap.GetLength(1)];
            for (int x = 0; x < fluenceMap.GetLength(1); x++)
            {
                rowValues[x] = fluenceMap[y, x].ToString("G6", CultureInfo.InvariantCulture);
            }
            writer.WriteLine(string.Join("\t", rowValues));
        }
    }
}
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes and commit them (`git commit -am 'Add new feature'`).
4. Push to your branch (`git push origin feature-branch`).
5. Create a Pull Request.

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.

## Contact

For questions or feedback, please open an issue or contact [Matthew Schmidt](mschmidt@gatewayscripts.com).

---

Feel free to modify or extend this README to fit your project’s needs. Happy coding!
