using Microsoft.AspNetCore.Mvc;
using MVC.WebApp.Models;
using MVC.WebApp.Repositories;
using OfficeOpenXml;

namespace MVC.WebApp.Controllers
{
    [Route("NashTech/[controller]/[action]")]
    public class RookiesController : Controller
    {
        private readonly ILogger<RookiesController> _logger;
        private readonly IPersonRepository _personRepository;
        private List<Person> _people;
        public RookiesController(ILogger<RookiesController> logger, IPersonRepository personRepository)
        {
            _logger = logger;
            _personRepository = personRepository;
            _people = _personRepository.GetAll();
        }
        public IActionResult Index() => View();

        public IActionResult Persons() => View(_personRepository.GetAll());

        public IActionResult Males() => View(_personRepository.Males());

        public IActionResult Oldest() => View(_personRepository.Oldest());

        public IActionResult FullNames() => View(_people);
        //public IActionResult AroundYear(int year = 0)
        //{
        //    if (year < 0)
        //    {
        //        return RedirectToAction("AroundYear");
        //    }

        //    List<Person> peopleAroundYear;
        //    if (year != 0)
        //    {
        //        peopleAroundYear = _personRepository.AroundYear(year);
        //    }
        //    else
        //    {
        //        peopleAroundYear = _personRepository.GetAll(); 
        //    }

        //    if (year != 0)
        //    {
        //        ViewBag.Before = peopleAroundYear.Where(p => p.DateOfBirth.Year < year).ToList();
        //        ViewBag.InYear = peopleAroundYear.Where(p => p.DateOfBirth.Year == year).ToList();
        //        ViewBag.After = peopleAroundYear.Where(p => p.DateOfBirth.Year > year).ToList();
        //    }
        //    else
        //    {
        //        ViewBag.Before = new List<Person>();
        //        ViewBag.InYear = new List<Person>();
        //        ViewBag.After = new List<Person>();
        //    }

        //    return View(year);
        //}
        public IActionResult AroundYear(int year = 0)
        {
            if (year < 0)
            {
                return RedirectToAction("AroundYear");
            }

            List<Person> peopleAroundYear = GetPeopleAroundYear(year);

            if (year != 0)
            {
                PopulateViewBagWithFilteredData(peopleAroundYear, year);
            }
            else
            {
                SetEmptyViewBags();
            }

            return View(year);
        }

        private List<Person> GetPeopleAroundYear(int year)
        {
            return year != 0
                ? _personRepository.AroundYear(year)
                : _personRepository.GetAll();
        }

        private void PopulateViewBagWithFilteredData(List<Person> people, int year)
        {
            ViewBag.Before = people.Where(p => p.DateOfBirth.Year < year).ToList();
            ViewBag.InYear = people.Where(p => p.DateOfBirth.Year == year).ToList();
            ViewBag.After = people.Where(p => p.DateOfBirth.Year > year).ToList();
        }

        private void SetEmptyViewBags()
        {
            ViewBag.Before = new List<Person>();
            ViewBag.InYear = new List<Person>();
            ViewBag.After = new List<Person>();
        }

        public IActionResult Export()
        {
            var stream = _personRepository.ExportToExcel();
            string excelFileName = $"Persons_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Person person)
        {
            if (ModelState.IsValid)
            {
                _personRepository.Create(person);
            }
            else
            {
                return View(person);
            }

            return RedirectToAction("Persons");
        }

        [HttpGet]
        public IActionResult Update(Guid id)
        {
            var person = _people.FirstOrDefault(p => p.Id == id);

            return View(person);
        }
        [HttpPost]
        public IActionResult Update(Person person)
        {
            if (ModelState.IsValid)
            {
                _personRepository.Update(person);
            }
            return RedirectToAction("Persons");
        }

        public IActionResult Detail(Guid id)
        {
            var person = _people.FirstOrDefault(p => p.Id == id);
            return View(person);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            return View(id);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var fullName = _people.FirstOrDefault(p => p.Id == id)?.FullName ?? string.Empty;
            _personRepository.Delete(id);
            return View("DeleteSuccess", fullName);
        }
        [HttpDelete]
        public IActionResult DeleteSuccess(string fullName)
        {
            return View(fullName);
        }
    }
}