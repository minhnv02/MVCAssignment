using System.Globalization;
using CsvHelper;
using MVC.WebApp.Models;
using OfficeOpenXml;

namespace MVC.WebApp.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly List<Person> _people;

        public PersonRepository()
        {
            _people = LoadPersonsFromCsv();
        }

        private static List<Person> LoadPersonsFromCsv()
        {
            try
            {
                using var reader = new StreamReader("./Data/Person_data.csv");
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<Person>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading persons from CSV: {ex.Message}");

                return [];
            }
        }

        private void SavePersonsToCsv()
        {
            try
            {
                using var writer = new StreamWriter("./Data/Person_data.csv");
                using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csvWriter.WriteRecords(_people);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving persons to CSV: {ex.Message}");
            }
        }

        public void Create(Person person)
        {
            _people.Add(
                new Person()
                {
                    Id = new Guid(),
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Gender = person.Gender,
                    DateOfBirth = person.DateOfBirth,
                    PhoneNumber = person.PhoneNumber,
                    BirthPlace = person.BirthPlace,
                    IsGraduated = person.IsGraduated
                }
            );
            SavePersonsToCsv();
        }

        public void Delete(Guid id)
        {
            var person = _people.FirstOrDefault(p => p.Id == id);
            if (person != null)
            {
                _people.Remove(person);
            }
            SavePersonsToCsv();
        }

        public List<Person> GetAll() => _people;

        public void Update(Person person)
        {
            var personToUpdate = _people.FirstOrDefault(p => p.Id == person.Id);

            if (personToUpdate == null)
            {
                return;
            }
            personToUpdate.FirstName = person.FirstName;
            personToUpdate.LastName = person.LastName;
            personToUpdate.Gender = person.Gender;
            personToUpdate.DateOfBirth = person.DateOfBirth;
            personToUpdate.PhoneNumber = person.PhoneNumber;
            personToUpdate.BirthPlace = person.BirthPlace;
            personToUpdate.IsGraduated = person.IsGraduated;
            SavePersonsToCsv();
        }

        public List<Person> Males()
        {
            return _people.Where(p => p.Gender == GenderType.Male).ToList();
        }

        public Person Oldest()
        {
            return _people.OrderBy(p => p.DateOfBirth.Year).DefaultIfEmpty(new Person()).First();
        }

        public List<Person> AroundYear(int year)
        {
            if (year < 0)
            {
                throw new ArgumentException("Year must be a positive number.");
            }

            var before = _people.Where(p => p.DateOfBirth.Year < year).ToList();
            var inYear = _people.Where(p => p.DateOfBirth.Year == year).ToList();
            var after = _people.Where(p => p.DateOfBirth.Year > year).ToList();

            return before.Concat(inYear).Concat(after).ToList();
        }

        public MemoryStream ExportToExcel()
        {
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.Add("Persons");

            worksheet.Cells.LoadFromCollection(_people, true);

            // Change format of date column (assuming DateOfBirth is the 4th column)
            if (worksheet.Dimension != null && worksheet.Dimension.Columns >= 4)
            {
                worksheet.Column(4).Style.Numberformat.Format = "dd/mm/yyyy";
            }

            package.Save();
            stream.Position = 0;
            return stream;
        }
    }
}
