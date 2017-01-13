var iMaxUpdateFileSize = 1024 * 1024 * 10;
var index = 1;
function StartUploadify(objectid,yearmonth) {
    $("#" + objectid + '_Temp').uploadify({
        'uploader': '/Scripts/JqueryUploadify/uploadify.swf',
        'cancelImg': '/Scripts/JqueryUploadify/images/cancel.png',
        'buttonText': 'Select',
        'buttonImg': '/Scripts/JqueryUploadify/images/uploadpic.png',
        'width': 135,
        'height': 113,
        'folder':'Uploads/Gallery/'+yearmonth+'/',
        'script': '/RemoteHandlers/UploadImage.ashx',
        'fileExt': '*.jpg;*.jpeg;*.gif;*.png;*.doc;*.docx;*.xls;*.xlsx;*.pdf;*.txt',
        'fileDesc': '只允许上传jpg,png,bmp,gif格式的图片', //允许上传的文件类型的描述，在弹出的文件选择框里会显示
        'sizeLimit': iMaxUpdateFileSize,
        'queueID': 'gallery_queue',
        'queueSizeLimit': 5, 
        'simUploadLimit': 5,
        'multi': true,
        'auto': true,
        'onSelect': function (a, b, c) { /*选择文件上传时可以禁用某些按钮*/ },
       'onComplete': function (a, b, c, d, e) { SetLi_UpdateFiles(objectid, c,d); },
        'onAllComplete': function (a, b) {
            enabledSaveButton(true);
        },
        'onCancel': function (a, b, c, d, e) { },
        'onError': function (a, b, c, d, e) {
            if (c.size > iMaxUpdateFileSize) {
                setTimeout("$('#'+objectid+'_Temp').uploadifyCancel('" + b + "')", 2000);
            }
        }
    });
}

function SetLi_UpdateFiles(objectid, objFile, d) {
    var src = $("input[name='" + objectid + "']").val();
    $("input[name='" + objectid + "']").val(src+d+";");
    $("img[id='" + objectid + "']").attr("src", d);
    var img = "<span class='upload_image_wrapper'><a href='javascript:void(0);' title='删除'>删除</a><img name='tmpimg" + index + "' src='" + d + "' /></span>";
    $("#garage").append(img);
    index++;
}

