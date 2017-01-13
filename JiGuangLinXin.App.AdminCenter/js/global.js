;(function($, window, document, undefined) {
    var common = {
    GetDate: function () {
        var date = new Date();
        return date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate();
    },
    /***********************************valiForm封装部分*****************************************/
    //验证配置
    validConfig: {
        btnSubmit: "", //指定当前表单下的哪一个按钮触发表单提交事件
        btnReset: "", //该表单下要绑定点击重置表单事件的按钮;
        tiptype: 3, //可用的值有：1、2、3、4和function函数，默认tiptype为1
        /*
        可用的值有：1、2、3、4和function函数，默认tiptype为1。 3、4是5.2.1版本新增
        1=> 自定义弹出框提示；
        2=> 侧边提示(会在当前元素的父级的next对象的子级查找显示提示信息的对象，表单以ajax提交时会弹出自定义提示框显示表单提交状态)；
        3=> 侧边提示(会在当前元素的siblings对象中查找显示提示信息的对象，表单以ajax提交时会弹出自定义提示框显示表单提交状态)；
        4=> 侧边提示(会在当前元素的父级的next对象下查找显示提示信息的对象，表单以ajax提交时不显示表单的提交状态)；
        如果上面的4种提示方式不是你需要的，你可以给tiptype传入自定义函数。通过自定义函数，可以实现你想要的任何提示效果：

        tiptype:function(msg,o,cssctl){
        //msg：提示信息;
        //o:{obj:*,type:*,curform:*},
        //obj指向的是当前验证的表单元素（或表单对象，验证全部验证通过，提交表单时o.obj为该表单对象），
        //type指示提示的状态，值为1、2、3、4， 1：正在检测/提交数据，2：通过验证，3：验证失败，4：提示ignore状态, 
        //curform为当前form对象;
        //cssctl:内置的提示信息样式控制函数，该函数需传入两个参数：显示提示信息的对象 和 当前提示的状态（既形参o中的type）;
        }*/
        ignoreHidden: false, //默认为false，当为true时对:hidden的表单元素将不做验证;
        dragonfly: false, //值为空时不做验证；
        tipSweep: false, //是否表单提交时才显示验证信息
        label: "",
        showAllError: false, //默认为false，true：提交表单时所有错误提示信息都会显示；false：一碰到验证不通过的对象就会停止检测后面的元素，只显示该元素的错误信息；
        postonce: false, //默认为false，指定是否开启二次提交防御，true开启，不指定则默认关闭；为true时，在数据成功提交后，表单将不能再继续提交。
        ajaxPost: false, //默认为false，使用ajax方式提交表单数据，将会把数据POST到config方法或表单action属性里设定的地址；
        datatype: {
            "*6-20": /^[^\s]{6,20}$/,
            "z2-4": /^[\u4E00-\u9FA5\uf900-\ufa2d]{2,4}$/,
            "username": function (gets, obj, curform, regxp) {
                //参数gets是获取到的表单元素值，obj为当前表单元素，curform为当前验证的表单，regxp为内置的一些正则表达式的引用;
                var reg1 = /^[\w\.]{4,16}$/,
				reg2 = /^[\u4E00-\u9FA5\uf900-\ufa2d]{2,8}$/;

                if (reg1.test(gets)) { return true; }
                if (reg2.test(gets)) { return true; }
                return false;

                //注意return可以返回true 或 false 或 字符串文字，true表示验证通过，返回字符串表示验证失败，字符串作为错误提示显示，返回false则                  用errmsg或默认的错误提示;
            },
            "phone": function () {
                // 5.0 版本之后，要实现二选一的验证效果，datatype 的名称 不 需要以 "option_" 开头;	
            },
            "*6-16": /^[^\s]{6,16}$/,
            money:/^\d{1,12}(?:\.\d{1,4})?$/,
            CardID12: /^\d{12}$/,
            CardID16: /^\d{16}$/,
            intege: /^-?[1-9]\d*$/, 				//整数
            intege1: /^[1-9]\d*$/, 				//正整数
            intege2: /^-[1-9]\\d*$/, 				//负整数
            regnum: /^([+-]?)\d*\.?\d+$/, 		//数字
            num1: /^[0-9]+$/, 				//正数（正整数 + 0）
            num2: /^-[1-9]\\d*|0$/, 				//负数（负整数 + 0）
            decmal: /^([+-]?)\\d*\\.\\d+$/, 		//浮点数
            decmal1: /^(\d*\.)?\d+$/, //正浮点数
            decmal2: "/^-([1-9]\\d*.\\d*|0.\\d*[1-9]\\d*)$/", //负浮点数
            decmal3: "/^-?([1-9]\\d*.\\d*|0.\\d*[1-9]\\d*|0?.0+|0)$/", //浮点数
            decmal4: /^[1-9]\d*.\d*|0.\d*[1-9]\d*|0?.0+|0$/, //非负浮点数（正浮点数 + 0）
            decmal5: "/^(-([1-9]\\d*.\\d*|0.\\d*[1-9]\\d*))|0?.0+|0$/", //非正浮点数（负浮点数 + 0）
            email: /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/, //邮件
            color: "/^[a-fA-F0-9]{6}$/", 			//颜色
            chinese: "/^[\\u4E00-\\u9FA5\\uF900-\\uFA2D]+$/", 				//仅中文
            ascii: "/^[\\x00-\\xFF]+$/", 			//仅ACSII字符
            zipcode: /^\d{6}$/, 					//邮编
            mobile: /^(13|15)[0-9]{9}$/, 			//手机
            camera: /^[0-9]{3}-[0-9]{8}$|^[0-9]{4}-[0-9]{7}$/, //座机
            mobile: /^(13|15|18|17)[0-9]{9}$/, 			//手机
            ip4: "/^(25[0-5]|2[0-4]\\d|[0-1]\\d{2}|[1-9]?\\d)\\.(25[0-5]|2[0-4]\\d|[0-1]\\d{2}|[1-9]?\\d)\\.(25[0-5]|2[0-4]\\d|[0-1]\\d{2}|[1-9]?\\d)\\.(25[0-5]|2[0-4]\\d|[0-1]\\d{2}|[1-9]?\\d)$/", //ip地址
            notempty: "/^\\S+$/", 					//非空
            picture: "/(.*)\\.(jpg|bmp|gif|ico|pcx|jpeg|tif|png|raw|tga)$/", //图片
            rar: "/(.*)\\.(rar|zip|7zip|tgz)$/", 							//压缩文件
            date: /^\d{4}(\-|\/|\.)\d{1,2}\1\d{1,2}$/, 				//日期
            qq: /^[1-9]*[1-9][0-9]*$/, 			//QQ号码
            tel: /^(([0\+]\d{2,3}-)?(0\d{2,3})-)?(\d{7,8})(-(\d{3,}))?$/, //电话号码的函数(包括验证国内区号,国际区号,分机号)
            username: /^(?!0-9)[_a-z0-9]{6,16}$/,
            "username2-10": /^\\w{2,10}$/, 				//用来用户注册。匹配由数字、26个英文字母或者下划线组成的字符串
            letter: /^[A-Za-z]+$/, 				    //字母
            "letter2-10": /^[A-Za-z]{2,10}$/,
            letter_u: "^/[A-Z]+$/", 				//大写字母
            letter_l: "/^[a-z]+$/", 				//小写字母
            filter: "/[^`~!@#$^&]/",
            url: /^(\w+:\/\/)?\w+(\.\w+)+.*$/,     //网址验证
            controllerUrl: /^\/[A-Za-z]+\/[A-Za-z]+$/,
            httpUrl:/^http[s]?:\/\/([\w-]+\.)+[\w-]+([\w-./?%&=]*)?$/,
            idcard: function (gets, obj, curform, datatype) {
                var iSum = 0;
                var info = "";
                var sId = gets;
                var aCity = { 11: "北京", 12: "天津", 13: "河北", 14: "山西", 15: "内蒙古", 21: "辽宁", 22: "吉林", 23: "黑龙江", 31: "上海", 32: "江苏", 33: "浙江", 34: "安徽", 35: "福建", 36: "江西", 37: "山东", 41: "河南", 42: "湖北", 43: "湖南", 44: "广东", 45: "广西", 46: "海南", 50: "重庆", 51: "四川", 52: "贵州", 53: "云南", 54: "西藏", 61: "陕西", 62: "甘肃", 63: "青海", 64: "宁夏", 65: "新疆", 71: "台湾", 81: "香港", 82: "澳门", 91: "国外" }
                if (!/^\d{17}(\d|x)$/i.test(sId)) return false;
                sId = sId.replace(/x$/i, "a");
                if (aCity[parseInt(sId.substr(0, 2))] == null) return false;
                sBirthday = sId.substr(6, 4) + "-" + Number(sId.substr(10, 2)) + "-" + Number(sId.substr(12, 2));
                var d = new Date(sBirthday.replace(/-/g, "/"));
                if (sBirthday != (d.getFullYear() + "-" + (d.getMonth() + 1) + "-" + d.getDate())) return false;
                for (var i = 17; i >= 0; i--) iSum += (Math.pow(2, i) % 11) * parseInt(sId.charAt(17 - i), 11);
                if (iSum % 11 != 1) return false;
                return true;
            },
            CustonPhone: function (gets, obj, curform, datatype) {
                var regPhone = /^(^(\d{3,4}-)?\d{7,8})$|(13[0-9]{9}|15[0-9]{9}$|18[0-9]{9})$|17[0-9]{9}$/;//手机座机一起验证
                var linkManTel = gets;

                if (gets.length <= 0) {
                    return "联系人电话只不能为空!";
                }
                if (!regPhone.test(linkManTel)) {
                    return "输入的电话格式不正确";
                }
                return true;
            }
        },
        usePlugin: {
            swfupload: {},
            datepicker: {},
            passwordstrength: {},
            jqtransform: {
                selector: "select,input"
            }
        },
        beforeCheck: function (curform) {
            //在表单提交执行验证之前执行的函数，curform参数是当前表单对象。
            //这里明确return false的话将不会继续执行验证操作;	
        },
        beforeSubmit: function (curform) {
            //在验证成功后，表单提交前执行的函数，curform参数是当前表单对象。
            //这里明确return false的话表单将不会提交;	
        },
        callback: function (data) {
            //返回数据data是json对象，{"info":"demo info","status":"y"}
            //info: 输出提示信息;
            //status: 返回提交数据的状态,是否提交成功。如可以用"y"表示提交成功，"n"表示提交失败，在ajax_post.php文件返回数据里自定字符，主要用              在callback函数里根据该值执行相应的回调操作;
            //你也可以在ajax_post.php文件返回更多信息在这里获取，进行相应操作；
            //ajax遇到服务端错误时也会执行回调，这时的data是{ status:**, statusText:**, readyState:**, responseText:** }；
            //这里执行回调操作;
            //注意：如果不是ajax方式提交表单，传入callback，这时data参数是当前表单对象，回调函数会在表单验证全部通过后执行，然后判断是否提交表               单，如果callback里明确return false，则表单不会提交，如果return true或没有return，则会提交表单。
        }
    },
    //验证对象
    validObj: new Array(),
    //初始化验证插件
    InitVaild: function (formId, setting) {
        $.extend(common.validConfig, setting);
        common.validObj[formId] = $("#" + formId).Validform(common.validConfig);
    },
    //判断表单是否验证通过
    isValid: function (formId) {
        if (common.validObj[formId] == null) { return true; }
        return common.validObj[formId].check(false);
    },
    VaildSubmit: function (formId) {
        if (typeof (common.validObj[formId]) == "undefined") {
            return true;
        }
        return common.validObj[formId].submitForm(false);
    },
    /***********************************JBox弹窗封装部分*****************************************/
    jbox: $.jBox,
    /**
    * jqery 异步无参数请求一个url地址，并将返回值填充到指定容器内
    * @author 陈秘
    * @param {obj} obj 页面标签对象(jquery)
    * @param {string} url ajax请求的路劲
    */
    load: function (obj, url) {
        $.ajax({
            type: "get",
            url: url,
            cache: false,
            data: {},
            beforeSend: function () {
                $.jBox.tip("处理中...", 'loading');
            },
            success: function (data) {
                if (typeof (data) == "undefined") {
                    $.jBox.tip('服务器返回： undefined', 'info');
                }
                obj.empty().html(data);
                $.jBox.closeTip();
            },
            error: function (XMLHttpRequest) { $.jBox.tip('服务器繁忙，请稍后再试...', 'error'); },
            complete: function (XMLHttpRequest) { XMLHttpRequest = null; }
        });
    },
    /**
    * jqery 异步POST请求一个url地址，成功后调用回调函数
    * @author 陈秘
    * @param {string} url ajax请求的路径
    * @param {json/string} parameter json格式或者字符串形式的参数
    * @param {function} callback 回调函数，函数的参数为请求成功后的返回值
    */
    post: function (url, parameter, callback) {
        $.post(url, parameter, function (data) {
            if (typeof (data) == "undefined") {
                $.jBox.tip('服务器返回： undefined', 'info');
            }
            callback(data);
        });
    },
    /**
    * jqery POST请求一个url地址，成功后调用回调函数
    * @author 陈秘
    * @param {string} url ajax请求的路径
    * @param {json/string} parm json格式或者字符串形式的参数
    * @param {function} callback 回调函数，函数的参数为请求成功后的返回值
    * @param {Boolean}  blnAsync 是否发送异步请求，默认：true
    */
    ajax: function (url, parm, callback, blnAsync) {
        $.ajax({
            type: "post",
            url: url,
            cache: false,
            data: parm,
            async: blnAsync || true,
            beforeSend: function () {
                $.jBox.tip("努力获取数据中...", 'loading');
            },
            success: function (data) {
                if (typeof (data) == "undefined") {
                    $.jBox.tip('服务器返回： undefined', 'info');
                }
                callback(data);
                $.jBox.closeTip();
            },
            error: function (XMLHttpRequest) { $.jBox.tip('服务器繁忙，请稍后再试...', 'error'); },
            complete: function (XMLHttpRequest) { XMLHttpRequest = null; }
        });
    },
    /**
    * jqery ajax请求一个url地址，成功后调用回调函数
    * @author 陈秘
    * @param {string} url ajax请求的路径
    * @param {json/string} parm json格式或者字符串形式的参数
    * @param {function} callback 回调函数，函数的参数为请求成功后的返回值
    * @param {Boolean}  blnAsync 是否发送异步请求，默认：true
    * @param {string}  type 请求方式,默认：postajaxResultFunction
    */
    ajaxHandle: function (url, parm, callback, blnAsync, type) {
        $.ajax({
            type: type || "post",
            url: url,
            cache: false,
            data: parm,
            async: blnAsync || true,
            beforeSend: function () {
                $.jBox.tip("处理中...", 'loading');
            },
            success: function (data) {
                if (typeof (data) == "undefined") {
                    $.jBox.tip('服务器返回： undefined', 'info');
                }
                callback(data);
                $.jBox.closeTip();
            },
            error: function (XMLHttpRequest) { alert(XMLHttpRequest.status); $.jBox.tip('服务器繁忙，请稍后再试...', 'error'); },
            complete: function (XMLHttpRequest) {
                XMLHttpRequest = null;
            }
        });
    },
    /**
    * jqery 异步POST请求一个url地址，成功后调用回调函数(带有jbox提示)
    * @author 陈秘
    * @param {string} url ajax请求的路径
    * @param {json/string} parameter json格式或者字符串形式的参数
    * @param {function} callback 回调函数，函数的参数为请求成功后的返回值
    */
    ajaxWithTip: function (url, parm, callback) {
        $.ajax({
            type: "post",
            url: url,
            cache: false,
            data: parm,
            beforeSend: function () {
                $.jBox.tip("正在加载中，请稍后...", 'loading');
            },
            success: function (data) {
                if (typeof (data) == "undefined") {
                    $.jBox.tip('服务器返回： undefined', 'info');
                }
                callback(data);
                $.jBox.closeTip();
            },
            error: function (XMLHttpRequest) { $.jBox.tip('服务器繁忙，请稍后再试...', 'error'); },
            complete: function (XMLHttpRequest) {
                XMLHttpRequest = null;
            }
        });
    },
    ajaxWithNoTip: function (url, parm, callback) {
        $.ajax({
            type: "post",
            url: url,
            cache: false,
            data: parm,
            beforeSend: function () {

            },
            success: function (data) {
                if (typeof (data) == "undefined") {

                }
                callback(data);
            },
            error: function (XMLHttpRequest) { },
            complete: function (XMLHttpRequest) {
                XMLHttpRequest = null;
            }
        });
    },
    /**
    * 关闭所有jBox窗体
    * @author 陈秘
    */
    close: function (token) {
        $.jBox.close(token);
    },
    /******-----begin $.jBox.tip() jBox相关参数详情，请参见API-----********/
    tip: function (content, icon) {
        $.jBox.tip(content, icon);
    },
    tipWithTime: function (content, icon, time) {
        $.jBox.tip(content, { icon: icon, timeout: time });
    },
    tipCloseTime: function (content, icon, time) {
        window.setTimeout(function () { $.jBox.tip(content, icon); }, time);
    },
    closeTip: function () {
        $.jBox.closeTip();
    },
    /******-----end $.jBox.tip() -----********/
    /******-----begin $.jBox.messager() -----********/
    messager: function (content, title) {
        $.jBox.messager(content, title);
    },
    closemessager: function () {
        $.jBox.closeMessager();
    },
    /******-----end $.jBox.messager() -----********/
    alert: function (content, icon, title) {
        icon = icon || 'info';
        title = title || '提示';
        $.jBox.prompt(content, title, icon);
    },
    alertCallBack: function (content, icon, title, callback) {
        icon = icon || 'info';
        title = title || '提示';
        $.jBox.prompt(content, title, icon, { closed: function () { callback(); } });
    },
    alertSuccess: function (content, title, callback) {
        options = {
            submit: function (v, h, f) {
                callback(v);
            }
        };
        $.jBox.success(content, title, options);
    },
    confirm: function (content, yes, no) {
        var submit = function (v, h, f) {
            if (v == 'ok')
                yes();
            else if (v == 'cancel')
                no();
            return true;
        };
        $.jBox.confirm(content, "提示", submit);
    },
    closeDialog: function (id) {
        $.jBox.close("true");
    },
    /**
    * ajax异步加载数据到指定的容器：默认容器的id：mainContent
    * @author 陈秘
    * @param {string} url 请求路径
    * @param {json/string} parm 参数
    */
    getView: function (url, parm) {
        common.ajaxWithTip(url, parm, function (data) { $("#mainContent").empty().html(data); });
    },
    /**
    * 以弹框的形式，加载从ajax异步请求的数据
    * @author 陈秘
    * @param {string} title 弹框title
    * @param {string} url 请求路径
    * @param {json/string} parm 参数
    */
    getDialog: function (title, url, parm) {
        $.ajax({
            type: "post",
            url: url,
            cache: false,
            data: parm,
            success: function (data) {
                common.alert(data, 'info', title);
            }
        });
    },
    /**
    * 以弹框的形式，加载从ajax异步请求的数据
    * @author 陈秘
    * @param {string} title 弹框title
    * @param {string} url 请求路径
    * @param {int} width 参数宽
    * @param {int} height 参数高
    */
    OpenWindows: function (title, url, width,height,buttons) {
        width = width || 900;
        height = height || 395;
        buttons = buttons || { buttons: { '关闭': true } }
        $.jBox.open(url, title, width, height, buttons);
    },
    /**
    * 得到字符串长度 汉字算两个
    * @author 陈秘
    * @param {string} s 需被查看长度的字符串
    * @return {string}  参数s的长度
    */
    getEncodsLength: function (s) { //
        var char_length = 0;
        for (var i = 0; i < s.length; i++) {
            var son_char = s.charAt(i);
            encodeURI(son_char).length > 2 ? char_length += 2 : char_length += 1;
        }
        return char_length;
    },
    /**
    * 截取字符串(不包含html标签)
    * @author 陈秘
    * @param {string} str 需被截取的字符串
    * @param {int} len 截取长度
    * @param {string} suffix 截取后添加的后缀，如：'...'
    * @return {string}  参数str截取后的字符串
    */
    subEncodString: function (str, len, suffix) {  //截取字符
        if (common.getEncodsLength(str) <= len * 2)
            return str;
        var char_length = 0;

        for (var i = 0; i < str.length; i++) {
            var son_str = str.charAt(i);
            encodeURI(son_str).length > 2 ? char_length += 1 : char_length += 0.5;
            if (char_length >= len) {
                var sub_len = char_length == len ? i + 1 : i;
                return str.substr(0, sub_len) + suffix;
            }
        }
    },
    /*表单序列化字符串转JSON串
    * @author 王吉
    * @param {string} strSerialize 序列化字符串
    */
    serializeToJson: function (strSerialize) {
        var proAttr = strSerialize.split("&"); //表单属性属性
        var json = "";
        for (i = 0; i < proAttr.length; i++) {
            var name = proAttr[i].split('=')[0];
            var value = proAttr[i].split('=')[1] || "";
            json = (json == '' ? '' : json + ',') + '"' + name + '":"' + value + '"';
        }
        if (json != "") {
            json = "[{" + json + "}]";
        }
        return eval(json)[0];
    },
    /**
    * 截取字符串(不包含html标签)
    * @author 陈秘
    * @param {string} str 需被截取的字符串
    * @param {int} length 截取长度
    * @param {string} suffix 截取后添加的后缀，如：'...'
    * @return {string}  参数str截取后的字符串
    */
    subString: function (s, length, suffix) {
        if (s.length < length)
            return s;
        else
            return s.substring(0, length) + suffix;
    },
    /**
    * 字符串转json格式
    * @author 陈秘
    * @param {string} obj 字符串
    * @return {json}  obj的json格式字符串
    */
    getJsonString: function (obj) {
        return JSON.stringify(obj);
    },
    /**
    * 格式化换行字符串，转为json格式
    * @author 陈秘
    * @param {string} msg 字符串
    * @return {json}  msg的json格式字符串
    */
    jsonFormat: function (msg) {
        while (msg.indexOf("\r\n") >= 0) {
            msg = msg.replace("\r\n", "\\r\\n");
        }
        var result = eval('(' + msg + ')');
        return result;
    },
    /**
    * 获取指定容器下的checkbox值，val以，分割
    * @author 陈秘
    * @param {string} wrap 指定容器或者form ID
    * @return {string}  checkbox的val集合
    */
    getAllCheckedVal: function (wrap) {
        var result = "";
        $("#" + wrap + " input:checked[class=cbxid]").each(function (index) {
            result += $(this).val() + ",";
        });
        return result;
    },
    /**
    * checkbox全选
    * @author 陈秘
    * @param {string} con 全选checkbox的ID
    * @param {string} wrap 指定容器或者form ID
    */
    checkAll: function (con, wrap) {
        $("#" + wrap + " .cbxid").each(function () {
            $(this).prop("checked", $(con).prop("checked"))
        });
    },
    /**
    * datatime 默认值设定
    * @author 陈秘
    * @param {string} date 字符串格式下的日期
    * @return {string} date值
    */
    dateTrim: function (date) {
        var temp = date.replace('00:00:00', '').replace('0:00:00', '').replace('1900/1/1', '');
        if ($.trim(temp) == "1900-01-01")
            return "";
        else if ($.trim(temp) == "1900-1-1")
            return "";
        else
            return temp;
    },
    /**
    * initFormParm 初始化表单
    * @author 陈秘
    * @param {string} container 容器ID
    * @param {array} data 值数组
    */
    initFormParm: function (container, data) {
        $("#" + container + " select,#" + container + " :text,#" + container + " :password,#" + container + " textarea,#" + container + " :hidden").each(function (i) {
            var name = $(this).attr("name");
            if (name != "undefined" && name != undefined && name != null && name != "") {
                var v = data[name];
                if (v != "undefined" && v != undefined)
                    $(this).val(v.toString());
            }
        });
    },
    /**
    * resetFormParm 清空表单
    * @author 陈秘
    * @param {string} container 容器ID
    */
    resetFormParm: function (container) {
        $("#" + container + " select,#" + container + " :text,#" + container + " :password,#" + container + " textarea,#" + container + " :hidden").each(function (i) {
            name = $(this).attr("name");
            if (name != "undefined" && name != undefined && name != null && name != "") {
                $(this).val("");
            }
        });
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
    CustUploadFile: function(btnUpload,eleId,folder,fileName,type,eq) {
        eq = eq || 0;
        type = type || 'Image';
        //var custFileName = $('#'+fileName).val() || ''; 
         fileName = fileName || '';
        /*上传封面*/
        //上传附件
        new AjaxUpload('#'+btnUpload, {
            action: '/lib/file-upload/FileUploadHandler.ashx',
            data: { Folder: folder,FileName:fileName,Type: type },
            onSubmit: function (file, ext) {
                if (!/^(jpeg|jpg|gif|png|bmp)$/.test(ext)) {
                    alert("只能上传格式为(jpeg|jpg|gif|png|bmp)格式的文件！", "提示");
                    return false;
                } else {
                    $("div[name='updata']").eq(eq).html("<img name='loadimgs' src='/lib/file-upload/loading.gif'/>");
                }
            },
            onComplete: function (file, response) {
                $("div[name='updata']").eq(eq).html('');
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
                    //用于记录，上传的图片
                    var newV = "/" + reArray[1] + "/" + reArray[0];
                    $("#"+eleId).val(newV);
                    $("#img"+eleId).attr("src", reArray[2]);
                }
            }
        });
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

function AjaxStart(str) {
    str = str || "努力加载中";
    common.tip(str, 'loading');
}
function AjaxStop() {
    common.closeTip();
}

//列表页，选中所有
function selectAll(chkd) {
    var chks = document.getElementsByTagName('input');
    for (var i = 0; i < chks.length; i++) {
        if (chks[i].type == 'checkbox') chks[i].checked = chkd;
    }
}
//获取页面中选中的复选框的值
function GetCheckedValue() {
    var srcs = "";
    var arrChk = $("input[name='ckSub']:checked");
    $(arrChk).each(function () {
        srcs += this.value + ",";
    });
    srcs = srcs.substring(0, srcs.lastIndexOf(','));
    return srcs;
}
/**
*检查是否选中
*Content:为选择项，提示内容 
*MulContent:不允许多选，提示内容
*是否允许多选
**/
function CheckSelected(Content, MulContent, IsMultiple) {
    IsMultiple = IsMultiple || false;
    Content = Content || "请选择需要操作的选项！";
    MulContent = MulContent || "不能选择多项进行操作！";
    //检查是否选择了角色
    var checkVal = GetCheckedValue();
    if (checkVal == '' || checkVal == null) {
        common.tip(Content, "info");
        return "";
    }
    if (checkVal.split(',').length > 1 && IsMultiple == false) {
        common.tip(MulContent, "info");
        return "";
    }
    return checkVal;
}


/**
列表选择checkbox批量操作
url：访问路径
IsMultiple:是否选择多项
confirm:是否要求确认
*/
function BatchOptions(url, isMultiple, confirm, tipStr) {
    var ids = CheckSelected(null, null, isMultiple); //获取选择项的ID集合
    if (ids == '') {
        return;
    }
    ConfirmOption(url, ids, confirm, tipStr);
}

/**
扩展异步提交(是否弹出确认框)
url：访问路径
ids:数据ID
confirm:是否要求确认
**/
function ConfirmOption(url, ids, confirm, tipStr) {
    confirm = confirm || false;
    tipStr = tipStr || "你确认要继续变更【修改、删除】数据？";
    if (confirm) { //提示框
        common.confirm(tipStr, function () {
            CommonOption(url, ids);
        }, function () {
            return false;
        });
    } else {
        CommonOption(url, ids);
    }
}
/**
通用异步提交数据变更
url：访问路径
ids:数据ID
**/
function CommonOption(url, ids) {
    common.ajaxHandle(url, { ids: ids }, function (data) {
        if (data == "success") {
            //刷新页面d
            common.tipCloseTime('数据变更成功!', 'success', 1000);

            setTimeout("this.location.reload()", 1500);

        } else {
            common.tipCloseTime('数据变更失败!', 'error', 1000);
        }
    }, false, null);
}

/**
*异步请求之后，处理结果
*/
function ajaxResultFunction(result, successTip, errorTip, tipTime) {
    successTip = successTip || "数据变更成功!";
    errorTip = errorTip || "数据变更失败!";
    tipTime = tipTime || 1000;
    if (result) {
        //刷新页面d
        common.tipCloseTime(successTip, 'success', tipTime);
        setTimeout("this.location.reload()", tipTime+800);

    } else {
        common.tipCloseTime(errorTip, 'error', tipTime);
    }
}