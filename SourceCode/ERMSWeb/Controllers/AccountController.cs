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
    }
}
