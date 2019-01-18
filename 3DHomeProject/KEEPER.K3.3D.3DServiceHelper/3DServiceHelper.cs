using KEEPER.K3._3D.Contracts;
using KEEPER.K3._3D.Core.ParamOption;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D._3DServiceHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class _3DServiceHelper
    {
        public static DynamicObject[] ConvertBills(Context ctx, ConvertOption option)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            DynamicObject[] targetDatas = service.ConvertBills(ctx, option);
            return targetDatas;

        }
    }
}
