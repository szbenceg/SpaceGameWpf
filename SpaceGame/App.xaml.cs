using SpaceGame.View;
using SpaceGame.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SpaceGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            SpaceViewModel viewModel = new SpaceViewModel();
            MainWindow win = new MainWindow();
            win.DataContext = viewModel;

            win.Show();
        }

    }
}
