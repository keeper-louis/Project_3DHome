using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Core.Entity
{
    [Description("altablein更新参数")]
    public class UpdateAltableinEntity
    {
        /// <summary>
        /// altable表主键
        /// </summary>
        public int altID { get; set; }

        /// <summary>
        /// k3cloud系统单据头内码
        /// </summary>
        public long k3cloudheadID { get; set; }

        /// <summary>
        /// k3cloud系统单据编号
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        /// k3cloud系统单据体内码
        /// </summary>
        public long k3cloudID { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string salenumber { get; set; }

        /// <summary>
        /// 销售订单行号
        /// </summary>
        public string linenumber { get; set; }

        /// <summary>
        /// 包装码
        /// </summary>
        public string packcode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorMsg { get; set; }
    }
}
