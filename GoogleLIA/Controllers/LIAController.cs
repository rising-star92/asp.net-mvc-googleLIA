using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoogleLIA.Models;

namespace GoogleLIA.Controllers
{
    public class LIAController : Controller
    {
        // GET: LIA    
        public ActionResult CustomRules()
        {
            return View();
        }

        public ActionResult MapFields()
        {
            return View();
        }
    }
}