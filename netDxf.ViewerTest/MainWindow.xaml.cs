using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace netDxf.ViewerTest
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClickOpen(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dxf";
            dlg.InitialDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "DXF");
            dlg.Filter = "Autocad DXF Files (.dxf)|*.dxf";

            // Show open file dialog box 
            bool? result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                viewer.FileName = dlg.FileName;
            }
        }

        private void OnClickZoomExtents(object sender, RoutedEventArgs e)
        {
            viewer.ZoomExtents();
        }

        private void OnClickZoomIn(object sender, RoutedEventArgs e)
        {
            viewer.ZoomIn();
        }

        private void OnClickZoomOut(object sender, RoutedEventArgs e)
        {
            viewer.ZoomOut();
        }
    }
}
