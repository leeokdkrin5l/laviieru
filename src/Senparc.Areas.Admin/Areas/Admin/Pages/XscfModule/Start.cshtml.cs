using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Senparc.CO2NET.Extensions;
using Senparc.Scf.Core.Enums;
using Senparc.Scf.Service;
using Senparc.Scf.XscfBase;

namespace Senparc.Areas.Admin.Areas.Admin.Pages
{
    public class XscfModuleStartModel : BaseAdminPageModel
    {
        public IXscfRegister XscfRegister { get; set; }
        public Senparc.Scf.Core.Models.DataBaseModel.XscfModule XscfModule { get; set; }
        public Dictionary<IXscfFunction, List<FunctionParammeterInfo>> FunctionParammeterInfoCollection { get; set; } = new Dictionary<IXscfFunction, List<FunctionParammeterInfo>>();

        XscfModuleService _xscfModuleService;
        IServiceProvider _serviceProvider;

        public string Msg { get; set; }
        public object Obj { get; set; }

        public XscfModuleStartModel(IServiceProvider serviceProvider, XscfModuleService xscfModuleService)
        {
            _serviceProvider = serviceProvider;
            _xscfModuleService = xscfModuleService;
        }

        public async Task OnGetAsync(string uid)
        {
            if (uid.IsNullOrEmpty())
            {
                throw new Exception("ģ����δ�ṩ��");
            }


            XscfModule = await _xscfModuleService.GetObjectAsync(z => z.Uid == uid).ConfigureAwait(false);

            if (XscfModule == null)
            {
                throw new Exception("ģ��δ��ӣ�");
            }

            XscfRegister = Senparc.Scf.XscfBase.Register.RegisterList.FirstOrDefault(z => z.Uid == uid);
            if (XscfRegister == null)
            {
                throw new Exception($"ģ�鶪ʧ��δ���أ�{Senparc.Scf.XscfBase.Register.RegisterList.Count}����");
            }

            foreach (var functionType in XscfRegister.Functions)
            {
                var function = _serviceProvider.GetService(functionType) as FunctionBase;//�磺Senparc.Xscf.ChangeNamespace.Functions.ChangeNamespace
                FunctionParammeterInfoCollection[function] = function.GetFunctionParammeterInfo().ToList();
            }

            Page();
        }

        public async Task<IActionResult> OnGetChangeStateAsync(int id, XscfModules_State toState)
        {
            var module = await _xscfModuleService.GetObjectAsync(z => z.Id == id).ConfigureAwait(false);

            if (module == null)
            {
                throw new Exception("ģ��δ��ӣ�");
            }

            module.UpdateState(toState);
            await _xscfModuleService.SaveObjectAsync(module);
            base.SetMessager(MessageType.success, "״̬����ɹ���");
            return RedirectToPage("Start", new { uid = module.Uid });
        }

    }
}
