using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using MyriaRPG.View.Pages;

namespace MyriaRPG.Services
{
    public class Navigation
    {
        private static Frame MainNavigationFrame = new();
        private static Frame PageNavigationFrame = new();
        private static Frame SettingsNavigationFrame = new();

        public static bool SetNavigationFrame(Frame navFrame, int page)
        {
            switch (page)
            {
                case 0: MainNavigationFrame = navFrame; MainNavigationFrame.Navigate(new Page_StartupMenue()); return true;
                case 1: PageNavigationFrame = navFrame; return true;
                case 2: SettingsNavigationFrame = navFrame; return true;
                default: return false;
            }

        }

        public static bool NavigateMain(Page page)
        {
            try
            {
                MainNavigationFrame.Navigate(page);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool NavigateStartup(Page page)
        {
            try
            {
                PageNavigationFrame.Navigate(page);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool NavigateSettings(Page page)
        {
            try 
            {
                SettingsNavigationFrame.Navigate(page);
            }
            catch
            {
                return false;
            }
            return true;
        }

    }

}
