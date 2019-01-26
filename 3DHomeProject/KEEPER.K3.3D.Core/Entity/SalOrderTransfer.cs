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
        /// 物料编码
        /// </summary>
        public long MATERIALID { get; set; }

        /// <summary>
        /// 辅助属性
        /// </summary>
        public long AUXPROPID { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public long Lot { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int amount { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string stocknumber { get; set; }
    }
}
