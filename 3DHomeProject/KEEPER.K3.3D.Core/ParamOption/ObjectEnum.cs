using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Core.ParamOption
{
    /// <summary>
    /// 处理的业务对象列表
    /// </summary>
    public enum ObjectEnum
    {
        PurTransfer,//采购件调拨
        OPLAQual,//合格品工序汇报
        OPLAUnQual,//不合格品工序汇报
        AlPurTransfer,//直接调拨接口采购件调拨
        AlInStock,//直接调拨接口简单生产入库
        AlTransfer,//直接调拨接口直接调拨
        SO2DE//销售订单下推销售出库

    }
}
