using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PANOPA.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;

namespace PANOPA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            var userName = _userManager.GetUserAsync(User).Result?.UserName;

            var processes = _context.Processes.ToList();

            var userProcesses = _context.UserProcesses.Where(x => x.UserName == userName).ToList();

            List<ProcessModel> processModelList = new List<ProcessModel>();

            foreach (var process in processes)
            {
                bool isActive = userProcesses.Any(up => up.ProcessName == process.Name);

                processModelList.Add(new ProcessModel
                {
                    Id = process.Id,
                    Name = process.Name,
                    IsActive = isActive
                });
            }

            return View(processModelList);
        }

        [HttpGet]
        public IActionResult Save()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Save([FromBody] Delay delay)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userProject = _context.Projects.FirstOrDefault(x => x.UserName == user.UserName);
            if (ModelState.IsValid)
            {
                // Veritaban�nda i�lem bilgilerini kaydet
                var newDelay = new Delay
                {
                    ProcessId = delay.ProcessId,
                    ProcessName = delay.ProcessName,
                    StartDate = delay.StartDate,
                    EndDate = delay.EndDate,
                    DelayTime = delay.DelayTime,
                    UserName = user?.UserName,
                    ProjectName = delay.ProjectName,
                    PanoNo = delay.PanoNo,
                    NameSurname = delay.NameSurname,
                    PausedReason = delay.PausedReason
                };

                _context.Delays.Add(newDelay);
                _context.SaveChanges();

                return Ok(new { message = "��lem ba�ar�yla kaydedildi." });
            }

            return BadRequest(new { message = "Ge�ersiz istek." });
        }

        public IActionResult Delay()
        {
            var userName = _userManager.GetUserAsync(User).Result?.UserName;

            var delay = _context.Delays.Where(x => x.UserName == userName).ToList();

            if(userName.Equals("admin"))
            {
                delay = _context.Delays.ToList();
            }

            return View(delay);
        }

        public IActionResult UserProcess()
        {
            var userName = _userManager.GetUserAsync(User).Result?.UserName;

            var processUsers = _context.UserProcesses.GroupBy(x => x.UserName).ToList();

            if (userName.Equals("admin"))
            {
                processUsers = processUsers.Where(x => x.Key != "admin").ToList();




                List<UserProcessModel> userProcessModels = new List<UserProcessModel>();

                foreach (var processUser in processUsers)
                {
                    UserProcessModel processModel = new UserProcessModel();
                    processModel.UserName = processUser.Key;
                    processModel.ProcessName = new List<string>();

                    //var userProject = _context.Projects.Where(x => x.UserName == processUser.Key).FirstOrDefault();

                    //processModel.ProjectName = userProject.Name;

                    var userProcesses = _context.UserProcesses.Where(x => x.UserName == processUser.Key).ToList();

                    foreach (var process in userProcesses)
                    {
                        processModel.ProcessName.Add(process.ProcessName);
                    }

                    userProcessModels.Add(processModel);
                }

                return View(userProcessModels);
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost] public IActionResult UpdateProject([FromBody] DelayModel delayInfo)
        {
            var delay = _context.Delays.Find(delayInfo.Id);
            if (delay != null)
            {
                delay.ProjectName = delayInfo.ProjectName;
                delay.PanoNo = delayInfo.PanoNo;
                delay.NameSurname = delayInfo.NameSurname;
                _context.Delays.Update(delay);
                _context.SaveChanges();
            }
            return View();
        }



        [HttpPost]
        public ActionResult UpdateProcess([FromBody] UserProcessModel model)
        {

            var userName = _userManager.GetUserAsync(User).Result?.UserName;

            if (userName.Equals("admin")) {

                var currentProcesses = _context.UserProcesses.Where(x => x.UserName == model.UserName).ToList();

                var currentProcessList = new List<string>();

                foreach (var item in currentProcesses)
                {
                    currentProcessList.Add(item.ProcessName);
                }

                var newProcesses = model.ProcessName.ToList();

                List<string> deletedProcesses = currentProcessList.Except(newProcesses).ToList();
                List<string> insertedProcesses = newProcesses.Except(currentProcessList).ToList();

                foreach (var item in deletedProcesses)
                {
                    var oldProcess = _context.UserProcesses.Where(x => x.ProcessName == item && x.UserName == model.UserName).FirstOrDefault();
                    if (oldProcess != null)
                    {
                        _context.UserProcesses.Remove(oldProcess);
                    }
                }

                _context.SaveChanges();


                foreach (var item in insertedProcesses)
                {

                    var userProcess = new UserProcess
                    {
                        ProcessName = item,
                        UserName = model.UserName
                    };

                    _context.UserProcesses.Add(userProcess);
                }
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(model.ProjectName))
                {
                    var oldProject = _context.Projects.Where(x => x.UserName == model.UserName).FirstOrDefault();
                    if (oldProject != null)
                    {
                        oldProject.Name = model.ProjectName;
                        _context.Projects.Update(oldProject);
                    }

                    var delays = _context.Delays.Where(x => x.UserName == model.UserName).ToList();

                    foreach (var delay in delays)
                    {
                        delay.ProjectName = model.ProjectName;
                        _context.Delays.Update(delay);
                    }

                    _context.SaveChanges();
                }

                return RedirectToAction("UserProcess", "Home");
            }

            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
