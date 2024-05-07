using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PANOPA.Migrations;
using PANOPA.Models;

namespace YourProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;


        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Kullanıcı adı veya şifre boş bırakılamaz.";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                return View();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, password)))
            {
                ViewBag.ErrorMessage = "Kullanıcı adı veya şifre yanlış.";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Username"] = username;
            ViewData["Password"] = password;

            return View(result);
        }



        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string username, string password, string nameSurname, string passwordRepeat)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nameSurname))
            {
                ViewBag.ErrorMessage = "Kullanıcı adı, şifre veya ad soyad boş bırakılamaz.";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                ViewData["NameSurname"] = nameSurname;
                ViewData["PasswordRepeat"] = passwordRepeat;
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = username,
                NameSurname = nameSurname
            };

            var userIsExist = _userManager.Users.Where(x => x.UserName == username);

            if(userIsExist.Any())
            {
                ViewBag.ErrorMessage = "Bu kullanıcı adı mevcut";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                ViewData["NameSurname"] = nameSurname;
                ViewData["PasswordRepeat"] = passwordRepeat;
                return View();
            }

            if (!password.Equals(passwordRepeat))
            {
                ViewBag.ErrorMessage = "Şifre ve şifre tekrarı aynı olmalıdır";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                ViewData["NameSurname"] = nameSurname;
                return View();
            }

            if (!nameSurname.Contains(' ') || nameSurname.Any(char.IsDigit))
            {
                ViewBag.ErrorMessage = "Adınızı ve soyadınızı yazınız";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                ViewData["NameSurname"] = nameSurname;
                return View();
            }

            bool containsUpperCase = false;
            bool containsLowerCase = false;
            bool containsDigit = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                    containsUpperCase = true;
                else if (char.IsLower(c))
                    containsLowerCase = true;
                else if (char.IsDigit(c))
                    containsDigit = true;
            }

            if (!containsDigit || !containsLowerCase || !containsUpperCase)
            {
                ViewBag.ErrorMessage = "Şifre 6 karakterden büyük olmalı, en az 1 büyük harf, 1 küçük harf ve 1 rakam içermelidir.";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                ViewData["NameSurname"] = nameSurname;
                return View();
            }


            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                var processes = _context.Processes.ToList();

                foreach(var process in processes)
                {
                    var userProcess = new UserProcess
                    {
                        ProcessName = process.Name,
                        UserName = username
                    };

                    _context.UserProcesses.Add(userProcess);
                }
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Kullanıcı oluşturulamadı";
                ViewData["Username"] = username;
                ViewData["Password"] = password;
                ViewData["NameSurname"] = nameSurname;
                ViewData["PasswordRepeat"] = passwordRepeat;
                return View(result);
            }
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Login");
        }
    }
}
