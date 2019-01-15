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
        /// 源单标识
        /// </summary>
        public string SourceFormId { get; set; }

        /// <summary>
        /// 目标单标识
        /// </summary>
        public string TargetFormId { get; set; }

        /// <summary>
        /// 单据转换规则KEY
        /// </summary>
        public string ConvertRuleKey { get; set; }
    }
}
