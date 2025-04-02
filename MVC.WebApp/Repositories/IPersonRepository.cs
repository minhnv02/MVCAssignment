using MVC.WebApp.Models;

namespace MVC.WebApp.Repositories
{
    public interface IPersonRepository
    {
        void Create(Person person);
        void Update(Person person);
        void Delete(Guid id);
        List<Person> GetAll();
        List<Person> Males();
        Person Oldest();
        List<Person> AroundYear(int year);
        MemoryStream ExportToExcel();
    }
}