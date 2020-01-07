using KPI.Model.DAO;
using KPI.Model.EF;
using KPI.Model.helpers;
using KPI.Model.ViewModel;
using KPI.Web.helpers;
using MvcBreadCrumbs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Threading;

namespace KPI.Web.Controllers
{
    [BreadCrumb(Clear = true)]
    public class ChartPeriodController : BaseController
    {

        // GET: Month
        [BreadCrumb(Clear = true)]
        public async Task<ActionResult> Index(string kpilevelcode, int? catid, string period, int? year, int? start, int? end, string type = "", int? comID = 0, int? dataID = 0, string title = "")
        {
            BreadCrumb.Add(Url.Action("Index", "Home"), "Home");
            BreadCrumb.SetLabel("ChartPeriod");

            if (period == "W")
            {
                BreadCrumb.SetLabel("Chart / Weekly");
            }
            else if (period == "M")
            {
                BreadCrumb.SetLabel("Chart / Monthly");
            }
            else if (period == "Q")
            {
                BreadCrumb.SetLabel("Chart / Quarterly");
            }
            else if (period == "Y")
            {
                BreadCrumb.SetLabel("Chart / Yearly");
            }

            var model = await new DataChartDAO().ListDatas(kpilevelcode, catid, period, year, start, end,(Session["UserProfile"] as UserProfileVM).User.ID);
            ViewBag.Model = model;
            return View();
        }


        public async Task<ActionResult> ListTasks(string code, int? page)
        {
            var urlRefer = Request.UrlReferrer.ToSafetyString();
            BreadCrumb.Add(Url.Action("Index", "Home"), "Home");
            BreadCrumb.Add(urlRefer, "ChartPeriod");
            BreadCrumb.SetLabel("ListTasks");

            var model = (await new UploadDAO().ListTasks(code)).OrderByDescending(x => x.CreatedDate).ToPagedList(page ?? 1, 20);
            return View(model);
        }

        public async Task<JsonResult> AddComment(AddCommentViewModel entity)
        {
            var sessionUser = Session["UserProfile"] as UserProfileVM;
            int levelNumberOfUserComment = sessionUser.User.LevelID;
            var data = await new KPILevelDAO().AddComment(entity, levelNumberOfUserComment);
            var tos = new List<string>();
            NotificationHub.SendNotifications();
            if (data.ListEmails.Count > 0 && await new SettingDAO().IsSendMail("ADDCOMMENT"))
            {
                var model = data.ListEmails.DistinctBy(x => x);
                string content = @"<p><b>*PLEASE DO NOT REPLY* this email was automatically sent from the KPI system.</b></p> 
                                   <p>The account <b>" + model.First()[0] + "</b> mentioned you in KPI System Apps. </p>" +
                                   "<p>Content: " + model.First()[4] + "</p>" +
                                   "<p>Link: <a href='" + data.QueryString + "'>Click Here</a></p>";
                Commons.SendMail(model.Select(x => x[1]).ToList(), "[KPI System-02] Comment", content, "Comment");
            }
            return Json(new { status = data.Status, isSendmail = true }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> DeleteComment(int id)
        {
            var sessionUser = Session["UserProfile"] as UserProfileVM;
            return Json(await new ActionPlanDAO().DeleteComment(id, sessionUser.User.ID), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> LoadDataComment(int dataid, int userid)
        {
            return Json(await new KPILevelDAO().ListComments(dataid, userid), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AddCommentHistory(int userid, int dataid)
        {
            return Json(await new KPILevelDAO().AddCommentHistory(userid, dataid), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> Remark(int dataid)
        {
            return Json(await new DataChartDAO().Remark(dataid), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AddFavourite(Model.EF.Favourite entity)
        {
            return Json(await new FavouriteDAO().Add(entity), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> LoadDataProvide(string obj, int page, int pageSize)
        {
            return Json(await new KPILevelDAO().LoadDataProvide(obj, page, pageSize), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> UpdateRemark(int dataid, string remark)
        {
            return Json(await new DataChartDAO().UpdateRemark(dataid, remark), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> Update(ActionPlan item)
        {
            return Json(await new ActionPlanDAO().Update(item), JsonRequestBehavior.AllowGet);
        }
        //Add ActionPlan
        public async Task<JsonResult> Add(AddActionPlanViewModel obj)
        {
            var userprofile = Session["UserProfile"] as UserProfileVM;
            obj.OwnerID = userprofile.User.ID;
            var data = await new ActionPlanDAO().Add(obj);//(item, obj.Subject, obj.Auditor, obj.CategoryID);
            NotificationHub.SendNotifications();
            if (data.ListEmails.Count > 0 && await new SettingDAO().IsSendMail("ADDTASK"))
            {

                string contentForPIC = @"<p><b>*PLEASE DO NOT REPLY* this email was automatically sent from the KPI system.</b></p> 
                                <p>The account <b>" + data.ListEmails.First()[0].ToTitleCase() + "</b> assigned a task to you in KPI Sytem App. </p>" +
                                "<p>Task name : <b>" + data.ListEmails.First()[3] + "</b></p>" +
                                "<p>Description : " + data.ListEmails.First()[4] + "</p>" +
                                "<p>Link: <a href='" + data.QueryString + "'>Click Here</a></p>";

                string contentAuditor = @"<p><b>*PLEASE DO NOT REPLY* this email was automatically sent from the KPI system.</b></p> 
                                <p>The account <b>" + data.ListEmailsForAuditor.First()[0].ToTitleCase() + "</b> created a new task ,assigned you are an auditor in KPI Sytem App. </p>" +
                                "<p>Task name : <b>" + data.ListEmailsForAuditor.First()[3] + "</b></p>" +
                                "<p>Description : " + data.ListEmailsForAuditor.First()[4] + "</p>" +
                                "<p>Link: <a href='" + data.QueryString + "'>Click Here</a></p>";
                Thread thread = new Thread( () =>
                {
                    Commons.SendMail(data.ListEmailsForAuditor.Select(x => x[1]).ToList(), "[KPI System-03] Action Plan (Add Task - Assign Auditor)", contentAuditor, "Action Plan (Add Task - Assign Auditor)");

                });
                Thread thread2 = new Thread( () =>
                {
                    Commons.SendMail(data.ListEmails.Select(x => x[1]).ToList(), "[KPI System-03] Action Plan (Add Task)", contentForPIC, "Action Plan (Add Task)");
                });
                thread.Start();
                thread2.Start();

                return Json(new { status = data.Status, isSendmail = true, message = data.Message }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { status = data.Status, isSendmail = true, message = data.Message }, JsonRequestBehavior.AllowGet);
        }
        //Delete ActionPlan (Task)
        public async Task<JsonResult> Delete(int id)
        {
            return Json(await new ActionPlanDAO().Delete(id, (Session["UserProfile"] as UserProfileVM).User.ID), JsonRequestBehavior.AllowGet);
        }
        //LoadActionplan
        public async Task<JsonResult> GetAll(int DataID, int CommentID, int UserID)
        {
            //var userprofile = Session["UserProfile"] as UserProfileVM;
            return Json(await new ActionPlanDAO().GetAll(DataID, CommentID, UserID), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetByID(int id)
        {
            return Json(await new ActionPlanDAO().GetByID(id), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> Approval(int id, int approveby, string KPILevelCode, int CategoryID, string url)
        {

            var model = await new ActionPlanDAO().Approval(id, (Session["UserProfile"] as UserProfileVM).User.ID, KPILevelCode, CategoryID);
            NotificationHub.SendNotifications();
            if (model.Item1.Count > 0 && await new SettingDAO().IsSendMail("APPROVAL"))
            {
                var data = model.Item1.DistinctBy(x => x);
                string content = @"<p><b>*PLEASE DO NOT REPLY* this email was automatically sent from the KPI system.</b></p> 
                                   <p>The account <b>" + data.First()[0].ToTitleCase() + "</b> approved the task <b>'" + data.First()[3] + "'</b> </p>" +
                                   "<p>Link: <a href='" + model.Item3 + "'>Click Here</a></p>";
                Commons.SendMail(data.Select(x => x[1]).ToList(), "[KPI System-05] Approved", content, "Action Plan (Approved)");
            }
            return Json(new { status = model.Item2, isSendmail = true }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> Done(int id, string KPILevelCode, int CategoryID, string url)
        {

            var userprofile = Session["UserProfile"] as UserProfileVM;
            var model = await new ActionPlanDAO().Done(id, userprofile.User.ID, KPILevelCode, CategoryID);
            NotificationHub.SendNotifications();
            if (model.Item1.Count > 0 && await new SettingDAO().IsSendMail("DONE"))
            {
                var data = model.Item1.DistinctBy(x => x);
                string content = @"<p><b>*PLEASE DO NOT REPLY* this email was automatically sent from the KPI system.</b></p> 
                                    <p>The account <b>" + data.First()[0].ToTitleCase() + "</b> has finished the task name <b>'" + data.First()[3] + "'</b></p>" +
                                    "<p>Link: <a href='" + model.Item3 + "'>Click Here</a></p>";
                Commons.SendMail(data.Select(x => x[1]).ToList(), "[KPI System-04] Action Plan (Finished Task)", content, "Action Plan (Finished Task)");
            }
            return Json(new { status = model.Item2, isSendmail = true }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AddNotification(Notification notification)
        {
            var status = await new NotificationDAO().Add(notification);
            NotificationHub.SendNotifications();
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> UpdateActionPlan(UpdateActionPlanVM actionPlan)
        {
            return Json(await new ActionPlanDAO().UpdateActionPlan(actionPlan), JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> UpdateSheduleDate(string name, string value, string pk)
        {
            var userprofile = Session["UserProfile"] as UserProfileVM;
            return Json(await new ActionPlanDAO().UpdateSheduleDate(name, value, pk, userprofile.User.ID), JsonRequestBehavior.AllowGet);
        }

        //public async Task<JsonResult> GetAllDataByCategory(int catid, string period, int? year)
        //{
        //    var currenYear = year ?? DateTime.Now.Year;
        //    return Json(new DataChartDAO().GetAllDataByCategory(catid, period, currenYear), JsonRequestBehavior.AllowGet);

        //}
    }
}