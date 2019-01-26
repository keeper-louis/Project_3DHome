using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Core.Entity
{
    public class SalOrderTransferList
    {
        /// <summary>
        /// 直接调拨单业务日期
        /// </summary>
        public DateTime BusinessDate { get; set; }

        /// <summary>
        /// 直接调拨单的分录信息
        /// </summary>
        public List<SalOrderTransfer> salOrderTransfer { get; set; }
    }
}
