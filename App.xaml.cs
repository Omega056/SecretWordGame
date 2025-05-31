using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SecretGame
{
    public partial class App : Application
    {
        public App()
        {
            // Ensure the data directory is set to the application folder
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}