using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VetDB.Models;
using PagedList;

namespace VetDB.Controllers
{
    public class MainController : Controller
    {
        private VetDataDBEntities db = new VetDataDBEntities();

        // GET: Main
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? Page)
        {
            ViewBag.CurrentSort = sortOrder; //Передаём текущий порядок сортировки на следущую страницу
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : ""; //Принимаем параметр порядка сортировке по имени, по умолчанию параметр не задан, выводим по возрастанию
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date"; //Принимаем параметр порядка сортировки по дате регистрации

            if (searchString != null) //Если меняется ключ поиска, возвращаемся на первую страницу, что бы видеть все элементы
            {
                Page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString; //Передаем текущую ключевую строку поиска на следущую страницу, что бы при переключении страниц не сбрасывался поиск

            var Table = from s in db.Table //Создаем запрос в базу данных
                           select s;
            if (!String.IsNullOrEmpty(searchString)) //Принимаем ключевую строку поиска, если переданное значение не пустое - возвращаем найденный элемент
            {
                Table = Table.Where(s => s.Name.Contains(searchString)); //
            }
            switch (sortOrder) //Выстраиваем согласно выбранному параметру
            {
                case "name_desc":
                    Table = Table.OrderByDescending(s => s.Name); //Задаем по убыванию имени
                    break;
                case "Date":
                    Table = Table.OrderBy(s => s.RegisterDate); //Задаем по возрастанию даты
                    break;
                case "date_desc":
                    Table = Table.OrderByDescending(s => s.RegisterDate); //Задаем по убыванию даты
                    break;
                default:
                    Table = Table.OrderBy(s => s.Name); // По умолчанию задаем по возрастанию имени
                    break;
            }
            int PageSize = 5; //Задаём колличество элементов на странице
            int PageNumber = (Page ?? 1); //По умолчанию показываем первую страницу
            return View(Table.ToPagedList(PageNumber, PageSize)); //Передаем представлению данные и номер страницы
        }

        // GET: Main/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) //Контроллеру не был передан Id элемента для детального просмотра, возвращаем страницу с ошибкой
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Table.Find(id); //Ищем полученный Id в таблице
            if (table == null) //Если id не существует, возвращаем страницу с ошибкой
            {
                return HttpNotFound();
            }
            return View(table); //передаем представлению данные об элементе с указанным id 
        }

        // GET: Main/Create
        public ActionResult Create()
        {
            return View(); //Возвращем предствление добавления записи
        }

        // POST: Main/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Owner,RegisterDate,Vaccinated")] Table table) //Принимаем данные из формы
        {
            if (ModelState.IsValid) //Проверяем корректность данных
            {
                db.Table.Add(table); //Добавляем в таблицу, сохраняем изменения, возвращаемся на главную
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(table);
        }

        // GET: Main/Edit/5
        public ActionResult Edit(int? id) //Принимаем Id редактируемого элемента
        {
            if (id == null) //Если id не существует, возвращаем страницу с ошибкой
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Table.Find(id); //Создаем набор данных для соответсвующего id, и если данные существуют передаем их в представление и выводим его
            if (table == null)
            {
                return HttpNotFound();
            }
            return View(table);
        }

        // POST: Main/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Owner,RegisterDate,Vaccinated")] Table table) //Принимаем измененные данные из формы
        {
            if (ModelState.IsValid) //Проверяем корректность данных
            {
                db.Entry(table).State = EntityState.Modified; //Передаем EF изменившиеся данные для обновления, сохраняем изменения, возвращаемся на главную
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(table);
        }

        // GET: Main/Delete/5
        public ActionResult Delete(int? id) //Принимаем Id удаляемого элемента
        {
            if (id == null) //Если id не существует, возвращаем страницу с ошибкой
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Table.Find(id); //Создаем набор данных для соответсвующего id, и если данные существуют передаем их в представление и выводим его
            if (table == null)
            {
                return HttpNotFound();
            }
            return View(table);
        }

        // POST: Main/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) //Получаем подтверждение об удалении из формы в представлении
        {
            Table table = db.Table.Find(id); //Находим запись с указанным id
            db.Table.Remove(table); //передаем EF команду на удаление, сохраняем изменения, возвращаемся на главную
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) //метод Dispose необходим для принудительного освобождения ресурсов.
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
