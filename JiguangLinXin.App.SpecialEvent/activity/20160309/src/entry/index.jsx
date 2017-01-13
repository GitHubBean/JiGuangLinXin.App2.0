import '../common/lib';
import React from 'react';
import ReactDOM from 'react-dom';
import {Button,Toast,ActionSheet,Dialog,Mask} from 'react-weui';
import provinceList from '../common/province';
import cityList from '../common/city';
import config from '../common/config';
import store from 'store2';
import util from '../common/util';
// import MultipleSelect from '../component/MultipleSelect';
// common
// import Head from '../common/head';
// import Accordion from '../pages/AccordionExample.jsx'
const {Alert} = Dialog;
const cityLength = cityList.length;
let slider;
const Index = React.createClass({
 
  getInitialState() {
    return {
      'vnameWidth': 96,
      'szzjxqInputWidth': 124,
      'cmjcInputWidth': 150,
      'addressInputWidth': 170,
      'wrapWidth': '94%',
      
      'alert_title': '提示信息',
      'alert_label': '',
      'alert_show': false,
      'alert_buttons': [{
        'label': '确认',
        'onClick': this.hideAlert.bind(this)
      }],
      
      'total_votes': '加载中',
      'total_village': '加载中',
      'total_pv': '加载中',
      
      'toast_label': '加载中...',
      'toast_show': false,
      'toast_icon': 'loading',
      
      'votes': [],
      'votes_page': '一',
      'votes_add': [],
      
      'search_result':[],
      'search_action': false,
      'search_nodata': '暂无数据',
      'search_add': [],
      
      'province': '',
      'proId': '',
      'cityList': [],
      'city': '',
      'area':'',
      'address_show': false,
      
      'subscribe': 'false',
      'register': 'false',
      
      'banner_show': false
    }
  },

  // componentWillMount() {
  //   // store.clear();  
  //   //判断日期
  //   store.remove(util.GetDateStr(-1));
  //   //投票排名
  //   $.ajax({
  //     type:"post",
  //     data:{
  //       'pn': 1,
  //       'rows': 100
  //     },
  //     url:config.ajaxBaseUrl + 'vote/Rank',
  //     success:(result)=> {
  //       if(result.State === 0) {
  //         const temp = [[],[],[],[],[],[],[],[],[],[]];
  //         for(var i=0;i<10;i++) {
  //           for(var j=0;j<10;j++) {
  //             temp[i][j] = result.Data.list[i*10+j];
  //             temp[i][j].add = [];
  //           }
  //         } 
  //         this.setState({
  //           'votes': temp,
  //           'toast_show': false,
  //           'banner_show': true
  //         })
  //         let me = this;
  //         slider = Swipe(document.getElementById('slider'), {
  //           startSlide: 0,  //起始图片切换的索引位置
  //           auto: 3000000, //设置自动切换时间，单位毫秒
  //           continuous: false,  //无限循环的图片切换效果
  //           disableScroll: false,  //阻止由于触摸而滚动屏幕
  //           stopPropagation: false,  //停止滑动事件
  //           callback: function(index, element) {
  //             me.formatPage();
  //           },  //回调函数，切换时触发
  //           transitionEnd: function(index, element) {
              
  //           }  //回调函数，切换结束调用该函数。
  //         });
  //       }
  //     },
  //     error:()=> {
  //       config.ajaxError(this);
  //     }
  //   })
  //   //累计浏览量
  //   $.ajax({
  //     type:"post",
  //     url:config.ajaxBaseUrl + 'vote/View',
  //     success:(result)=> {
        
  //     },
  //     error:()=> {
  //       config.ajaxError(this);
  //     }
  //   })
  //   //投票人数、报名社区
  //   $.ajax({
  //     type:"post",
  //     url:config.ajaxBaseUrl + 'vote/Statistics',
  //     success:(result)=> {
  //       if(result.State === 0) {
  //         this.setState({
  //           'total_votes': result.Data.voteCount,
  //           'total_village': result.Data.villageCout,
  //           'total_pv': result.Data.viewCount
  //         })
  //       }
  //       const options = {
  //         useEasing : true, 
  //         useGrouping : true, 
  //         separator : ',', 
  //         decimal : '.', 
  //         prefix : '', 
  //         suffix : '' 
  //       };
  //       const votes = new CountUp("total_votes", 0, result.Data.voteCount, 0, 2.5, options);
  //       const village = new CountUp("total_village", 0, result.Data.villageCout, 0, 2.5, options);
  //       const pv = new CountUp("total_pv", 0, result.Data.viewCount, 0, 2.5, options);
  //       votes.start();
  //       village.start();
  //       pv.start();
  //     },
  //     error:()=> {
  //       config.ajaxError(this);
  //     }
  //   })
  // }, 

  // componentDidMount() {
  //   const deviceWidth = $(window).width();
  //   const vnameWidth = deviceWidth * 0.25;
  //   const moduleWidth = this.refs.module.clientWidth;
  //   const moduleTitleWidth = this.refs.module_title.clientWidth;
  //   console.log(moduleTitleWidth)
  //   // const sszjxqLabelWidth = this.refs.sszjxq_label.clientWidth;
  //   const cmjcLabel = this.refs.cmjc_label.clientWidth;
  //   this.setState({
  //     'wrapWidth': parseInt(deviceWidth*0.94),
  //     'vnameWidth': parseInt(deviceWidth*0.94) - 56 - 36 - 56 - 60,
  //     // 'szzjxqInputWidth': sszjxqWidth - sszjxqLabelWidth - 82,
  //     'addressInputWidth': moduleTitleWidth - cmjcLabel - 2,
  //     'cmjcInputWidth': moduleWidth - cmjcLabel - 110
  //   })
    
  // },
  
  //Alert
  hideAlert() {
    this.setState({'alert_show': false});
  },
  
  //上下页
  handlePrev() {
    slider.prev();
    this.formatPage();
  },
  handleNext(){
    slider.next();
    this.formatPage();
  },
  formatPage() {
    let page;
    switch(slider.getPos()) {
      case 0: page = '一';break;
      case 1: page = '二';break;
      case 2: page = '三';break;
      case 3: page = '四';break;
      case 4: page = '五';break;
      case 5: page = '六';break;
      case 6: page = '七';break;
      case 7: page = '八';break;
      case 8: page = '九';break;
      case 9: page = '十';break;
    }
    this.setState({
      'votes_page': page
    })
  },
  
  //投票
  handleVoteSend(bid,i,j,type,e) {
    //弹出订阅
    if(store('subscribe') == 'false' || !store('subscribe')) {
      this.setState({
        'subscribe': 'true'
      })
      store('subscribe','true');
    }
    //今日投票数
    let count = store(util.GetDateStr(0)) ? store(util.GetDateStr(0)) : 0;
    console.log(util.GetDateStr(0))
    if(count == 8) {
      //弹出注册
      if(store('register') == 'false' || !store('register')) {
        this.setState({
          'register': 'true'
        })
        store('register','true');
      }
    }
    if(count >= 10) {
      this.setState({
        'alert_title': '投票失败',
        'alert_label': '您今天的投票次数已使用完',
        'alert_show': true
      })
      return false;
    }
    store(util.GetDateStr(0),++count);
    
    //判断是排名还是搜索
    if(type ==1) {
      let temp = this.state.votes;
      temp[i][j].votes++;
      temp[i][j].color = true;
      temp[i][j].add.push({
        'show': true
      }) 
      //投票动画效果
      // let votes_add = this.state.votes_add;
      // votes_add.push({
      //   'show': true
      // })
      // let votes_add_index = votes_add.length;
      let votes_add_index = temp[i][j].add.length;
      this.setState({
        'votes': temp,
        // 'votes_add': votes_add
      })
      setTimeout(()=>{
        temp[i][j].add[votes_add_index - 1].show = false;
        this.setState({
          // 'votes_add': votes_add
          'votes': temp,
        })
      },1000)
    } else {
      let temp = this.state.search_result;
      temp[i].votes++;
      temp[i].color = true;
      //投票动画效果
      let votes_add = this.state.votes_add;
      votes_add.push({
        'show': true
      })
      let votes_add_index = votes_add.length;
      this.setState({
        'search_result': temp,
        'search_add': votes_add
      })
      setTimeout(()=>{
        votes_add[votes_add_index - 1].show = false;
        this.setState({
          'search_add': votes_add
        })
      },1000)
    }
    
    //发送投票
    $.ajax({
      type:"post",
      data:{
        'buildingId': bid,
      },
      url:config.ajaxBaseUrl + 'vote/Send',
      success:(result)=> {
          // console.log(result)
        this.setState({
          'total_votes': this.state.total_votes + 1
        })
      },
      error:()=> {
        config.ajaxError(this);
      }
    })
  },
  
  //搜索小区
  handleSearchBuild() {
    // if(!this.state.province && !this.state.city && !this.state.area) {
    //   this.setState({
    //     'alert_title': '搜索小区失败',
    //     'alert_label': '请选择地区',
    //     'alert_show': true
    //   })
    //   return false;
    // }
    if(!this.refs.existName.value) {
      this.setState({
        'alert_title': '搜索小区失败',
        'alert_label': '请输入小区名',
        'alert_show': true
      })
      return false;
    }
    $.ajax({
      type:"post",
      data:{
        'buildingName': this.refs.existName.value,
      },
      url:config.ajaxBaseUrl + 'vote/Query',
      success:(result)=> {
        if(result.State ===0) {
          this.setState({
            'search_result': result.Data
          })
        } else {
           this.setState({
             'search_nodata': '小区不存在，请确认小区名或新建'
           })
        } 
        console.log(this.state.search_result)     
      },
      error:()=> {
        config.ajaxError(this);
      }
    })
  },

  handleSelectCityOpen() {
    this.setState({
      'address_show': true
    })
  },
  handleSelectCityClose() {
    this.setState({
      'address_show': false
    })
  },
  handleSelectCity(address) {
    this.setState({
      'province': address[0],
      'city': address[1] == address[0] ? '' : address[1],
      'area': address[2]
    })
    // console.log(address)
  },
  
  render() { 
    const moduleStyle = {
      'width': this.state.wrapWidth
    }
    const vnameStyle = {
      'width': this.state.vnameWidth
    }
    // const sszjxqStlye = {
    //   'width': this.state.szzjxqInputWidth
    // }
    const addressStyle = {
      'width': this.state.addressInputWidth
    }
    const cmjcStyle = {
      'width': this.state.cmjcInputWidth
    }
    
    return(
      <div>
      
        <div className='module-1'>
          <div className='banner'>
            <img className={'animated bounceIn'} src="img/banner.png" alt="全国和谐小区人气大赛" width='100%'/>
          </div>
        </div>
        
        <div className='module-2'>
          <div className='module-title'></div>
          <div className='module-img'></div>
          <div className='qa'>
            <h2 className='question'>Q1、我能为我的社区做什么？</h2>
            <div className='answer'>
              <p>您可以通过投票，让您的小区成为全国知名社区。</p>
              <p>投票方式：<br />进入“全国和谐小区人气大赛”活动界面“参与投票”板块，输入您所在的社区名，进入投票页面，每人每天最多可投10票。</p>
            </div>
          </div>
          
          <div className='qa'>
            <h2 className='question'>Q2、活动奖励是什么？</h2>
            <div className='answer'>
              <p>截止互动结束，按投票数评选出全国百佳最具人气小区，以上小区的每位用户将获得最高50元的现金券奖励！</p>
              <p>本次活动奖励及奖励发放时间请关注邻信官方微信。</p>
            </div>
          </div>
          
        </div>
         
        <div className='module-3'>
          <div className='module-title'></div>
          <div className='module-img'></div>
          <div className='qa'>
            <h2 className='question'>Q1、如何成为小区群主？</h2>
            <div className='answer'>
              <p>您可以通过投票，让您的小区成为全国知名社区。</p>
              <p>本次活动的小区群主首选对象为社区QQ/微信群的群主或者管理员，也可自荐成为自家小区的群主。</p>
              <ol type='a'>
                <li>收到邻信官方“邀请函”成为自家小区群主；</li>
                <li>在活动页下方“参与投票”板块创建自家小区，创建成功后成为自家小区群主。</li>
              </ol>
            </div>
          </div>
          
          <div className='qa'>
            <h2 className='question'>Q2、我能带领我的邻居做什么？</h2>
            <div className='answer'>
              <p>当您光荣的成为小区群主后，就意味着您的小区已经成功进入全国社区人气排名系统，社区人气排名将由全国真实人气投票产生。</p>
              <p>身为群主，你有责任带领您的社区伙伴们为自家小区的人气而战。同时，这场人气大赛更是各社区凝聚力的比拼。加油吧,各位群主！带领你的小伙伴们，为社区荣誉而战，让全国的人民都来仰慕你们小区的超高人气！</p>
            </div>
          </div>
          
          <div className='qa'>
            <h2 className='question'>Q3、群主福利 ？</h2>
            <div className='answer'>
              <p>由于群主奖励太过诱惑！请通过页面底部二维码关注官方微信或电话咨询。福利绝对超乎你想象！</p>
            </div>
          </div>
          
        </div>
        
        <div className='module-4' ref='module'>
          <div className='module-img'></div>
          <img className='code' src="img/code.png" alt="二维码" width='100%' />
        </div>
        
        <div className='home-footer'></div>
        <Alert title={this.state.alert_title} show={this.state.alert_show} buttons={this.state.alert_buttons}>{this.state.alert_label}</Alert>
        <Toast icon={this.state.toast_icon} show={this.state.toast_show}>{this.state.toast_label}</Toast>
      </div>
    )
  }
});
 
ReactDOM.render(<Index />,document.getElementById('jglx-index-wrap'))