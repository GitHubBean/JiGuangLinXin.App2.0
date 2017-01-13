const util = {
  param(key) {
    let url = location.search.replace(/^\?/,'').split('&');
    let paramsObj = {};
    for(var i = 0, iLen = url.length; i < iLen; i++){
        let param = url[i].split('=');
        paramsObj[param[0]] = param[1];
    }
    if(key){
        return decodeURI(paramsObj[key]) || '';
    }
    return paramsObj;
  },
  //字符串日期
  GetDateStr(AddDayCount) {
    const dd = new Date();
    dd.setDate(dd.getDate()+AddDayCount);//获取AddDayCount天后的日期
    const y = dd.getFullYear();
    const m = dd.getMonth()+1;//获取当前月份的日期
    const d = dd.getDate();
    return y+"-"+m+"-"+d;
  },
  
}

export default util;