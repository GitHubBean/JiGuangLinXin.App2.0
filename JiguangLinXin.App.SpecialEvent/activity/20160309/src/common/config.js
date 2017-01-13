const Config = {
  //活动
  // ajaxBaseUrl: 'http://192.168.3.195:8200/sp/',
  ajaxBaseUrl: '/sp/',
  //微网
  // ajaxBaseUrl: 'http://192.168.3.195:8123/lx/',
  ajaxError(me) {
    me.setState({
      'toast_label': '网络异常',
      'toast_show': true,
      'toast_icon': 'warn',
    })
    setTimeout(()=>{
      me.setState({'toast_show':false});
    },2000)
  },
}

export default Config;