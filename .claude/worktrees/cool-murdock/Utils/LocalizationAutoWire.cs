using MyriaRPG.Model;
using System.Reflection;
using System.Windows;

namespace MyriaRPG.Utils
{
    public static class LocalizationAutoWire
    {
        public static void Wire(object vm)
        {
            void Refresh()
            {
                var t = vm.GetType();
                foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (p.PropertyType != typeof(string) || !p.CanWrite) continue;
                    var attr = p.GetCustomAttribute<LocalizedKeyAttribute>();
                    if (attr is null) continue;

                    var value = MyriaLib.Systems.Localization.T(attr.Key); // your T(...) :contentReference[oaicite:2]{index=2}
                    p.SetValue(vm, value);
                }

            }

            MyriaLib.Systems.Localization.LanguageChanged += (_, __) =>
                Application.Current.Dispatcher.Invoke(Refresh);
            Refresh();
        }

    }

}
