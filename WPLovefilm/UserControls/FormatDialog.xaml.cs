using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WPLovefilm.Models;
using WPLovefilm.Service;

namespace WPLovefilm.UserControls
{
    public partial class FormatDialog : UserControl
    {
        public List<LFFormat> FormatList;

        public delegate void SettingsSavedDelegate();
        public event SettingsSavedDelegate FormatSettingsSaved;

        public FormatDialog()
        {
            InitializeComponent();

            FormatList = Format.Instance.GetFormatList();

            FormatListBox.ItemsSource = FormatList;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            Format.Instance.SetFormats(FormatList);

            if (this.FormatSettingsSaved != null)
            {
                this.FormatSettingsSaved();
            }
        }
    }
}
