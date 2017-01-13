namespace JiGuangLinXin.App.Entities.BaseEnum
{
    #region 通用
    /// <summary>
    /// 设备/平台 枚举
    /// </summary>
    public enum DriversEnum
    {
        Default = 0,//默认，一般为平台后台管理系统
        Android = 1,
        Ios = 2,
        BusinessCenter = 3
    }
    /// <summary>
    /// 业务模块枚举
    /// </summary>
    public enum ModuleEnum
    {
        Default = 0,
        登录 = 1,
        充值 = 2,
        提现 = 3,
        邀约 = 4,
        活动 = 5,
        红包 = 6,
        银联便民 = 7,
        注册 = 8,
        个人信息 = 9,
        九宫格抽奖 = 50,
        发短信 = 100
    }
    /// <summary>
    /// 页面标识
    /// </summary>
    public enum PageEnum
    {
        首页 = 0,
        社区中心首页 = 1,
        精品汇频道页 = 2,
        社区服务频道页 = 3,
        消息频道页 = 4,
        帮助中心页 = 5
    }

    /// <summary>
    /// 通用的 操作枚举
    /// </summary>
    public enum OperateStatusEnum
    {
        /// <summary>
        /// 默认值 0，表示正常
        /// </summary>
        Default = 0,
        /// <summary>
        /// 值为1，标识已经变动，如：已删除、已取消等
        /// </summary>
        Operate = 1
    }

    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum SexEnum
    {
        女 = 0,
        男 = 1,
        不详 = 2
    }
    /// <summary>
    /// 附件的类型
    /// </summary>
    public enum AttachmentTypeEnum
    {
        图片 = 1,
        视频 = 2
    }
    /// <summary>
    /// 文件上传，结果标识
    /// </summary>
    public enum FileUploadStateEnum
    {
        上传成功 = 0,
        文件太大超出限制 = 1,
        上传失败 = 2
    }

    /// <summary>
    /// 附件文件夹
    /// </summary>
    public enum AttachmentFolderEnum
    {
        /// <summary>
        ///楼盘视频素材
        /// </summary>
        video,
        /// <summary>
        /// 全景户型-楼盘立体图，注意：文件名以0123456数字开头
        /// </summary>
        cube,
        /// <summary>
        /// 区位展示
        /// </summary>
        location,
        /// <summary>
        /// 景观图片
        /// </summary>
        landscape,
        /// <summary>
        /// 物业配套
        /// </summary>
        property,
        /// <summary>
        /// 建筑规划
        /// </summary>
        planning,

        /// <summary>
        /// 各种验证图片（具体是什么图片，参照附件的备注）
        /// </summary>
        identification,

        /// <summary>
        /// 社区/小区
        /// </summary>
        community,
        /// <summary>
        /// 话题(邻友圈)
        /// </summary>
        topic,
        /// <summary>
        /// 社区相册
        /// </summary>
        album,
        /// <summary>
        /// 社区互动
        /// </summary>
        interactive,
        /// <summary>
        /// 用户头像
        /// </summary>
        avatar,
        /// <summary>
        /// 小区服务商
        /// </summary>
        servicer,
        /// <summary>
        /// 商品图片
        /// </summary>
        goods,
        /// <summary>
        /// 活动中心
        /// </summary>
        events,
        /// <summary>
        /// 楼盘素材
        /// </summary>
        building,
        /// <summary>
        /// messageCenter 消息中心图片
        /// </summary>
        mc,
        /// <summary>
        /// 广告推广图片、附件
        /// </summary>
        ad,
        /// <summary>
        /// 首页推荐
        /// </summary>
        home,
        /// <summary>
        /// 邻里团
        /// </summary>
        groupbuy,
        /// <summary>
        /// APP安装包目录
        /// </summary>
        appdownload,

    }
    /// <summary>
    /// 会员各种反馈信息标识
    /// </summary>
    public enum FeedbackEnum
    {
        反馈 = 0,
        举报 = 1,
        系统 = 2  //如银联销帐，支付宝支付成功，销帐失败
    }

    /// <summary>
    /// 通用页面标识
    /// </summary>
    public enum ProtocolEnum
    {
        用户协议 = 1,
        关于我们 = 2,
        邻里团介绍 = 3,
        App下载H5 = 4,



        邻里团 = 10,
        /// <summary>
        /// 投票活动、报名活动
        /// </summary>
        社区活动 = 11,
        楼盘推荐 = 12,
        全景看房 = 13,
        游戏中心 = 14,
        社区中心 = 15,
        商家服务 = 16,
        群主中心 = 17,
    }

    #endregion

    #region  极光消息推送
    /// <summary>
    /// 极光推送的消息各种标识
    /// </summary>
    public enum PushMessageEnum
    {

        默认 = 0,
        好友申请 = 1,//按照 uid, nickname 跳转
        用户跳转 = 2,
        社区服务跳转 = 3,//按照 uid, nickname 跳转
        精品汇跳转 = 4,  //按照 proId,proName 跳转
        /// <summary>
        /// 社区活动跳转，非常特殊，跳转到一个指定的URL，url存放在 JPushMsgModel 的logo临时属性中
        /// </summary>
        社区活动跳转 = 5, //按照   proName logo跳转

        审核通过 = 10,//小区社区认证：因为审核成功，需要通知客户端更改 审核状态
        审核失败 = 11,

        禁言 = 12,
        解禁 = 13,

        管理员审核通过 = 14, //用户申请成为管理员
        管理员审核失败 = 15
    }

    #endregion
    #region 日志 消息
    /// <summary>
    /// 日志错误标识
    /// </summary>
    public enum ErrorLogEnum
    {
        /// <summary>
        /// 默认错误
        /// </summary>
        Default = 0,
        /// <summary>
        /// 系统内部错误(App接口错误)
        /// </summary>
        System = 1,
        /// <summary>
        /// 商家管理后台
        /// </summary>
        Business = 2,
        /// <summary>
        /// 第三方服务异常：推送等
        /// </summary>
        Service = 3,
        /// <summary>
        /// 支付宝日志
        /// </summary>
        Alipay = 4,
        /// <summary>
        /// 短信
        /// </summary>
        Sms = 5,
    }

    /// <summary>
    /// 推送的消息类型
    /// </summary>
    public enum MessageTypeEnum
    {
        Default = 0,
        业务消息 = 1
    }
    /// <summary>
    /// 推送的消息目标枚举
    /// </summary>
    public enum MessageTargetEnum
    {
        所有人 = 0,
        单人 = 1,
        指定群 = 2
    }
    #endregion

    #region 用户
    /// <summary>
    /// 认证状态枚举
    /// </summary>
    public enum AuditingEnum
    {
        未认证 = 0,
        认证成功 = 1,
        认证失败 = 2,
        认证中 = 3
    }

    /// <summary>
    /// 会员状态枚举
    /// </summary>
    public enum UserStatusEnum
    {
        正常 = 0,
        冻结 = 1,
        禁言 = 2
    }


    public enum MemberRoleEnum
    {
        平台 = 0,
        会员 = 1,
        商家 = 2
    }


    /// <summary>
    /// 管理员角色
    /// </summary>
    public enum ManagerRoleEnum
    {
        超级管理员 = 0,
        商家管理员 = 1,
        系统业务员 = 2,
        小区管理员 = 3,
        系统财务员 = 4

    }

    /// <summary>
    /// 商家分类枚举
    /// </summary>
    public enum BusinessFlagEnum
    {
        社区商家 = 0,
        开发商 = 1
    }


    #endregion


    #region 话题
    /// <summary>
    /// 邀约类型
    /// </summary>
    public enum InviteTypeEnum
    {
        话题 = 0,
        报名 = 1
    }

    /// <summary>
    /// 邻里圈商家商家推广位类型
    /// </summary>
    public enum ShowcaseFlagEnum
    {
        纯图片 = 0,
        图片链接 = 1,
        视频 = 2,
        精品汇 = 3,
        新家推荐 = 4
    }

    #endregion

    #region 楼盘

    /// <summary>
    /// 楼盘标识
    /// </summary>
    public enum BuildingFlagEnum
    {
        热销 = 1,
        特价 = 2,
        新盘 = 3
    }

    /// <summary>
    /// 楼盘分类
    /// </summary>
    public enum BuildingTypeEnum
    {
        精品小户 = 1,
        别墅洋房 = 2
    }


    #endregion

    #region 红包
    /// <summary>
    /// 红包的分类
    /// </summary>
    public enum LuckGiftTypeEnum
    {
        群红包 = 0,
        单个红包 = 1,
        商家推广红包 = 2,
        社区互动红包 = 3,
        新家推荐红包 = 4,
        邻友圈用户红包 = 5
    }
    /// <summary>
    /// 红包的标识
    /// </summary>
    public enum LuckGiftFlagEnum
    {
        没有红包 = 0,
        有红包 = 1,
        红包被领光 = 2
    }

    /// <summary>
    /// 红包的状态
    /// </summary>
    public enum LuckGiftStateEnum
    {
        正常 = 0,
        已过期 = 1,
    }

    #endregion

    #region 广告

    /// <summary>
    /// 广告投放目标
    /// </summary>
    public enum AdTargetEnum
    {
        全平台广告 = 0,
        小区定向广告 = 1
    }
    /// <summary>
    /// 广告分类
    /// </summary>
    public enum AdTypeEnum
    {
        纯文字 = 0,
        纯图片 = 1,
        图文 = 2,
        视频 = 3
    }

    #endregion

    #region 聚合数据

    /// <summary>
    /// 支付的类型
    /// </summary>
    public enum PaymentTypeEnum
    {
        水费 = 1,
        电费 = 2,
        燃气费 = 3,
        话费 = 4,
        活动话费 = 9,
        充值 = 10
    }


    /// <summary>
    /// 销帐类型
    /// </summary>
    public enum PayOffEnum
    {
        未销帐 = 0,
        已销帐 = 1,
        销帐失败 = 2,
        已退款 = 3
    }

    #endregion

    #region 订单状态

    /// <summary>
    ///支付状态
    /// </summary>
    public enum PayStateEnum
    {
        未付款 = 0,
        已付款 = 1
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStateEnum
    {
        待确认 = 0,
        已完成 = 1,
        已取消 = 2,
        待发货 = 3,
        已发货 = 4,
    }

    /// <summary>
    /// 邻里团状态
    /// </summary>
    public enum GroupBuyOrderStateEnum
    {
        已完成 = 1,
        已退款 = 2,
        待发货 = 3,
        已发货 = 4,
    }

    /// <summary>
    /// 邻里团标识
    /// </summary>
    public enum GroupBuyOrderFlagEnum
    {
        已成团 = 1,
        组团失败 = 2,
        组团中 = 3,
    }

    #endregion

    #region 券、卡

    /// <summary>
    /// 卡使用状态
    /// </summary>
    public enum CardUseStateEnum
    {
        未消费 = 0,
        已消费 = 1,
        已删除 = 2
    }

    /// <summary>
    /// 卡销售状态
    /// </summary>
    public enum CardSaleStateEnum
    {
        正常 = 0,
        已下架 = 1,
        已删除 = 2
    }


    /// <summary>
    /// 业主卡购买渠道
    /// </summary>
    public enum OwnerCardWays
    {
        管理员推广 = 0,
        物业 = 1,
        代理商 = 2,
        卡密兑换 = 3

    }
    /// <summary>
    /// 业主卡Flag
    /// </summary>
    public enum OwnerCardFlagEnum
    {

        和谐卡 = 1,
        互敬卡 = 2,
        友爱卡 = 3
    }




    #endregion
    #region  账单相关
    /// <summary>
    /// 账单流水类型
    /// </summary>
    public enum BillEnum
    {
        Default = 0,
        充值 = 1,
        /// <summary>
        /// 水电气
        /// </summary>
        便民缴费 = 2,
        商品购买 = 3,
        红包 = 4,   //用户发的聊天、话题红包
        提现 = 5,
        便民缴费返还 = 6,
        平台业主卡 = 7,
        商家抵用券 = 8,
        邻里团 = 9,
        楼盘红包 = 10,
        话费充值85折 = 11,
        聊天红包退款 = 20,
        便民退款 = 30,
        邻里团退款 = 31


    }
    /// <summary>
    /// 账单标识，是否进入平台流水记录
    /// </summary>
    public enum BillFlagEnum
    {
        普通流水 = 0,
        平台流水 = 1
    }
    public enum BillFlagModuleEnum
    {
        /// <summary>
        /// 聚合
        /// </summary>
        官方平台 = 0,
        /// <summary>
        /// 银联
        /// </summary>
        第三方平台 = 1
    }

    /// <summary>
    /// 申请提现的方式
    /// </summary>
    public enum ApplyCashWayEnum
    {
        支付宝 = 0
    }
    /// <summary>
    /// 提现状态
    /// </summary>
    public enum ApplyCashStateEnum
    {
        等待审核 = 0,
        提现成功 = 1,
        提现失败 = 2
    }



    #endregion

    #region 活动
    /// <summary>
    /// 活动类型枚举
    /// </summary>
    public enum ActiveTypeEnum
    {
        普通活动 = 0,
        话费活动 = 1,
        商品活动 = 2,
        管理员活动 = 3
    }


    /// <summary>
    /// 活动参与历史的标识
    /// </summary>
    public enum EventHistoryFlagEnum
    {
        社区互动 = 1,
        商家活动 = 2,
        楼盘活动 = 3
    }
    /// <summary>
    /// 活动标识
    /// </summary>
    public enum EventFlagEnum
    {
        报名 = 1,
        投票 = 2,
        邻里团 = 3,
        封面链接 = 4,
        销售产品 = 5
    }

    /// <summary>
    /// H5页面各种活动的分类标识
    /// </summary>
    public enum EventH5ModuleEnum
    {
        签到送流量 = 1,
        老虎机送电影票 = 2,
        退返积分 = 3
    }
    /// <summary>
    /// 邻信电影季活动的各种标识
    /// </summary>
    public enum FilmFlagEnum
    {
        默认 = 0,
        电影票 = 1,
        业主卡 = 2,
        红包 = 3,
        流量 = 4,
        积分 = 5
    }



    #endregion
    /// <summary>
    /// 评论类型枚举
    /// </summary>
    public enum CommentTypeEnum
    {
        商家活动 = 1,
        邻友圈话题 = 2,
        社区互动 = 3
    }
    /// <summary>
    /// 社区互动标识枚举
    /// </summary>
    public enum InteractiveFlagEnum
    {
        社区互动 = 0,
        投票互动 = 1
    }
    /// <summary>
    /// 消息中心模块
    /// </summary>
    public enum MessageCenterModuleEnum
    {
        社区服务 = 1000,
        社区活动 = 1001,
        游戏中心 = 1002,
        附件的人 = 1003,
        精品汇 = 1004,
        邻里圈 = 1005,
        邻妹妹 = 1006,
        新家推荐 = 1007,
        用户认证 = 1008,
        便民缴费 = 1009

    }
    /// <summary>
    /// 首页推荐中心模块代码
    /// </summary>
    public enum IndexRecommedEnum
    {
        社区服务 = 1,
        便民购 = 2,
        活动中心 = 3,
        楼盘推荐 = 4,
        邻里团 = 5
    }
    /// <summary>
    /// 邻里团 状态
    /// </summary>
    public enum GroupbuyStateEnum
    {
        团购中 = 0,
        团购成功 = 1,
        团购失败 = 2,
    }

    /// <summary>
    /// 审核认证的模块
    /// </summary>
    public enum CheckHistoryStateEnum
    {
        商家入驻 = 1,
        精品汇 = 2,
        邻里团 = 3,
        商家活动 = 4,

        用户认证 = 11,
        群主认证 = 12,

        商家提现 = 21,
        用户提现 = 22,
        群管理员审核 = 23,

        用户退款 = 30
    }


}
