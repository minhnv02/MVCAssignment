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

        public IActionResult Persons() => View(_people);

        public IActionResult Males() => View(_people.Where(p => p.Gender == GenderType.Male).ToList());

        public IActionResult Oldest() => View(_people.OrderBy(p => p.DateOfBirth.Year).FirstOrDefault());

        public IActionResult FullNames() => View(_people);
        public IActionResult AroundYear(int year = 0)
        {
            if (year != 0)
            {
                var before = _people.Where(p => p.DateOfBirth.Year < year).ToList();
                var inYear = _people.Where(p => p.DateOfBirth.Year == year).ToList();
                var after = _people.Where(p => p.DateOfBirth.Year > year).ToList();

                ViewBag.Before = before;
                ViewBag.InYear = inYear;
                ViewBag.After = after;
            }

            return View(year);
        }

        public IActionResult Export()
        {
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.Add("Persons");

            worksheet.Cells.LoadFromCollection(_people, true);

            // Change format of date column
            worksheet.Column(4).Style.Numberformat.Format = "dd/mm/yyyy";
            package.Save();

            stream.Position = 0;
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
            var fullName = _people.FirstOrDefault(p => p.Id == id).FullName;
            _personRepository.Delete(id);
            return View("DeleteSuccess", fullName);
        }

        public IActionResult DeleteSuccess(string fullName)
        {
            return View(fullName);
        }
    }
}