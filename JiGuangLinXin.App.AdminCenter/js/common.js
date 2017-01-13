;(function($, window, document, undefined) {
    var common = {
    GetDate: function () {
        var date = new Date();
        return date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate();
    },
    /**
    * CustUploadFile 通用上传
    * @author 陈秘
    * @param {string} btnUpload 上传控件的ID
    * @param {string} eleId 隐藏字段ID
    * @param {array} folder 文件夹
    * @param {string} fileName 自定义文件名（文件名存到一个控件的val上）
    * @param {string} type 类型
    */
    ImagesUploadFileFun1: function (btnUpload, eleId, folder, fileName, type, eq) {
        eq = eq || 0;
        type = type || 'Image';
        //var custFileName = $('#'+fileName).val() || ''; 
         fileName = fileName || '';
        /*上传封面*/
        //上传附件
        new AjaxUpload('#'+btnUpload, {
            action: '/assets/js/file-upload/FileUploadHandler.ashx',
            data: { Folder: folder,FileName:fileName,Type: type },
            onSubmit: function (file, ext) {
                if (!/^(jpeg|jpg|gif|png|bmp)$/.test(ext)) {
                    alert("只能上传格式为(jpeg|jpg|gif|png|bmp)格式的文件！", "提示");
                    return false;
                } else {
                    //$("div[name='updata']").eq(eq).html("<img name='loadimgs' src='/assets/js/file-upload/loading.gif'/>");
                    $('#uploadTips').html('上传中，请稍候...');
                }
            },
            onComplete: function (file, response) {
                //$("div[name='updata']").eq(eq).html('');
                $('#uploadTips').html('');
                var responseText = response.toString();
                if (/MSIE 6.0/i.test(navigator.userAgent)) {
                }

                //显示1
                var reArray = responseText.split('|');
                if (reArray.length < 2) { //异常
                    alert(reArray[0]);
                    return false;
                } else if (reArray[0] == null || reArray[0] == "") {
                    alert("上传失败！");
                    return false;
                } else {
                    //用于记录，上传的图片;文件夹+文件名
                    var newV =  reArray[1] + "/" + reArray[0];
                    $("#"+eleId).val(newV);
                    $("#img"+eleId).attr("src", reArray[2]);
                }
            }
        });
    },

    ImagesUploadFileFun2: function() {
        
    },

    /*  html字符串格式化
    使用方式：
            var html = "<div>{{con}}</div>";
            var ret = StringFormat(html, { con: "模块加载" });
    ------------------------------------------------------------------------------*/
    StringFormat: function(tpl, o, r, i) {
        var _has = {}.hasOwnProperty;
        var isFn = _type('Function'),
            isFunction = function(s) { return isFn(s) && s.call; },
            TPLPROP_RE = /\{\{([\w~%]+)\}\}/g,
            _toString = {}.toString;


        function _type(t) {
            return function (s) {
                return _toString.call(s) === '[object ' + t + ']';
            };
        }

        /* 转化成整形数字
        */
        function parseInt(n) {
            return parseInt(n, 10);
        }

        function _A() {
            return this._slice.call.apply(this._slice, arguments)
        }
        function isEmpty(v) {
            return v === null || v === "";
        }
        function result1(v, o) {
            return isFunction(v) ? v.apply(o, _A(arguments, 2)) : v;
        }

         return isFunction(tpl) ? tpl(o) : (tpl + '').replace(r || TPLPROP_RE,
            function (a, b) {
                var v = b.indexOf('%') === 0 ? (i + (parseInt(b.slice(1)) || 0)) : (b == '~' ? o : result1(o[b], o));
                return isEmpty(v) ? '' : v;
            });
    },
    /*数字类型格式化 ，v：小数点位数*/
    NumGfix:function(value, v) {
        if (typeof v !== 'undefined') {
            return parseFloat(value).toFixed(v);
        }
        return parseFloat(value).toFixed(2);
    },
     /*
    获取指定的URL参数值
    参数：paramName URL参数
    调用方法:getParam("key")
    返回值:value
    */
    GetParam:function(paramName) {
        var paramValue = "", isFound = false;
        if (window.location.search.indexOf("?") == 0 && window.location.search.indexOf("=") > 1) {
            var arrSource = unescape(window.location.search).substring(1, window.location.search.length).split("&"), i = 0;
            while (i < arrSource.length && !isFound) {
                if (arrSource[i].indexOf("=") > 0) {
                    if (arrSource[i].split("=")[0].toLowerCase() == paramName.toLowerCase()) {
                        paramValue = arrSource[i].split("=")[1];
                        isFound = true;
                    }
                }
                i++;
            }
        }
        return paramValue;
    },
      /*
    *正则验证
    */
    valid:function (exp, o) {
        return exp.test(o);
    },
    /*
    *浏览器URL跳转
    */
    GoUrl:function(url) {
        if (url.indexOf('/') == 0) {
            url = 'http://' + window.location.host + url;
        }
        window.location.href = url;
    },
     isIntege: function (o) {  //正整数,>0
            return common.valid(/^[1-9]\d*$/, o);
        },
    isMobilePhone: function (o) {  //手机
        return common.valid(/^(13|15|18|17)[0-9]{9}$/, o);
    },
    isTelPhone: function (o) {  //座机
        return common.valid(/^(([0\+]\d{2,3}-)?(0\d{2,3})-)?(\d{7,8})(-(\d{3,}))?$/, o);
    },
    isLinkPhone: function (o) {  //座机+手机
        return common.isMobilePhone(o) || common.isTelPhone(o);
    },
    isEmpty: function (o) {  //非空:包括未声明，null，空字符
        return typeof (o) == "undefined" || o == null || common.valid(/^\s*$/, o);
    },
    isUrl: function (o) {  //URl
        return common.valid(/^(\w+:\/\/)?\w+(\.\w+)+.*$/, o);
    }
};

    window.common = common;
    /*----------------------------字符串常用操作---------------------------------------*/
    $.extend(String.prototype, {
        //字符串转数字，保留小数点后几位
        sfix: function (v) {
            if (typeof v !== 'undefined') {
                return parseFloat(this).toFixed(v);
            }
            return parseFloat(this).toFixed(2);
        },
        //去除空格
        trim: function () {
            return this.replace(/^\s+|\s+$/g, '');
        }
    });
    /*---------------------------字符串常用操作 end---------------------------------------*/


    /*----------------------------日期常用操作 ---------------------------------------*/
    $.extend(Date.prototype, {
        // 对Date的扩展，将 Date 转化为指定格式的String   
        // 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符，   
        // 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)   
        // 例子：   
        // (new Date()).format("yyyy-MM-dd hh:mm:ss.S") ==> 2015-01-12 09:09:04.423   
        // (new Date()).format("yyyy-M-d h:m:s.S")      ==> 2015-01-12 9:9:4.18   
        format: function (fmt) {
            var o = {
                "M+": this.getMonth() + 1,                 //月份   
                "d+": this.getDate(),                    //日   
                "h+": this.getHours(),                   //小时   
                "m+": this.getMinutes(),                 //分   
                "s+": this.getSeconds(),                 //秒   
                "q+": Math.floor((this.getMonth() + 3) / 3), //季度   
                "S": this.getMilliseconds()             //毫秒   
            };
            if (/(y+)/.test(fmt))
                fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
            for (var k in o)
                if (new RegExp("(" + k + ")").test(fmt))
                    fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
            return fmt;
        },
        getWeek: function () {
            var day = this.getDay(), week = "";
            if (day == 0) week = "星期日";
            if (day == 1) week = "星期一";
            if (day == 2) week = "星期二";
            if (day == 3) week = "星期三";
            if (day == 4) week = "星期四";
            if (day == 5) week = "星期五";
            if (day == 6) week = "星期六";
            return week;
        }
    });
    /*----------------------------日期常用操作 end---------------------------------------*/


})(jQuery, window, document);