using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ERMSWeb.Utils
{
    public static class AppSetting
    {
        public static string GetAppSettingString(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString();
        }

        private static string _Version;
        public static string Version
        {
            get
            {
                if (String.IsNullOrEmpty(_Version))
                {
                    var locStr = Assembly.GetExecutingAssembly().Location;
                    _Version = FileVersionInfo.GetVersionInfo(locStr).ProductVersion;
                }

                return _Version;
            }
        }


        private static string _WebApiHostUrl;

        public static string WebApiHostUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_WebApiHostUrl))
                {
                    _WebApiHostUrl = GetAppSettingString("WebApiHostUrl");
                }

                return _WebApiHostUrl;
            }
        }


        private static string _VatWebApiHostUrl;
        public static string VatWebApiHostUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_VatWebApiHostUrl))
                {
                    _VatWebApiHostUrl = GetAppSettingString("VatWebApiHostUrl");
                }

                return _VatWebApiHostUrl;
            }
        }


        private static string _ClientId;
        public static string ClientId
        {
            get
            {
                if (string.IsNullOrEmpty(_ClientId))
                {
                    _ClientId = ConfigurationManager.AppSettings["clientId"];
                }

                return _ClientId;
            }
        }

        private static string _ClientSecret;
        public static string ClientSecret
        {
            get
            {
                if (string.IsNullOrEmpty(_ClientSecret))
                {
                    _ClientSecret = ConfigurationManager.AppSettings["clientSecret"];
                }

                return _ClientSecret;
            }
        }
    }
}