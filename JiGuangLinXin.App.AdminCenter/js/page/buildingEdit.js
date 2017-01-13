$("#frm").html5Validate(function () {
    this.submit();
}, {
});
/***layDate**/
laydate.skin('molv');
var start = {
    elem: '#B_BTime',
    format: 'YYYY-MM-DD hh:mm:ss',
    min: laydate.now(), //设定最小日期为当前日期
    max: '2099-06-16 23:59:59', //最大日期
    istime: true,
    istoday: false,
    choose: function (datas) {
        end.min = datas; //开始日选好后，重置结束日的最小日期
        end.start = datas //将结束日的初始值设定为开始日
    }
};
var end = {
    elem: '#B_ETime',
    format: 'YYYY-MM-DD hh:mm:ss',
    min: laydate.now(),
    max: '2099-06-16 23:59:59',
    istime: true,
    istoday: false,
    choose: function (datas) {
        start.max = datas; //结束日选好后，重置开始日的最大日期
    }
};
laydate(start);
laydate(end);

$(function () {
    $('#btnSubmit').click(function () {
        //$('#edit-settings').show();
        // console.log(JSON.stringify($('#tags').data('tag')['values']));
        //if ($('#edit-settings').hasClass("active")) {
        //    $('#edit-settings').removeClass('active');
        //} else {
        //    $('#edit-settings').addClass('active');
        //}
    });

    //自定义标签tags
    var tag_input = $('#B_Tags');
    try {
        tag_input.tag(
          {
              placeholder: tag_input.attr('placeholder'),
          }
        );
    }
    catch (e) {
        //display a textarea for old IE, because it doesn't support this plugin or another one I tried!
        tag_input.after('<textarea id="' + tag_input.attr('id') + '" name="' + tag_input.attr('name') + '" rows="3">' + tag_input.val() + '</textarea>').remove();
        //$('#form-field-tags').autosize({append: "\n"});
    }
    //自定义标签tags end


    /*上传楼盘封面*/
    common.ImagesUploadFileFun1('imgB_CovereImg', 'B_CovereImg', 'Building');

    /*上传视频封面*/
    common.ImagesUploadFileFun1('imgB_VideoImg', 'B_VideoImg', 'Video');

    /*上传视频文件*/
    //common.ImagesUploadFileFun1('imgB_CovereImg', 'B_CovereImg', 'Building');


    //富文本编辑器
    $('#B_Content').xheditor({
        tools: 'full',
        width: '98%',
        height: '320px',
        upImgUrl: '/Editor/UploadFile',
        upImgExt: 'jpg,jpeg,gif,png'
    });
});