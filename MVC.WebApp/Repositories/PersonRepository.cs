using System.Globalization;
using CsvHelper;
using MVC.WebApp.Models;

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
    }
}
