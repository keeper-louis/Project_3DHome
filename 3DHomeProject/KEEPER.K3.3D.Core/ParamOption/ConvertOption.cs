using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Core.ParamOption
{
    public class ConvertOption
    {

        /// <summary>
        /// prtableIn表主键集合
        /// </summary>
        public Dictionary<string, int> dic { get; set; }
        /// <summary>
        /// prtableIn表数据主键
        /// </summary>
        public List<long> prtInId { get; set; }

        /// <summary>
        /// 源单标识
        /// </summary>
        public string SourceFormId { get; set; }

        /// <summary>
        /// 业务日期
        /// </summary>
        public DateTime FDATE { get; set; }
        /// <summary>
        /// 目标单标识
        /// </summary>
        public string TargetFormId { get; set; }

        /// <summary>
        /// 单据转换规则KEY
        /// </summary>
        public string ConvertRuleKey { get; set; }

        /// <summary>
        /// 源单ids
        /// 如果按整单下推，该参数可用集合长度>1
        /// 如果按明细下推，该参数集合长度=1
        /// </summary>
        public List<long> SourceBillIds { get; set; }

        /// <summary>
        /// 源单明细ids
        /// </summary>
        public List<long> SourceBillEntryIds { get; set; }

        /// <summary>
        /// 源单明细实体key
        /// </summary>
        public string SourceEntryEntityKey { get; set; }

        /// <summary>
        /// 合格或不合格数量
        /// </summary>
        public List<int> mount { get; set; }
    }
}
