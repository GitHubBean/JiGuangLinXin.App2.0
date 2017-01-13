var iMaxUpdateFileSize = 1024 * 1024 * 10;
var index = 1;
function StartUploadify(objectid, yearmonth) {
    console.log(0);
    $("#" + objectid + '_Temp').uploadify({
        'uploader': '/assets/js/jquery-uploadify/uploadify.swf',
        'cancelImg': '/assets/js/jquery-uploadify/images/cancel.png',
        'buttonText': 'Select',
        'buttonImg': '/assets/js/jquery-uploadify/images/uploadpic.png',
        'width': 135,
        'height': 113,
        'folder': 'gallery/' + yearmonth + '/',
        'script': '/assets/js/jquery-uploadify/UploadImage.ashx',
        'fileExt': '*.jpg;*.jpeg;*.gif;*.png;*bmp',
        'fileDesc': '只允许上传jpg,png,bmp,gif格式的图片', //允许上传的文件类型的描述，在弹出的文件选择框里会显示
        'sizeLimit': iMaxUpdateFileSize,
        'queueID': 'gallery_queue',
        'queueSizeLimit': 5,
        'simUploadLimit': 5,
        'multi': true,
        'auto': true,
        'onSelect': function (a, b, c) { /*选择文件上传时可以禁用某些按钮*/ },
        'onComplete': function (a, b, c, d, e) { SetLi_UpdateFiles(objectid, c, d); },
        'onAllComplete': function (a, b) {
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
    $("input[name='" + objectid + "']").val(src + d + ";");
    $("img[id='" + objectid + "']").attr("src", d);
    var img = "<span class='upload_image_wrapper'><a href='javascript:void(0);' title='从图片栏中删除该图片' class='del'  onclick='delThisImage(this);'></a><a href='javascript:void(0);' title='将该图插入到描述' class='ins' onclick='insThisImage(this);'>插入</a><img name='tmpimg" + index + "' src='" + d + "' /></span>";
    $("#garage").append(img);
    index++;
}


//删除图片栏中的图片
function delThisImage(obj) {
    var parent = $(obj).parent();
    var val = parent.find("img").attr("src");
    $("#GalleryItems").val($("#GalleryItems").val().replace(val + ";", ""));
    parent.remove();
}
function insThisImage(obj) {
    var parent = $(obj).parent();
    var val = parent.find("img").attr("src").replace("Published", "Thumbnails");
    var data = $('#P_Content').val();
    $('#P_Content').val(data + "<img src='" + val + "' />");
}