using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Net;
using System.Threading;
using ERMSWeb.Models;
using ERMSWeb.Utils;

namespace ERMSWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Constructors

        public AccountController()
        {

        }
        #endregion

        #region Methods

        [AllowAnonymous]
        public ActionResult LogOn()
        {
            #region BEBUG MODE
            #if DEBUG
            ViewBag.username = "Admin";
            ViewBag.password = "123456";

            #endif
            #endregion

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult LogOn(string loginName, string password)
        {
            return View();
        }
        #endregion

        #region Private methods
        private async Task<LogOnModel> ValidateUser(string userName, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    return new LogOnModel()
                    {
                        CheckState = CheckState.EmptyUserName,
                        Message = "Please enter the valid user name for your account."
                    };
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return new LogOnModel()
                    {
                        CheckState = CheckState.EmptyPassword,
                        Message = "Please enter your password."
                    };
                }


                var result = await GenerateAuthToken(new UserDto { UniqueID = Guid.NewGuid().ToString(), LoginName = userName, Password = password });
                if (result.CheckState == CheckState.Success)
                {
                    return new LogOnModel()
                    {
                        CheckState = CheckState.Success,
                        User = new UserDto { UniqueID = Guid.NewGuid().ToString(), LoginName = userName, Password = password },
                        Message = "login success"
                    };
                }
                else
                {
                    return result;
                }
            }
            catch (HttpException ex)
            {
                return new LogOnModel()
                {
                    CheckState = CheckState.UnKnown,
                    Message = "Generate auth token failed, please check if WebApi is running."
                };
            }
            catch (Exception ex)
            {
                return new LogOnModel()
                {
                    CheckState = CheckState.UnKnown,
                    Message = "Unknown error occures  "
                };
            }

        }

        private async Task<LogOnModel> GenerateAuthToken(UserDto user)
        {
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "password");
                parameters.Add("username", user.LoginName);
                parameters.Add("password", user.Password);
                parameters.Add("client_id", AppSetting.ClientId);
                parameters.Add("client_secret", AppSetting.ClientSecret);

                var tokenRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri(AppSetting.WebApiHostUrl + "/Token"),
                    Method = HttpMethod.Post,
                    Content = new FormUrlEncodedContent(parameters)
                };

                tokenRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    var tokenResponse = await client.SendAsync(tokenRequest);

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        var token = tokenResponse.Content.ReadAsStringAsync().Result;

                        var needChangePassword = tokenResponse.Headers.Contains("needChangePassword");
                        var isExternalUser = tokenResponse.Headers.Contains("isExternalUser");
                        IEnumerable<string> projectCustomerName;
                        tokenResponse.Headers.TryGetValues("projectCustomerName", out projectCustomerName);

                        IEnumerable<string> passwordHash;
                        tokenResponse.Headers.TryGetValues("passwordHash", out passwordHash);

                        IEnumerable<string> userIDs;
                        tokenResponse.Headers.TryGetValues("userID", out userIDs);


                        // append web api host url to result
                        dynamic tokenObj = JsonConvert.DeserializeObject(token);
                        tokenObj.api_host = AppSetting.WebApiHostUrl;
                        tokenObj.vat_api_host = AppSetting.VatWebApiHostUrl;
                        tokenObj.version = AppSetting.Version;
                        tokenObj.user_name = Uri.EscapeDataString(user.LoginName);
                        tokenObj.local_name = Uri.EscapeDataString(user.LoginName);
                        tokenObj.need_change_password = needChangePassword;
                        tokenObj.is_external_user = isExternalUser;

                        if (projectCustomerName != null)
                        {
                            tokenObj.project_customer_name = projectCustomerName.FirstOrDefault();
                        }

                        if (passwordHash != null)
                        {
                            tokenObj.password_hash = passwordHash.FirstOrDefault();
                        }

                        if (userIDs != null)
                        {
                            tokenObj.user_id = userIDs.FirstOrDefault();
                        }

                        // set ApiToken to cookies
                        var cookie = new HttpCookie("AtmsTeslaApiToken", JsonConvert.SerializeObject(tokenObj)) { Expires = DateTime.MaxValue, HttpOnly = false };
                        Response.Cookies.Add(cookie);

                        return new LogOnModel() { CheckState = CheckState.Success };
                    }
                    else
                    {
                        var errorInfo = tokenResponse.Content.ReadAsStringAsync().Result;
                        dynamic errorObj = JsonConvert.DeserializeObject(errorInfo);

                        return new LogOnModel() { CheckState = errorObj.error, Message = errorObj.error_description };
                    }
                }
                catch (Exception)
                {

                }

                return null;
            }
        }

        #endregion
    }
}
