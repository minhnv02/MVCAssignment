using CsvHelper;
using MVC.WebApp.Models;
using MVC.WebApp.Repositories;

namespace MVC.Tests
{
    public class PersonRepositoryTests
    {
        private const string TestDataFileName = "Person_data.csv";
        private string _testDataPath;
        private List<Person> _testData;
        private PersonRepository _repository;

        [OneTimeSetUp]
        public void SetUp()
        {
            // Set up test data path
            _testDataPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "Data",
                TestDataFileName
            );
            Directory.CreateDirectory(Path.GetDirectoryName(_testDataPath));

            // Set up test data
            _testData = new List<Person>
            {
                new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = GenderType.Male,
                    DateOfBirth = new DateTime(1990, 5, 15),
                    PhoneNumber = "1234567890",
                    BirthPlace = "New York",
                    IsGraduated = true
                },
                new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Gender = GenderType.Female,
                    DateOfBirth = new DateTime(1985, 10, 20),
                    PhoneNumber = "0987654321",
                    BirthPlace = "Los Angeles",
                    IsGraduated = false
                }
            };

            // Create test CSV file
            using (var writer = new StreamWriter(_testDataPath))
            using (
                var csvWriter = new CsvWriter(
                    writer,
                    System.Globalization.CultureInfo.InvariantCulture
                )
            )
            {
                csvWriter.WriteRecords(_testData);
            }

            _repository = new PersonRepository();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Clean up test CSV file
            File.Delete(_testDataPath);
        }

        [Test]
        public void GetAll_ReturnsAllPersons()
        {
            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.AreEqual(_testData.Count, result.Count);
        }

        [Test]
        public void Create_AddsNewPersonToRepository()
        {
            // Arrange
            var newPerson = new Person
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Gender = GenderType.Male,
                DateOfBirth = new DateTime(1995, 3, 1),
                PhoneNumber = "5551234567",
                BirthPlace = "Chicago",
                IsGraduated = true
            };

            // Act
            _repository.Create(newPerson);
            var result = _repository.GetAll();

            // Assert
            Assert.AreEqual(_testData.Count + 1, result.Count);
        }

        [Test]
        public void Update_UpdatesPersonFromRepository()
        {
            // Arrange
            var people = _repository.GetAll();
            var personToUpdate = people.First();
            personToUpdate.FirstName = "Changed";

            // Act
            _repository.Update(personToUpdate);
            var result = people.Where(p => p.Id == personToUpdate.Id).FirstOrDefault();

            // Assert
            Assert.AreEqual(result.FirstName, personToUpdate.FirstName);
        }


        [Test]
        public void Delete_RemovesPersonFromRepository()
        {
            // Arrange
            var personToDelete = _testData.First();

            // Act
            _repository.Delete(personToDelete.Id);
            var result = _repository.GetAll();

            // Assert
            Assert.AreEqual(_testData.Count - 1, result.Count - 1);
        }
    }
}
