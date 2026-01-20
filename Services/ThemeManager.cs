using System.Windows;

namespace MyriaRPG.Services
{
    public static class ThemeManager
    {
        private static readonly Uri LightUri = new("Assets/Light.xaml", UriKind.Relative);
        private static readonly Uri DarkUri = new("Assets/Dark.xaml", UriKind.Relative);

        public static bool IsDark { get; private set; }

        public static void Apply(bool dark)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var newDict = new ResourceDictionary { Source = dark ? DarkUri : LightUri };

            // replace the first theme dictionary, or add if missing
            if (dictionaries.Count > 0)
                dictionaries[0] = newDict;
            else
                dictionaries.Add(newDict);

            IsDark = dark;
        }

    }

}
