using Basler.Pylon;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaslerCamera
{
    public class Parameter
    {
        public double Gain_val { get; set; }
        public double ExposureTime_val { get; set; }
        public double Gamma_val { get; set; }
        public string Save_Image_Path_val { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Do you want to close the window？", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void ChangeIcon(Image name, string absolute_path, string tip, string log_message)
        {
            name.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + absolute_path));
            name.ToolTip = tip;
            Logger.WriteLog(log_message, 1, richTextBoxGeneral);
        }

        private void DisableAllButtons(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Button button)
                {
                    button.IsEnabled = false;
                }
                DisableAllButtons(child);
            }
        }

        private void DisableAllToggleButton(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ToggleButton togglebutton)
                {
                    togglebutton.IsEnabled = false;
                }
                DisableAllToggleButton(child);
            }
        }

        private void CameraInit()
        {
            BC.Display = Display_Windows;
            bool state;
            BC.Init(out state);
            if (!state)
            {
                DisableAllButtons(this);
                DisableAllToggleButton(this);
            }
        }

        private Dictionary<string, double> GetCameraParameterRage()
        {
            Dictionary<string, double> camera_parameter = new Dictionary<string, double>();
            Gain.Minimum = BC.camera.Parameters[PLCamera.Gain].GetMinimum();
            Gain.Maximum = BC.camera.Parameters[PLCamera.Gain].GetMaximum();
            camera_parameter.Add("Gain Min", (double)Gain.Minimum);
            camera_parameter.Add("Gain Max", (double)Gain.Maximum);
            Gain_Tip.Text = "min:" + Gain.Minimum.ToString() + ", max:" + Gain.Maximum.ToString();
            ExposureTime.Minimum = BC.camera.Parameters[PLCamera.ExposureTime].GetMinimum();
            ExposureTime.Maximum = BC.camera.Parameters[PLCamera.ExposureTime].GetMaximum();
            camera_parameter.Add("ExposureTime Min", (double)ExposureTime.Minimum);
            camera_parameter.Add("ExposureTime Max", (double)ExposureTime.Maximum);
            ExposureTime_Tip.Text = "min:" + ExposureTime.Minimum.ToString() + ", max:" + ExposureTime.Maximum.ToString();
            Gamma.Minimum = BC.camera.Parameters[PLCamera.Gamma].GetMinimum();
            Gamma.Maximum = BC.camera.Parameters[PLCamera.Gamma].GetMaximum();
            camera_parameter.Add("Gamma Min", (double)Gamma.Minimum);
            camera_parameter.Add("Gamma Max", (double)Gamma.Maximum);
            Gamma_Tip.Text = "min:" + Gamma.Minimum.ToString() + ", max:" + Gamma.Maximum.ToString();
            return camera_parameter;

        }

        public void CameraParameterInit()
        {
            Dictionary<string, double> camera_parameter = GetCameraParameterRage();
            // Load Camera Parameter
            if (Convert.ToDouble(Gain.Text) >= camera_parameter["Gain Min"] && Convert.ToDouble(Gain.Text) <= camera_parameter["Gain Max"])
            {
                BC.camera.Parameters[PLCamera.Gain].SetValue(Convert.ToDouble(Gain.Text));
            }
            else
            {
                BC.camera.Parameters[PLCamera.Gain].SetValue(camera_parameter["Gain Min"]);
            }
            if (Convert.ToDouble(ExposureTime.Text) >= camera_parameter["ExposureTime Min"] && Convert.ToDouble(ExposureTime.Text) <= camera_parameter["ExposureTime Max"])
            {
                BC.camera.Parameters[PLCamera.ExposureTime].SetValue(Convert.ToDouble(ExposureTime.Text));
            }
            else
            {
                BC.camera.Parameters[PLCamera.ExposureTime].SetValue(camera_parameter["ExposureTime Min"]);
            }
            if (Convert.ToDouble(Gamma.Text) >= camera_parameter["Gamma Min"] && Convert.ToDouble(Gamma.Text) <= camera_parameter["Gamma Max"])
            {
                BC.camera.Parameters[PLCamera.Gamma].SetValue(Convert.ToDouble(Gamma.Text));
            }
            else
            {
                BC.camera.Parameters[PLCamera.Gamma].SetValue(camera_parameter["Gamma Min"]);
            }
        }

        #region Config
        private void LoadConfig()
        {
            List<Parameter> Parameter_info = Config.Load();
            Gain.Text = Parameter_info[0].Gain_val.ToString();
            ExposureTime.Text = Parameter_info[0].ExposureTime_val.ToString();
            Gamma.Text = Parameter_info[0].Gamma_val.ToString();
            Save_Image_Path.Text = Parameter_info[0].Save_Image_Path_val;
        }

        private void SaveConfig()
        {
            List<Parameter> Parameter_config = new List<Parameter>()
                        {
                            new Parameter()
                            {
                               Gain_val=Convert.ToDouble(Gain.Text),
                               ExposureTime_val=Convert.ToDouble(ExposureTime.Text),
                               Gamma_val=Convert.ToDouble(Gamma.Text),
                               Save_Image_Path_val = Save_Image_Path.Text
                            }
                        };
            Config.Save(Parameter_config);
            Logger.WriteLog("Save config!", 1, richTextBoxGeneral);
        }
        #endregion
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CameraInit();
            LoadConfig();
        }
        BaseLogRecord Logger = new BaseLogRecord();
        BaseConfig<Parameter> Config = new BaseConfig<Parameter>();
        Basler BC = new Basler();
        bool Cam_IsOpen = false;
        #endregion

        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Save_Image):
                    {
                        if (Cam_IsOpen)
                        {
                            if (!string.IsNullOrEmpty(Save_Image_Path.Text))
                            {
                                if (Directory.Exists(Save_Image_Path.Text))
                                {
                                    BC.image_storage_path = Save_Image_Path.Text;
                                    BC.save_img = true;
                                    Logger.WriteLog("Save the Image!", 1, richTextBoxGeneral);
                                }
                                else
                                {
                                    MessageBox.Show("Image storage path is invalid!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Image storage path is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Camera doesn't turn on!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        
                        break;
                    }
            }
        }

        #region Continue Acquisition ToggleButton
        private void Continue_Acquisition_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                BC.OpenCamera();
                CameraParameterInit();
                BC.ContinueAcquisition();
                ChangeIcon(Continue_Acquisition_Icon, @"Icon\Stop.png", "Stop Acquisition", "Turn on the camera!");
                Cam_IsOpen = true;
            }
            catch
            {
                MessageBox.Show("Camera initializing failed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }
        private void Continue_Acquisition_Unchecked(object sender, RoutedEventArgs e)
        {
            BC.StopAcquisition();
            ChangeIcon(Continue_Acquisition_Icon, @"Icon\Start.png", "Continue Acquisition", "Turn off the camera!");
            Cam_IsOpen = false;
        }
        #endregion
        #endregion

        #region
        private void Parameter_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Set_Camera_Parameter):
                    {
                        if (Cam_IsOpen)
                        {
                            BC.camera.Parameters[PLCamera.Gain].SetValue(Convert.ToDouble(Gain.Text));
                            BC.camera.Parameters[PLCamera.ExposureTime].SetValue(Convert.ToDouble(ExposureTime.Text));
                            BC.camera.Parameters[PLCamera.Gamma].SetValue(Convert.ToDouble(Gamma.Text));
                            //BC.camera.Parameters[PLCamera.Width].SetValue(300);
                            //BC.camera.Parameters[PLCamera.Height].SetValue(300);
                            Logger.WriteLog("Set the camera parameter!", 1, richTextBoxGeneral);
                        }
                        else
                        {
                            MessageBox.Show("Camera doesn't turn on!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    }
                case nameof(Save_Config):
                    {
                        SaveConfig();
                        break;
                    }
                case nameof(Open_Image_Storage_Folder):
                    {
                        System.Windows.Forms.FolderBrowserDialog image_storage_path = new System.Windows.Forms.FolderBrowserDialog();
                        image_storage_path.Description = "Choose Save Image Path";
                        image_storage_path.ShowDialog();
                        Save_Image_Path.Text = image_storage_path.SelectedPath;
                        break;
                    }
            }
        }
        #endregion


    }
}
