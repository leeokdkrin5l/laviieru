using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Senparc.CO2NET.Extensions;
using Senparc.Scf.Service;
using Senparc.Scf.XscfBase;

namespace Senparc.Areas.Admin.Areas.Admin.Pages.XscfModule
{
    public class StartModel : BaseAdminPageModel
    {
        public IXscfRegister XscfRegister { get; set; }
        public Senparc.Scf.Core.Models.DataBaseModel.XscfModule XscfModule { get; set; }

        XscfModuleService _xscfModuleService;

        public StartModel(XscfModuleService xscfModuleService)
        {
            _xscfModuleService = xscfModuleService;
        }

        public async Task OnGetAsync(string uid)
        {
            if (uid.IsNullOrEmpty())
            {
                throw new Exception("ģ�鲻���ڣ�");
            }

            XscfRegister = Senparc.Scf.XscfBase.Register.RegisterList.FirstOrDefault(z => z.Uid == uid);
            if (XscfRegister == null)
            {
                throw new Exception("ģ�鲻���ڣ�");
            }

            XscfModule = _xscfModuleService.GetObject(z => z.Uid == uid);

            if (XscfModule==null)
            {
                throw new Exception("ģ��δע�ᣡ");
            }

            Page();
        }
    }
}
