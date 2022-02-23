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

        public IActionResult OnGet()
        {
            bool isEdit = Id > 0;
            if (isEdit)
            {
                var userInfo = _adminUserInfoService.GetAdminUserInfo(Id);
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
    }
}