using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Senparc.CO2NET.Extensions;
using Senparc.Scf.Core.Enums;
using Senparc.Scf.Core.Models;
using Senparc.Scf.Core.Validator;
using Senparc.Scf.Service;

namespace Senparc.Areas.Admin.Areas.Admin.Pages
{
    public class AdminUserInfo_EditModel : BaseAdminPageModel, IValidatorEnvironment
    {
        /// <summary>
        /// Id
        /// </summary>
        [BindProperty]
        public int Id { get; set; }

        public bool IsEdit { get; set; }


        [BindProperty]
        public CreateOrUpdate_AdminUserInfoDto AdminUserInfo { get; set; } = new CreateOrUpdate_AdminUserInfoDto();
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
                AdminUserInfo.UserName = userInfo.UserName;
                AdminUserInfo.Note = userInfo.Note;
                Id = userInfo.Id;
            }
            IsEdit = isEdit;
            return Page();
        }

        public IActionResult OnPost()
        {
            bool isEdit = Id > 0;
            this.Validator(AdminUserInfo.UserName, "�û���", "UserName", false)
                .IsFalse(z => this._adminUserInfoService.CheckUserNameExisted(Id, z), "�û����Ѵ��ڣ�", true);
            if (!isEdit)
            {
                if (!AdminUserInfo.Password.IsNullOrEmpty())
                {
                    this.Validator(AdminUserInfo.Password, "����", "Password", false).MinLength(6);
                }
            }
            else
            {
                this.Validator(AdminUserInfo.Password, "����", "Password", false).MinLength(6);
            }


            if (!ModelState.IsValid)
            {
                return Page();
            }

            AdminUserInfo userInfo = null;
            if (isEdit)
            {
                userInfo = _adminUserInfoService.GetAdminUserInfo(Id);
                if (userInfo == null)
                {
                    return RenderError("��Ϣ�����ڣ�");
                }

                userInfo.UpdateObject(AdminUserInfo.UserName, AdminUserInfo.Password);
            }
            else
            {
                string userName = AdminUserInfo.UserName;
                string password = AdminUserInfo.Password;
                userInfo = new AdminUserInfo(ref userName, ref password, null, null, AdminUserInfo.Note);
            }

            //await this.TryUpdateModelAsync(userInfo, "", z => z.Note, z => z.UserName);
            this._adminUserInfoService.SaveObject(userInfo);

            base.SetMessager(MessageType.success, $"{(isEdit ? "�޸�" : "����")}�ɹ���");
            return RedirectToPage("./Index");
        }
    }
}