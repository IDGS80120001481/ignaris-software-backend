using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LignarisBack.Controllers
{
    public class TokenService : Controller
    {
        // GET: TokenService
        public ActionResult Index()
        {
            return View();
        }

        // GET: TokenService/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TokenService/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TokenService/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TokenService/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TokenService/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TokenService/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TokenService/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
