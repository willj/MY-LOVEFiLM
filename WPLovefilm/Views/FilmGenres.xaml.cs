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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WPLovefilm.Views
{
    public partial class FilmGenres : PhoneApplicationPage
    {
        public FilmGenres()
        {
            InitializeComponent();

            App.SetSystemTray(this);
        }
    }
}