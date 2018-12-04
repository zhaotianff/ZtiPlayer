using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZtiPlayer.Utils
{
    public static class LanguageManager
    {
        public static string DetectLanguage()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public static void ChangeLanguage(string name)
        {
            string lang = "";
            switch(name)
            {
                case "en-US":
                    lang = "en-US";
                    break;
                case "zh-CN":
                    lang = "zh-CN";
                    break;
                default:
                    lang = "zh-CN";
                    break;
            }

            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("Resources/Str/" + lang + ".xaml",UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = rd;
        }
    }
}
