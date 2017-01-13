namespace JiGuangLinXin.App.Entities.ServicesModel
{
    public class JuheOrderList
    {
        public string reason;
        public int error_code;
        public JuheOrderState result;

    }
    public class JuheOrderState
    {
        public string uordercash;
        public string sporder_id;
        public string game_state;
    }

    public class JuheProvinceList
    {
        public string reason;
        public int error_code;
        public JuheProvince[] result;

    }
    public class JuheProvince
    {
        public string provinceId;
        public string provinceName;
    }

    public class JuheCityList
    {
        public string reason;
        public int error_code;
        public JuheCity[] result;

    }
    public class JuheCity
    {
        public string provinceId;
        public string cityId;
        public string cityName;
    }

    public class JuheProjectList
    {
        public string reason;
        public int error_code;
        public JuheProject[] result;

    }
    public class JuheProject
    {
        public string provinceId;
        public string cityId;
        public string payProjectId;
        public string payProjectName;
    }

    public class JuheUnitList
    {
        public string reason;
        public int error_code;
        public JuheUnit[] result;

    }
    public class JuheUnit
    {
        public string provinceId;
        public string cityId;
        public string payProjectId;
        public object payUnitId;
        public string payUnitName;
    }
    public class JuheProductList
    {
        public string reason;
        public int error_code;
        public JuheProduct result;
    }
    public class JuheProduct
    {
        public string productId;
        public string productName;
    }
    public class JuheBalance
    {
        public string reason;
        public int error_code;
        public JuheBalanceResult result;
    }
    public class JuheBalanceResult
    {
        public string account;
        public JuheBalances balances;
    }
    public class JuheBalances
    {
        public JuheBalanc balance;
    }
    public class JuheBalanc
    {
        public string balance;
        public string contractNo;
        public string payMentDay;

    }
    public class JuheTelcheck
    {
        public string reason;
        public int error_code;
        public JuheTelcheckResult result;
    }
    public class JuheTelcheckResult
    {
        public string inprice;
        public string game_area;
    }
    public class JuheTelorder
    {
        public string reason;
        public int error_code;
        public JuheTelorderResult result;
    }
    public class JuheTelorderResult
    {
        public string game_state;
        public string uorderid;
        public string sporder_id;
        public string mobilephone;
        public string cardname;
    }
    public class JuheOrder
    {
        public string reason;
        public int error_code;
        public JuheOrderResult result;
    }
    public class JuheOrderResult
    {
        public string orderid;
        public string uorderid;
        public string cardname;
    }
    /// <summary>
    /// 流量充值订单集合
    /// </summary>
    public class JuheTrafficOrder
    {
        public string reason;
        public int error_code;
        public JuheTrafficOrderResult result;
    }
    /// <summary>
    /// 流量充值订单
    /// </summary>
    public class JuheTrafficOrderResult
    {
        public string orderid;
        public string uorderid;
        public string cardname;
    }
}