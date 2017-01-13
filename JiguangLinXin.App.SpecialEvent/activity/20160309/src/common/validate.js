let validate = {
  //名字，1：不小于几位，2：不能有空格，3：不能纯数字，4：不能有特殊字符
  nick:function(str,length) {
    let [space,number,teshu]= [/\s/,/^\d+$/,/((?=[\x21-\x7e]+)[^A-Za-z0-9])/];
  	if(str.length<length) {
  		return 1;
  	} else if(space.test(str)) {
  		return 2;
  	} else if(number.test(str)) {
  		return 3;
  	} else if(teshu.test(str)) {
  		return 4;
  	} else {
  		return true;
  	}
  },
  //手机号验证
  tel:function(tel) {
    if (!tel.match(/^((1[3,5,8][0-9])|(14[5,7])|(17[0,6,7,8]))\d{8}$/)) {
      return false;
    } else {
      return true;
    }
  }
}
export default validate;