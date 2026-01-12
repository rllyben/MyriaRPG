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
        private static Frame IngameNavigationFrame = new();
        private static Frame GameNavigationFrame = new();
        private static Frame IngameMenueNavigationFrame = new();
        private static Frame IngameSettingsNavigationFrame = new();
        private static Page GameRoomPage = new();
        private static Page GameCombatPage = new();

        public static bool SetNavigationFrame(Frame navFrame, int frameType)
        {
            switch (frameType)
            {
                case 0: MainNavigationFrame = navFrame; MainNavigationFrame.Navigate(new Page_StartupMenue()); return true;
                case 1: PageNavigationFrame = navFrame; return true;
                case 2: SettingsNavigationFrame = navFrame; return true;
                case 3: IngameNavigationFrame = navFrame; return true;
                case 4: GameNavigationFrame = navFrame; return true;
                case 5: IngameMenueNavigationFrame = navFrame; return true;
                case 6: IngameSettingsNavigationFrame = navFrame; return true;
                default: return false;
            }

        }
        public static bool RegisterGamePage(Page page, int pageType)
        {
            switch (pageType)
            {
                case 0: GameRoomPage = page; return true;
                case 1: GameCombatPage = page; return true;
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
        public static bool NavigateIngameWindow(Page page)
        {
            try
            {
                IngameNavigationFrame.Navigate(page);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool NavigateGamePage(Page page)
        {
            try
            {
                GameNavigationFrame.Navigate(page);
            }
            catch
            { 
                return false; 
            }
            return true;
        }
        public static bool NavigateIngameMenue(Page page)
        {
            try
            {
                IngameMenueNavigationFrame.Navigate(page);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool NavigateIngameSettings(Page page)
        {
            try
            {
                IngameSettingsNavigationFrame.Navigate(page);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool NavigateGamePageToRegister(int id)
        {
            try
            {
                switch (id)
                {
                    case 0: GameNavigationFrame.Navigate(GameRoomPage); return true;
                    case 1: GameNavigationFrame.Navigate(GameCombatPage); return true;
                    default: return false;
                }

            }
            catch 
            { 
                return false; 
            }

        }

    }

}
