﻿using AutoMapper;
using AymanFreelance.BLL.Interfaces;
using AymanFreelance.DAL.Entities;
using AymanFreelance.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;
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

        #region AddNew Project + Edit
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
                return RedirectToAction("Projects", "SponsorClient");
            }

            return View(model);
        }

        public IActionResult UpdateProject(int? ProjectId)
        {
            var data = new ProjectTBL_VM();

            if (ProjectId == 0 || !unitOfWork.ProjectTBLRepository.GetAllCustomized(
                filter: a => a.IsDeleted == false && a.ID == ProjectId).Any())
                return RedirectToAction("Projects", "SponsorClient");

            var project = unitOfWork.ProjectTBLRepository.GetAllCustomized(
                 filter: a => a.IsDeleted == false && a.ID == ProjectId, includes: new Expression<Func<ProjectTBL, object>>[]
            {
                                         p => p.ProjectOwnerTBL,
                                         p => p.ProjectFreelancerTBL

            }).FirstOrDefault();
            if (string.IsNullOrEmpty(project.ProjectFreelancerTBLId))
            {
                data = Mapper.Map<ProjectTBL_VM>(project);
            }
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProject(ProjectTBL_VM model)
        {
            if (ModelState.IsValid) {
                var project = unitOfWork.ProjectTBLRepository.GetAllCustomized(
                filter: a => a.IsDeleted == false && a.ID == model.ID).FirstOrDefault();
                project.TotalCost = model.TotalCost;
                project.RequiredDaysOfDelivery = model.RequiredDaysOfDelivery;
                project.Description = model.Description;
                project.Name = model.Name;
                project.IsPaymentInAdvancePaid = model.IsPaymentInAdvancePaid;

                unitOfWork.ProjectTBLRepository.Update(project);
            }
            return View(model);
        }
        #endregion

        #region Projects + Manage + Hire Freelancer
        public IActionResult Projects()
        {
            var data = new Search_VM();

            var projects = unitOfWork.ProjectTBLRepository.GetAllCustomized(
                       filter: a => a.IsDeleted == false && a.IsDelivered == false && a.ProjectOwnerTBLId == User.FindFirstValue(ClaimTypes.NameIdentifier).ToString(), includes: new Expression<Func<ProjectTBL, object>>[]
                       {
                                  p => p.ProjectOwnerTBL,
                                  p => p.ProjectFreelancerTBL
                       });
            data.ProjectTBL_VM = Mapper.Map<List<ProjectTBL_VM>>(projects.OrderByDescending(a => a.CreationDate));

            return View(data);
        }

        public IActionResult ManageProject(int? ProjectId)
        {
            if (ProjectId == 0 || !unitOfWork.ProjectTBLRepository.GetAllCustomized(
                filter: a => a.IsDeleted == false && a.ID == ProjectId).Any())
                return RedirectToAction("Projects", "SponsorClient");

            var data = new WhoisProject_VM();
            var project = unitOfWork.ProjectTBLRepository.GetAllCustomized(
                 filter: a => a.IsDeleted == false && a.ID == ProjectId, includes: new Expression<Func<ProjectTBL, object>>[]
            {
                                                             p => p.ProjectOwnerTBL,
                                                             p => p.ProjectFreelancerTBL

            }).FirstOrDefault();
            data.ProjectTBL_VM = Mapper.Map<ProjectTBL_VM>(project);

            var Applications = unitOfWork.ProjectApplicationTBLRepository.GetAllCustomized(
                 filter: a => a.IsDeleted == false && a.ProjectTBLId == ProjectId, includes: new Expression<Func<ProjectApplicationTBL, object>>[]
            {
                                                             p => p.AppUserWhoWrote,

            });

            data.ProjectApplicationTBL_VM = Mapper.Map<List<ProjectApplicationTBL_VM>>(Applications.OrderByDescending(a => a.CreationDate));

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> HireFreelancerToProject(int? ApplicationId)
        {
            if (ApplicationId == 0 || !unitOfWork.ProjectApplicationTBLRepository.GetAllCustomized(
                     filter: a => a.IsDeleted == false && a.ID == ApplicationId).Any())
                return RedirectToAction("Projects", "SponsorClient");

            var application = unitOfWork.ProjectApplicationTBLRepository.GetAllCustomized(
                          filter: a => a.IsDeleted == false && a.ID == ApplicationId).FirstOrDefault();
            var project = unitOfWork.ProjectTBLRepository.GetAllCustomized(
                          filter: a => a.IsDeleted == false && a.ID == ApplicationId).FirstOrDefault();

            project.ProjectFreelancerTBLId = application.AppUserWhoWroteId;
            project.TotalCost = application.Cost;
            project.DateOfStartWork = DateTime.Now;
            project.DateOfDelivery = DateTime.Now.AddDays((int)application.DaysOfDelivery);
            project.RequiredDaysOfDelivery = application.DaysOfDelivery;
            project.PaymentInAdvance = application.PaymentInAdvance;
            project.IncomeProfit = (1 / 100) * project.TotalCost;
            unitOfWork.ProjectTBLRepository.Update(project);


            // Send to Freelancer Start
            var appSettings = configuration.GetSection("AppSettingsEmails").Get<Dictionary<string, string>>();

            string AymanFreelanceUrl = appSettings["AymanFreelance.Pl.Url"];
            string AymanFreelanceUrl2 = configuration["AymanFreelance.Pl.Url"];
            if (string.IsNullOrEmpty(AymanFreelanceUrl) && string.IsNullOrEmpty(AymanFreelanceUrl2))
                AymanFreelanceUrl = "https://FreelancerHouse.runasp.net/";

            string AymanFreelanceName = appSettings["AymanFreelance.Pl.Name"];
            string AymanFreelanceName2 = configuration["AymanFreelance.Pl.Name"];
            if (string.IsNullOrEmpty(AymanFreelanceName) && string.IsNullOrEmpty(AymanFreelanceName2))
                AymanFreelanceName = "Freelancer's House";


            var Freelancer = await unitOfWork.UserManager.FindByIdAsync(project.ProjectFreelancerTBLId);
            var Client = await unitOfWork.UserManager.FindByIdAsync(project.ProjectOwnerTBLId);
            //var ActivateUserLink = configuration["AymanFreelance.Pl.Url"] + "Home/WhoisProject?ProjectId=" + project.ID;
            string ActivateUserLink = AymanFreelanceUrl + "Home/WhoisProject?ProjectId=" + project.ID;

            var Email = new EmailTBL_VM();
            Email.To = Freelancer.Email;
            //Email.Subject = configuration["AymanFreelance.Pl.Name"] + " - Project Hiring";
            Email.Subject = AymanFreelanceName + " - Project Hiring";
            Email.Body = await GetActivationTemplateAsync(Freelancer.UserName, Email.Subject, project.HashCode, ActivateUserLink);
            var newEmail = Mapper.Map<EmailTBL>(Email);
            // Send email
            await unitOfWork.EmailTBLRepository.SendEmailAsync(newEmail, 2);
            // Save Email
            unitOfWork.EmailTBLRepository.Add(newEmail);
            // Send to Freelancer End

            return RedirectToAction("ManageProject", "SponsorClient", new { ProjectId = project.ID });
        }
        #endregion

        private async Task<string> GetActivationTemplateAsync(string UserName, string Subject, string ProjectCode, string ProjectUrl)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template3", "HireFreelancer.html");
            string html = await System.IO.File.ReadAllTextAsync(templatePath);

            // Replace placeholders
            html = html.Replace("{{UserName}}", UserName);
            html = html.Replace("{{Subject}}", Subject);
            html = html.Replace("{{ProjectCode}}", ProjectCode);
            html = html.Replace("{{ProjectUrl}}", ProjectUrl);
            html = html.Replace("{{Year}}", DateTime.Now.Year.ToString());

            return html;
        }
    }
}
