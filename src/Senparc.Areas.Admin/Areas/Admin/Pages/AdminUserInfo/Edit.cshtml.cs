using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        /// <summary>
        /// ����
        /// </summary>		
        public string Password { get; set; }
        /// <summary>
        /// ������
        /// </summary>		
        public string PasswordSalt { get; set; }
        /// <summary>
        /// ��ʵ����
        /// </summary>		
        public string RealName { get; set; }
        /// <summary>
        /// �ֻ���
        /// </summary>		
        public string Phone { get; set; }
        /// <summary>
        /// ��ע
        /// </summary>		
        public string Note { get; set; }
        /// <summary>
        /// ��ǰ��¼ʱ��
        /// </summary>		
        public DateTime ThisLoginTime { get; set; }
        /// <summary>
        /// ��ǰ��¼IP
        /// </summary>		
        public string ThisLoginIP { get; set; }
        /// <summary>
        /// �ϴε�¼ʱ��
        /// </summary>		
        public DateTime LastLoginTime { get; set; }
        /// <summary>
        /// �ϴε�¼Ip
        /// </summary>		
        public string LastLoginIP { get; set; }
        /// <summary>
        /// ���ʱ��
        /// </summary>		
        public DateTime AddTime { get; set; }
        public bool IsEdit { get; set; }

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
                UserName = userInfo.UserName;
                Note = userInfo.Note;
                Id = userInfo.Id;
            }
            IsEdit = isEdit;
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            bool isEdit = id > 0;
            //this.Validator(model.UserName, "�û���", "UserName", false)
            //    .IsFalse(z => this._adminUserInfoService.CheckUserNameExisted(model.Id, z), "�û����Ѵ��ڣ�", true);

            //if (!isEdit || !model.Password.IsNullOrEmpty())
            //{
            //    this.Validator(model.Password, "����", "Password", false).MinLength(6);
            //}

            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

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