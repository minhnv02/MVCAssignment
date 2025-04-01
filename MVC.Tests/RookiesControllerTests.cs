using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MVC.WebApp.Controllers;
using MVC.WebApp.Models;
using MVC.WebApp.Repositories;

namespace MVC.Tests
{
    public class RookiesControllerTests
    {
        private Mock<ILogger<RookiesController>> _loggerMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private RookiesController _controller;
        private List<Person> _people;

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<RookiesController>>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _people = new List<Person>
            {
                new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = GenderType.Male,
                    DateOfBirth = new DateTime(1990, 5, 15)
                },
                new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Gender = GenderType.Female,
                    DateOfBirth = new DateTime(1985, 10, 20)
                },
            };
            _personRepositoryMock.Setup(repo => repo.GetAll()).Returns(_people);
            _controller = new RookiesController(_loggerMock.Object, _personRepositoryMock.Object);
        }

        [Test]
        public void Index_ReturnsView()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Persons_ReturnsViewResultWithPeople()
        {
            // Act
            var result = _controller.Persons();

            // Assert
            // Must return the view result
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData.Model);
            var model = (List<Person>)viewResult.ViewData.Model;
            Assert.AreEqual(_people.Count, model.Count());
        }

        [Test]
        public void Males_ReturnsViewResultWithMales()
        {
            // Act
            var result = _controller.Males();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData.Model);
            var model = (List<Person>)viewResult.ViewData.Model;
            Assert.AreEqual(_people.Count(p => p.Gender == GenderType.Male), model.Count());
        }

        [Test]
        public void Oldest_ReturnsViewResultWithOldestPerson()
        {
            // Act
            var result = _controller.Oldest();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<Person>(viewResult.ViewData.Model);
            var model = (Person)viewResult.ViewData.Model;
            Assert.AreEqual(_people.OrderBy(p => p.DateOfBirth.Year).First(), model);
        }

        [Test]
        public void FullNames_ReturnsViewResultWithPeople()
        {
            // Act
            var result = _controller.FullNames();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData.Model);
            var model = (List<Person>)viewResult.ViewData.Model;
            Assert.AreEqual(_people.Count, model.Count());
        }

        [TestCase(1985)]
        [TestCase(1990)]
        [TestCase(2000)]
        public void AroundYear_ReturnsViewResultWithPeopleGroupedByYear(int year)
        {
            // Act
            var result = _controller.AroundYear(year);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData["Before"]);
            var beforeModel = (List<Person>)viewResult.ViewData["Before"];
            // For inYearModel and afterModel
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData["InYear"]);
            var inYearModel = (List<Person>)viewResult.ViewData["InYear"];
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData["After"]);
            var afterModel = (List<Person>)viewResult.ViewData["After"];

            Assert.AreEqual(_people.Count(p => p.DateOfBirth.Year < year), beforeModel.Count());
            Assert.AreEqual(_people.Count(p => p.DateOfBirth.Year == year), inYearModel.Count());
            Assert.AreEqual(_people.Count(p => p.DateOfBirth.Year > year), afterModel.Count());
        }

        [Test]
        public void Export_ReturnsFileContentResult()
        {
            // Act
            var result = _controller.Export();

            // Assert
            Assert.IsInstanceOf<FileStreamResult>(result);
        }

        [Test]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Create_POSTValidModel_RedirectsToPersons()
        {
            // Arrange
            var newPerson = new Person
            {
                FirstName = "Jimmy",
                LastName = "Logan",
                Gender = GenderType.Male,
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            // Act
            var result = _controller.Create(newPerson);

            // Assert
            _personRepositoryMock.Verify(repo => repo.Create(newPerson), Times.Once);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Persons", redirectResult.ActionName);
        }

        [Test]
        public void Create_POSTInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("FirstName", "FirstName is required.");
            var newPerson = new Person
            {
                Gender = GenderType.Male,
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            // Act
            var result = _controller.Create(newPerson);

            // Assert
            _personRepositoryMock.Verify(repo => repo.Create(It.IsAny<Person>()), Times.Never);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.AreSame(newPerson, viewResult.ViewData.Model);
        }

        [Test]
        public void Update_GETReturnsViewResultWithPerson()
        {
            // Arrange
            var personId = _people.First().Id;

            // Act
            var result = _controller.Update(personId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<Person>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            Assert.AreEqual(_people.First(p => p.Id == personId), model);
        }

        [Test]
        public void Update_POSTValidModel_RedirectsToPersons()
        {
            // Arrange
            var personToUpdate = _people.First();
            personToUpdate.FirstName = "Updated Name";

            // Act
            var result = _controller.Update(personToUpdate);

            // Assert
            _personRepositoryMock.Verify(repo => repo.Update(personToUpdate), Times.Once);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("Persons", (result as RedirectToActionResult).ActionName);
        }

        [Test]
        public void Update_POSTInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("FirstName", "FirstName is required.");
            var personToUpdate = _people.First();
            personToUpdate.FirstName = string.Empty;

            // Act
            var result = _controller.Update(personToUpdate);

            // Assert
            _personRepositoryMock.Verify(repo => repo.Update(It.IsAny<Person>()), Times.Never);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        [Test]
        public void Delete_GET_ReturnsViewResultWithId()
        {
            // Arrange
            var personId = _people.First().Id;

            // Act
            var result = _controller.Delete(personId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<Guid>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            Assert.AreEqual(personId, model);
        }

        [Test]
        public void DeleteConfirmed_POST_DeletesPersonAndReturnsViewWithName()
        {
            // Arrange
            var personToDelete = _people.First();

            // Act
            var result = _controller.DeleteConfirmed(personToDelete.Id);

            // Assert
            _personRepositoryMock.Verify(repo => repo.Delete(personToDelete.Id), Times.Once);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<string>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            Assert.AreEqual(personToDelete.FullName, model);
        }

        [Test]
        public void Detail_ReturnsViewResultWithPerson()
        {
            // Arrange
            var personId = _people.First().Id;

            // Act
            var result = _controller.Detail(personId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsAssignableFrom<Person>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            Assert.AreEqual(_people.First(p => p.Id == personId), model);
        }
    }
}
