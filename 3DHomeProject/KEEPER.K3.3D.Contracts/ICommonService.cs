using KEEPER.K3._3D.Core.ParamOption;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Contracts
{
    /// <summary>
    /// 服务契约
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface ICommonService
    {
        /// <summary>
        /// 后台调用单据转换生成目标单
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="option">单据转换参数</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IEnumerable<DynamicObject> ConvertBills(Context ctx, ConvertOption option);

    }
}
