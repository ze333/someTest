using System;
using System.Data.Services.Client;
using System.Net;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using TestProj.BPMonlineService;

namespace TestProj.Controllers
{
    public class HomeController : Controller
    {
        private static DataServiceQuery<Contact> _allContacts;
        private static readonly Uri ServerUri = new Uri("http://178.159.246.209:1410/0/ServiceModel/EntityDataService.svc/");

        private static readonly XNamespace Ds = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace Dsmd = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";

        [HttpGet]
        public ActionResult Index()
        {
            GetContactCollection();
            return View(_allContacts);
        }

        [HttpGet]
        public ActionResult CreateContact()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
        
        public ActionResult EditView(Contact contact, string submitButton)
        {
            switch (submitButton)
            {
                case "Edit":
                    var id = contact.Id;
                    foreach (var cont in _allContacts)
                    {
                        if (id == cont.Id)
                        {
                            contact = cont;
                        }
                    }
                    return View(contact);
                case "Delete": 
                    return (Delete(contact));
                default:
                    return (Index());
            }
        }

        public void GetContactCollection()
        {
            var context = new BPMonline(ServerUri) {Credentials = new NetworkCredential("Supervisor", "Supervisor")};
            _allContacts = context.ContactCollection;
        }

        [HttpPost]
        public ActionResult CreateContact(Contact contact)
        {
            var content = new XElement(Dsmd + "properties",
                          new XElement(Ds + "Name", contact.Name),
                          new XElement(Ds + "MobilePhone", contact.MobilePhone),
                          new XElement(Ds + "Dear", contact.Dear),
                          new XElement(Ds + "JobTitle", contact.JobTitle),
                          new XElement(Ds +"BirthDate", contact.BirthDate));
            var entry = new XElement(Atom + "entry",
                        new XElement(Atom + "content",
                        new XAttribute("type", "application/xml"), content));
            Console.WriteLine(entry.ToString());
            var request = (HttpWebRequest)WebRequest.Create(ServerUri + "ContactCollection/");
            request.Credentials = new NetworkCredential("Supervisor", "Supervisor");
            request.Method = "POST";
            request.Accept = "application/atom+xml";
            request.ContentType = "application/atom+xml;type=entry";
            using (var writer = XmlWriter.Create(request.GetRequestStream()))
            {
                entry.WriteTo(writer);
            }
            using (WebResponse response = request.GetResponse())
            {
                if (((HttpWebResponse)response).StatusCode == HttpStatusCode.Created)
                {
                    return RedirectToAction("CreateContact");
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
        }

        [HttpPost]
        public ActionResult Update(Contact contact)
        {
            var contactId = contact.Id.ToString();
            var content = new XElement(Dsmd + "properties",
                   new XElement(Ds + "Name", contact.Name),
                   new XElement(Ds + "MobilePhone", contact.MobilePhone),
                   new XElement(Ds + "Dear", contact.Dear),
                   new XElement(Ds + "JobTitle", contact.JobTitle),
                   new XElement(Ds + "BirthDate", contact.BirthDate)
            );
            var entry = new XElement(Atom + "entry",
                    new XElement(Atom + "content",
                            new XAttribute("type", "application/xml"),
                            content));
            var request = (HttpWebRequest)WebRequest.Create(ServerUri
                    + "ContactCollection(guid'" + contactId + "')");
            request.Credentials = new NetworkCredential("Supervisor", "Supervisor");
            request.Method = "PUT";
            request.Accept = "application/atom+xml";
            request.ContentType = "application/atom+xml;type=entry";
            using (var writer = XmlWriter.Create(request.GetRequestStream()))
            {
                entry.WriteTo(writer);
            }
            using (request.GetResponse())
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Delete(Contact contact)
        {
            var contactId = contact.Id.ToString();
            var request = (HttpWebRequest)WebRequest.Create(ServerUri
                    + "ContactCollection(guid'" + contactId + "')");
            request.Credentials = new NetworkCredential("Supervisor", "Supervisor");
            request.Method = "DELETE";
            using (request.GetResponse())
            {
                return RedirectToAction("Index");
            }
        }
    }
}