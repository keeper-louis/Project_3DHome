using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Core.ParamOption
{
    public enum UpdatePrtableinEnum
    {
        AfterGetDate = 0,//获取数据后
        AfterCreateModel = 1,//数据组装完成后反写k3cloud实际内码
        SaveError = 2,//数据保存失败
        AuditError = 3,//数据审核失败
        SaveSucess = 4,//保存成功
        AuditSucess = 5//审核成功
    }
}
