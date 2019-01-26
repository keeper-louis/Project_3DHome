using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Core.Entity
{
    /// <summary>
    /// 支持采购件直接调拨单的销售订单实体类
    /// </summary>
    public class SalOrderTransfer
    {
        /// <summary>
        /// 调出库存组织 or 调入库存组织
        /// </summary>
        public long orgID { get; set; }
        

    }
}
