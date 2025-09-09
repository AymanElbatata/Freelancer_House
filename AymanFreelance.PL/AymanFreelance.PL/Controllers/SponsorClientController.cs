using AutoMapper;
using AymanFreelance.BLL.Interfaces;
using AymanFreelance.DAL.Entities;
using AymanFreelance.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AymanFreelance.PL.Controllers
{
    [Authorize(Roles = "SponsorClient")]
    public class SponsorClientController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper Mapper;
        private readonly IConfiguration configuration;

        public SponsorClientController(IUnitOfWork unitOfWork, IMapper Mapper, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.Mapper = Mapper;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddNewProject()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewProject(ProjectTBL_VM model)
        {
            if (ModelState.IsValid)
            { 
                var newProject = Mapper.Map<ProjectTBL>(model);
                newProject.Image = "https://external-content.duckduckgo.com/iu/?u=http%3A%2F%2Fprojectaccelerator.co.uk%2Fwp-content%2Fuploads%2F2016%2F08%2FProject-Management.jpg&f=1&nofb=1&ipt=f65cd68c95661a34991df98318eba44507444372592ad8a02b862371fbccf9cb";
                newProject.ProjectOwnerTBLId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                unitOfWork.ProjectTBLRepository.Add(newProject);
                return RedirectToAction("Projects", "Home");
            }

            return View(model);
        }
    }
}
