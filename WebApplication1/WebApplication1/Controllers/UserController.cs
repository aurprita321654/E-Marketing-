using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        addadEntities1 db = new addadEntities1();
        // GET: User
        public ActionResult Index(int? page)
        {
            
            if (TempData["cart"] != null)
            {
                float x = 0;
                List<cart> li2 = TempData["cart"] as List<cart>;
                foreach(var item in li2)
                {
                    x += item.bill;
                }
                TempData["total"] = x;
            }

            TempData.Keep();
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<category> stu = list.ToPagedList(pageindex, pagesize);

            return View(stu);
        }

        public ActionResult AddtoCart(int? Id)
        {
            product p = db.products.Where(x => x.pro_id == Id).SingleOrDefault();
            return View(p);
        }

        List<cart> li = new List<cart>();
        [HttpPost]

        public ActionResult AddtoCart(product pi, string qty, int Id)
        {
            product p = db.products.Where(x => x.pro_id == Id).SingleOrDefault();

            cart c = new cart();
            c.productid = p.pro_id;
            c.productname = p.pro_name;
            c.price = (float)p.pro_price;
            c.qty = Convert.ToInt32(qty);
            c.bill = c.price * c.qty;
            if (TempData["cart"] == null)
            {
                li.Add(c);
                TempData["cart"] = li;
            }
            else
            {
                List<cart> li2 = TempData["cart"] as List<cart>;
                int flag = 0;
                foreach(var item in li2)
                {
                    if(item.productid == c.productid)
                    {
                        item.qty += c.qty;
                        item.bill += c.bill;
                        flag = 1;
                    }
                }

                if (flag == 0)
                {
                    li2.Add(c);
                }

                TempData["cart"] = li2;

                
            }


            TempData.Keep();


            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            TempData.Keep();

            return View();
        }

        [HttpPost]

        public ActionResult Checkout(tbl_order order)
        {
            List<cart> li = TempData["cart"] as List<cart>;

            tbl_invoice iv = new tbl_invoice();
            iv.in_fk_user = Convert.ToInt32(Session["u_id"]);
            iv.in_date = System.DateTime.Now;
            iv.in_totalbill = (float)TempData["total"];
            db.tbl_invoice.Add(iv);
            db.SaveChanges();

            foreach(var item in li)
            {
                tbl_order od = new tbl_order();
                od.o_fk_pro = item.productid;
                od.o_fk_invoice = iv.in_id;
                od.o_date = System.DateTime.Now;
                od.o_qty = item.qty;
                od.o_unitprice = (int)item.price;
                db.tbl_order.Add(od);
                db.SaveChanges();
            }

            TempData.Remove("total");
            TempData.Remove("cart");

            TempData["msg"] = "Transaction Successful...";
            TempData.Keep();

            return RedirectToAction("Index");
        }



        public ActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Signup(usr uvm, HttpPostedFileBase imgfile)
        {
            string path = uploadimgfile(imgfile);
            if (path.Equals("-1"))
            {
                ViewBag.error = "Image could not be uploaded....";
            }
            else
            {
                usr u = new usr();
                u.u_name = uvm.u_name;
                u.u_pass = uvm.u_pass;
                u.u_mail = uvm.u_mail;            
                u.u_img = path;
                u.u_contact = uvm.u_contact;
                u.nid_img = path;
                u.nid_no = uvm.nid_no;
                u.status = uvm.status;
                db.usrs.Add(u);
                db.SaveChanges();
                return RedirectToAction("Login");

            }

            return View();
        }

        public string uploadimgfile(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);

                        //    ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable.'); </script>");
                }
            }

            else
            {
                Response.Write("<script>alert('Please select a file'); </script>");
                path = "-1";
            }



            return path;
        }

        public ActionResult login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult login(usr avm)
        {
            usr ad = db.usrs.Where(x => x.u_mail == avm.u_mail && x.u_pass == avm.u_pass).SingleOrDefault();
            if (ad != null)
            {

                Session["u_id"] = ad.u_id.ToString();
                return RedirectToAction("Index");

            }
            else
            {
                ViewBag.error = "Invalid username or password";

            }

            return View();
        }

        [HttpGet]
        public ActionResult CreateAd()
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");

            return View();
        }

        [HttpPost]
        public ActionResult CreateAd(product pvm, HttpPostedFileBase imgfile)
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");


            string path = uploadimgfile(imgfile);
            if (path.Equals("-1"))
            {
                ViewBag.error = "Image could not be uploaded....";
            }
            else
            {
                product p = new product();
                p.pro_name = pvm.pro_name;
                p.pro_price = pvm.pro_price;
                p.pro_img = path;
                p.pro_fk_cat = pvm.pro_fk_cat;
                p.pro_descrip = pvm.pro_descrip;
                p.pro_fk_usr = Convert.ToInt32(Session["u_id"].ToString());
                db.products.Add(p);
                db.SaveChanges();
                Response.Redirect("index");

            }

            return View();
        }
        public ActionResult Ads(int? id, int? page)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.pro_fk_cat == id).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<product> stu = list.ToPagedList(pageindex, pagesize);


            return View(stu);


        }

        [HttpPost]
        public ActionResult Ads(int? id, int? page, string search)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.pro_name.Contains(search)).OrderByDescending(x => x.pro_id).ToList();
            IPagedList<product> stu = list.ToPagedList(pageindex, pagesize);


            return View(stu);


        }

        public ActionResult ViewAd(int? id)
        {
            Adviewmodel ad = new Adviewmodel();
            product p = db.products.Where(x => x.pro_id == id).SingleOrDefault();
            ad.pro_id = p.pro_id;
            ad.pro_name = p.pro_name;
            ad.pro_img = p.pro_img;
            ad.pro_price = p.pro_price;
            ad.pro_descrip = p.pro_descrip;
            category cat = db.categories.Where(x => x.cat_id == p.pro_fk_cat).SingleOrDefault();
            ad.cat_name = cat.cat_name;
            usr u = db.usrs.Where(x => x.u_id == p.pro_fk_usr).SingleOrDefault();
            ad.u_name = u.u_name;
            ad.u_img = u.u_img;
            ad.u_contact = u.u_contact;
            ad.pro_fk_usr = u.u_id;




            return View(ad);
        }

        public ActionResult Signout()
        {
            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("login");
        }




        
        public ActionResult DeleteAd(int? id)
        {

            product p = db.products.Where(x => x.pro_id ==id).SingleOrDefault();
            db.products.Remove(p);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}