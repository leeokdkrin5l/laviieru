using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Senparc.Scf.Core.Models;
using Senparc.Scf.Service;

namespace Senparc.Areas.Admin.Areas.Admin.Pages
{
    public class AdminUserInfo_EditModel : BaseAdminPageModel
    {
        /// <summary>
        /// Id
        /// </summary>
        [BindProperty]
        public int Id { get; set; }
 
        public bool IsEdit { get; set; }

        [BindProperty]
        public AdminUserInfo AdminUserInfo { get; set; }
        //public CreateUpdate_AdminUserInfoDto AdminUserInfo { get; set; }

        private readonly AdminUserInfoService _adminUserInfoService;


        public AdminUserInfo_EditModel(AdminUserInfoService adminUserInfoService)
        {
            _adminUserInfoService = adminUserInfoService;
        }

        public IActionResult OnGet(int id)
        {
            bool isEdit = id > 0;
            if (isEdit)
            {
                var userInfo = _adminUserInfoService.GetAdminUserInfo(id);
                if (userInfo == null)
                {
                    throw new Exception("��Ϣ�����ڣ�");//TODO:��ʱ
                    return RenderError("��Ϣ�����ڣ�");
                }
                //AdminUserInfo.UserName = userInfo.UserName;
                //AdminUserInfo.Note = userInfo.Note;
                Id = userInfo.Id;
            }
            IsEdit = isEdit;
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            bool isEdit = id > 0;
            this.Validator(AdminUserInfo.UserName, "�û���", "UserName", false)
                .IsFalse(z => this._adminUserInfoService.CheckUserNameExisted(model.Id, z), "�û����Ѵ��ڣ�", true);

            if (!isEdit || !AdminUserInfo.Password.IsNullOrEmpty())
            {
                this.Validator(AdminUserInfo.Password, "����", "Password", false).MinLength(6);
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //AdminUserInfo userInfo = null;
            //if (isEdit)
            //{
            //    userInfo = _adminUserInfoService.GetAdminUserInfo(model.Id);
            //    if (userInfo == null)
            //    {
            //        return RenderError("��Ϣ�����ڣ�");
            //    }
            //}
            //else
            //{
            //    var passwordSalt = DateTime.Now.Ticks.ToString();
            //    userInfo = new AdminUserInfo()
            //    {
            //        PasswordSalt = passwordSalt,
            //        LastLoginTime = DateTime.Now,
            //        ThisLoginTime = DateTime.Now,
            //        AddTime = DateTime.Now,
            //    };
            //}

            //if (!model.Password.IsNullOrEmpty())
            //{
            //    userInfo.Password = this._adminUserInfoService.GetPassword(model.Password, userInfo.PasswordSalt, false);//��������
            //}

            //await this.TryUpdateModelAsync(userInfo, "", z => z.Note, z => z.UserName);
            //this._adminUserInfoService.SaveObject(userInfo);

            //base.SetMessager(MessageType.success, $"{(isEdit ? "�޸�" : "����")}�ɹ���");
            //return RedirectToAction("Index");

            return Page();
        }
    }
}