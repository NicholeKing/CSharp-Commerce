using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Commerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Commerce.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        public static List<Product> Cart = new List<Product>(){};

        [HttpGet("")]
        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost("regCheck")]
        public IActionResult regCheck(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                    {
                        ModelState.AddModelError("Email", "Email already in use!");
                        return View("Index");
                    }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.Add(newUser);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("LoggedIn", newUser.UserId);
                return Redirect("Dashboard");
            }
            return View("Index");
        }

        [HttpPost("logCheck")]
        public IActionResult logCheck(LUser CheckUser)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == CheckUser.LEmail);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LEmail", "Invalid Email/Password");
                    return View("Index");
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LUser>();
                
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(CheckUser, userInDb.Password, CheckUser.LPassword);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    ModelState.AddModelError("LEmail", "Invalid Email/Password");
                    return View("Index");
                    // handle failure (this should be similar to how "existing email" is handled)
                }
                HttpContext.Session.SetInt32("LoggedIn", userInDb.UserId);
                return Redirect("Dashboard");
            }
            return View("Index");
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            int? Sess = HttpContext.Session.GetInt32("LoggedIn");
            if(Sess == null)
            {
                HttpContext.Session.Clear();
                return Redirect("/");
            }
            ViewBag.Person = dbContext.Users.Include(s => s.Stock).FirstOrDefault(a => a.UserId == (int)Sess);

            ViewBag.Cart = Cart.Count();

            ViewBag.Others = dbContext.Products.Include(f => f.Seller).Where(d => d.UserId != (int)Sess).Where(s => s.Quantity > 0).ToList();
            return View();
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }

        [HttpGet("AddProduct")]
        public IActionResult AddProduct()
        {
            int? Sess = HttpContext.Session.GetInt32("LoggedIn");
            if(Sess == null)
            {
                HttpContext.Session.Clear();
                return Redirect("/");
            }
            return View();
        }

        [HttpPost("AddCheck")]
        public IActionResult AddCheck(Product newProd)
        {
            if(ModelState.IsValid)
            {
                int? Sess = HttpContext.Session.GetInt32("LoggedIn");
                newProd.UserId = (int)Sess;
                dbContext.Add(newProd);
                dbContext.SaveChanges();
                return Redirect("Dashboard");
            }
            return View("AddProduct");
        }

        [HttpGet("Product/{PID}")]
        public IActionResult OneProduct(int PID)
        {
            ViewBag.Prod = dbContext.Products.FirstOrDefault(a => a.PID == PID);
            return View();
        }

        [HttpGet("Delete/{PID}")]
        public IActionResult Delete(int PID)
        {
            Product ProdToDelete = dbContext.Products.SingleOrDefault(g => g.PID == PID);
            dbContext.Products.Remove(ProdToDelete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Edit/{UserId}/{PID}")]
        public IActionResult Edit(int UserId, int PID)
        {
            int? Sess = HttpContext.Session.GetInt32("LoggedIn");
            if(Sess == null)
            {
                HttpContext.Session.Clear();
                return Redirect("/");
            }
            if(Sess != UserId)
            {
                return Redirect("/logout");
            }
            Product EditProd = dbContext.Products.FirstOrDefault(a => a.PID == PID);
            return View("Edit", EditProd);
        }

        [HttpPost("EditCheck")]
        public IActionResult EditCheck(Product EdProd)
        {
            Console.WriteLine($"********************{EdProd.PID} - {EdProd.Name} - {EdProd.Description} - {EdProd.Price} - {EdProd.Quantity}");
            if(ModelState.IsValid)
            {
                Product OrigProd = dbContext.Products.FirstOrDefault(b => b.PID == EdProd.PID);
                OrigProd.Name = EdProd.Name;
                OrigProd.Description = EdProd.Description;
                OrigProd.Price = EdProd.Price;
                OrigProd.Quantity = EdProd.Quantity;
                OrigProd.UpdatedAt = DateTime.Now;
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            Product EditProd = dbContext.Products.FirstOrDefault(a => a.PID == EdProd.PID);
            return View("Edit", EditProd);
        }

        [HttpPost("AddToCart/{PID}")]
        public IActionResult AddToCart(int quant, int PID)
        {
            Product BoughtProd = dbContext.Products.Include(a => a.Seller).FirstOrDefault(s => s.PID == PID);
            for(var i = 1; i<=quant; i++)
            {
                Cart.Add(BoughtProd);
            }
            return RedirectToAction("Dashboard");
        } 

        [HttpGet("MyCart")]
        public IActionResult MyCart()
        {
            ViewBag.Cart = Cart;
            return View();
        }

        [HttpGet("RemoveFromCart/{i}")]
        public IActionResult RemoveFromCart(int i)
        {
            Cart.RemoveAt(i);
            return RedirectToAction("MyCart");
        }

        [HttpGet("Checkout")]
        public IActionResult Checkout()
        {
            int? Sess = HttpContext.Session.GetInt32("LoggedIn");
            foreach(var i in Cart)
            {
                Product OrderedProd = dbContext.Products.FirstOrDefault(a => a.PID == i.PID);
                OrderedProd.Quantity -= 1;
                Transaction NewTrans = new Transaction();
                NewTrans.UserId = (int)Sess;
                NewTrans.PID = i.PID;
                dbContext.Add(NewTrans);
                dbContext.SaveChanges();
            }
            Cart.Clear();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("MyOrders")]
        public IActionResult MyOrders()
        {
            int? Sess = HttpContext.Session.GetInt32("LoggedIn");
            if(Sess == null)
            {
                HttpContext.Session.Clear();
                return Redirect("/");
            }
            ViewBag.AllOrders = dbContext.Transactions.Include(p => p.Product).ThenInclude(s => s.Seller).Where(a => a.UserId == (int)Sess);
            return View();
        }
    }
}
