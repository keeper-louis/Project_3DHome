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
        /// prtablein表的主键
        /// </summary>
        public long prtID { get; set; }

        /// <summary>
        /// prtablein表的fdate列
        /// </summary>
        public DateTime FDATE { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string saleNumber { get; set; }

        /// <summary>
        /// 销售订单行号
        /// </summary>
        public string lineNumber { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        public string technicsCode { get; set; }

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
