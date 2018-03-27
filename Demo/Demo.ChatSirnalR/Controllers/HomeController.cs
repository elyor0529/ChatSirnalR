using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Demo.ChatSirnalR.Hubs;
using Demo.ChatSirnalR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;

namespace Demo.ChatSirnalR.Controllers
{
    public class HomeController : MainController
    {
        private ApplicationDbContext _dbContext;

        public HomeController()
        {
        }

        public HomeController(ApplicationDbContext dbContext)
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

        public ActionResult MessageHistories(string id)
        {
            try
            {
                var user = DbContext.Users.Find(id);
                if (user == null)
                    throw new Exception($"User#{id} is unavaliable!");

                var histories = DbContext.MessageHistories
                    .Where(w => w.FromUserId == user.Id || w.ToUserId == user.Id)
                    .OrderBy(a => a.Date)
                    .AsEnumerable()
                    .Select(s => new
                    {
                        date = s.Date.ToLongDateString() + " " + s.Date.ToShortTimeString(),
                        text = s.Content,
                        id = s.FromUserId
                    }).ToArray();

                return Json(new
                {
                    success = true,
                    result = histories
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                return Json(new
                {
                    success = false,
                    result = exception.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}