using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Demo.ChatSirnalR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Demo.ChatSirnalR.Controllers
{
    public class HomeController : MainController
    {
        private ApplicationDbContext _dbContext;

        protected HomeController()
        {
        }

        protected HomeController(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        private ApplicationDbContext DbContext
        {
            get { return _dbContext ?? HttpContext.GetOwinContext().GetUserManager<ApplicationDbContext>(); }
            set { _dbContext = value; }
        }

        public ActionResult Index()
        {
            return View();
        }

    }
}