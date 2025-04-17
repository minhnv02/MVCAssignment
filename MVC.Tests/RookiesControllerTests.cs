using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MVC.WebApp.Controllers;
using MVC.WebApp.Models;
using MVC.WebApp.Repositories;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace MVC.Tests
{
    public class RookiesControllerTests
    {
        private Mock<ILogger<RookiesController>> _loggerMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private RookiesController _controller;
        private List<Person> _people;

        public RookiesControllerTests()
        {
            _personRepositoryMock = new Mock<IPersonRepository>();
        }

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
            _personRepositoryMock.Setup(repo => repo.Males()).Returns(_people.Where(p => p.Gender == GenderType.Male).ToList());
            _personRepositoryMock.Setup(repo => repo.Oldest()).Returns(_people.OrderBy(p => p.DateOfBirth.Year).First());
            _personRepositoryMock.Setup(repo => repo.AroundYear(It.IsAny<int>())).Returns<int>(
              year => _people.Where(p => p.DateOfBirth.Year >= year - 1 && p.DateOfBirth.Year <= year + 1).ToList()
            );
            _personRepositoryMock.Setup(repo => repo.ExportToExcel()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("fake excel content")));
            _controller = new RookiesController(_loggerMock.Object, _personRepositoryMock.Object);
        }

        [Test]
        public void Index_ReturnsView()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Persons_ReturnsViewResultWithPeople()
        {
            // Act
            var result = _controller.Persons();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.ViewData.Model, Is.Not.Null);
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData.Model);
            var model = (List<Person>)viewResult.ViewData.Model;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Count(), Is.EqualTo(_people.Count));
        }

        [Test]
        public void Males_ReturnsViewResultWithMales()
        {
            // Act
            var result = _controller.Males();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.ViewData.Model, Is.Not.Null);
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData.Model);
            var model = (List<Person>)viewResult.ViewData.Model;
            Assert.That(model, Is.Not.Null);  
            Assert.That(model.Count(), Is.EqualTo(_people.Count(p => p.Gender == GenderType.Male)));
        }

        [Test]
        public void Oldest_ReturnsViewResultWithOldestPerson()
        {
            // Act
            var result = _controller.Oldest();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsAssignableFrom<Person>(viewResult.ViewData.Model);
            var model = (Person)viewResult.ViewData.Model;
            Assert.That(model, Is.EqualTo(_people.OrderBy(p => p.DateOfBirth.Year).First()));
        }

        [Test]
        public void FullNames_ReturnsViewResultWithPeople()
        {
            // Act
            var result = _controller.FullNames();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult?.ViewData.Model, Is.Not.Null);
            Assert.IsAssignableFrom<List<Person>>(viewResult.ViewData.Model);
            var model = (List<Person>)viewResult.ViewData.Model;
            Assert.That(model.Count(), Is.EqualTo(_people.Count()));
        }

        [Test]
        public void AroundYear_WithNegativeYear_ShouldRedirectToSameActionWithoutParameters()
        {
            // Arrange
            int year = -5;

            // Act
            var result = _controller.AroundYear(year);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("AroundYear"));
            Assert.That(redirectResult.RouteValues, Is.Null);
        }

        [Test]
        public void AroundYear_WithZeroYear_ShouldReturnAllPeopleWithEmptyViewBags()
        {
            // Arrange
            int year = 0;

            // Act
            var result = _controller.AroundYear(year);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.EqualTo(year));

            var before = _controller.ViewBag.Before as List<Person>;
            var inYear = _controller.ViewBag.InYear as List<Person>;
            var after = _controller.ViewBag.After as List<Person>;

            Assert.That(before, Is.Not.Null);
            Assert.That(inYear, Is.Not.Null);
            Assert.That(after, Is.Not.Null);
          
            Assert.That(before, Is.Empty);
            Assert.That(inYear, Is.Empty);
            Assert.That(after, Is.Empty);

            _personRepositoryMock.Verify(repo => repo.GetAll(), Times.Exactly(2));
            _personRepositoryMock.Verify(repo => repo.AroundYear(It.IsAny<int>()), Times.Never);
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
            Assert.That(result, Is.InstanceOf<ViewResult>());
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
            _personRepositoryMock.Verify(repo => repo.Create(It.IsAny<Person>()), Times.Once);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Persons"));
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
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.AreSame(newPerson, viewResult.ViewData.Model);
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.ViewData.Model, Is.EqualTo(newPerson));
        }

        [Test]
        public void Update_GETReturnsViewResultWithPerson()
        {
            // Arrange
            var personId = _people.First().Id;

            // Act
            var result = _controller.Update(personId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.IsAssignableFrom<Person>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            //Assert.AreEqual(_people.First(p => p.Id == personId), model);
            Assert.That(model, Is.EqualTo(_people.First(p => p.Id == personId)));
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
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(result as RedirectToActionResult, Is.Not.Null);
            Assert.That((result as RedirectToActionResult).ActionName, Is.EqualTo("Persons"));
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
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        [Test]
        public void Delete_GET_ReturnsViewResultWithId()
        {
            // Arrange
            var personId = _people.First().Id;

            // Act
            var result = _controller.Delete(personId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.IsAssignableFrom<Guid>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            Assert.That(model, Is.EqualTo(personId));
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
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.IsAssignableFrom<string>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            //Assert.AreEqual(personToDelete.FullName, model);
            Assert.That(model, Is.EqualTo(personToDelete.FullName));
        }

        [Test]
        public void Detail_ReturnsViewResultWithPerson()
        {
            // Arrange
            var personId = _people.First().Id;

            // Act
            var result = _controller.Detail(personId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.IsAssignableFrom<Person>(viewResult.ViewData.Model);
            var model = viewResult.ViewData.Model;
            Assert.That(model, Is.EqualTo(_people.First(p => p.Id == personId)));
        }
    }
}
