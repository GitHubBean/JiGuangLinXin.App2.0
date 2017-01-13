import '../common/lib';
import React from 'react';
import ReactDOM from 'react-dom';
import {Button,Toast,ActionSheet,Dialog,Mask} from 'react-weui';
// import provinceList from '../common/province';
// import cityList from '../common/city';
import config from '../common/config';
import store from 'store2';
import util from '../common/util';
import MultipleSelect from '../component/MultipleSelect';
// common
// import Head from '../common/head';
// import Accordion from '../pages/AccordionExample.jsx'
const {Alert} = Dialog;
// const cityLength = cityList.length;
let slider;
const Index = React.createClass({
  
  getInitialState() {
    return {
      'body_top': 0,
      
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
      'toast_show': true,
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

  componentWillMount() {
    // store.clear();  
    //判断日期
    store.remove(util.GetDateStr(-1));
    //投票排名
    $.ajax({
      type:"post",
      data:{
        'pn': 1,
        'rows': 100
      },
      url:config.ajaxBaseUrl + 'vote/Rank',
      success:(result)=> {
        if(result.State === 0) {
          const temp = [[],[],[],[],[],[],[],[],[],[]];
          for(var i=0;i<10;i++) {
            for(var j=0;j<10;j++) {
              temp[i][j] = result.Data.list[i*10+j];
              temp[i][j].add = [];
            }
          } 
          this.setState({
            'votes': temp,
            'toast_show': false,
            'banner_show': true
          })
          let me = this;
          slider = Swipe(document.getElementById('slider'), {
            startSlide: 0,  //起始图片切换的索引位置
            auto: 3000000, //设置自动切换时间，单位毫秒
            continuous: false,  //无限循环的图片切换效果
            disableScroll: false,  //阻止由于触摸而滚动屏幕
            stopPropagation: false,  //停止滑动事件
            callback: function(index, element) {
              me.formatPage();
            },  //回调函数，切换时触发
            transitionEnd: function(index, element) {
              
            }  //回调函数，切换结束调用该函数。
          });
        }
      },
      error:()=> {
        config.ajaxError(this);
      }
    })
    //累计浏览量
    $.ajax({
      type:"post",
      url:config.ajaxBaseUrl + 'vote/View',
      success:(result)=> {
        
      },
      error:()=> {
        config.ajaxError(this);
      }
    })
    //投票人数、报名社区
    $.ajax({
      type:"post",
      url:config.ajaxBaseUrl + 'vote/Statistics',
      success:(result)=> {
        if(result.State === 0) {
          this.setState({
            'total_votes': result.Data.voteCount,
            'total_village': result.Data.villageCout,
            'total_pv': result.Data.viewCount
          })
        }
        const options = {
          useEasing : true, 
          useGrouping : true, 
          separator : ',', 
          decimal : '.', 
          prefix : '', 
          suffix : '' 
        };
        const votes = new CountUp("total_votes", 0, result.Data.voteCount, 0, 2.5, options);
        const village = new CountUp("total_village", 0, result.Data.villageCout, 0, 2.5, options);
        const pv = new CountUp("total_pv", 0, result.Data.viewCount, 0, 2.5, options);
        votes.start();
        village.start();
        pv.start();
      },
      error:()=> {
        config.ajaxError(this);
      }
    })
  }, 

  componentDidMount() {
    const deviceWidth = $(window).width();
    const vnameWidth = deviceWidth * 0.25;
    const moduleWidth = this.refs.module.clientWidth;
    const moduleTitleWidth = this.refs.module_title.clientWidth;
    // console.log(moduleTitleWidth)
    // const sszjxqLabelWidth = this.refs.sszjxq_label.clientWidth;
    const cmjcLabel = this.refs.cmjc_label.clientWidth;
    this.setState({
      'wrapWidth': parseInt(deviceWidth*0.94),
      'vnameWidth': parseInt(deviceWidth*0.94) - 56 - 36 - 56 - 60,
      // 'szzjxqInputWidth': sszjxqWidth - sszjxqLabelWidth - 82,
      'addressInputWidth': moduleTitleWidth - cmjcLabel - 2,
      'cmjcInputWidth': moduleWidth - cmjcLabel - 112
    })
    
  },
  
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
      let votes_add_index = temp[i][j].add.length;
      this.setState({
        'votes': temp,
      })
      setTimeout(()=>{
        temp[i][j].add[votes_add_index - 1].show = false;
        this.setState({
          'votes': temp,
        })
      },1000)
    } else {
      let temp = this.state.search_result;
      temp[i].votes++;
      temp[i].color = true;
      temp[i].add.push({
        'show': true
      }) 
      let votes_add_index = temp[i].add.length;
      this.setState({
        'search_result': temp,
      })
      setTimeout(()=>{
        temp[i].add[votes_add_index - 1].show = false;
        this.setState({
          'search_result': temp
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
    if(!this.state.province && !this.state.city && !this.state.area) {
      this.setState({
        'alert_title': '搜索小区失败',
        'alert_label': '请选择地区',
        'alert_show': true
      })
      return false;
    }
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
        'cityName': this.state.city ? this.state.city : this.state.province,
        'disName': this.state.area
      },
      url:config.ajaxBaseUrl + 'vote/Query',
      success:(result)=> {
        console.log(result)
        if(result.State ===0) {
          let temp = [];
          let length = result.Data.length;
          for(var i=0;i<length;i++) {
            temp[i] = result.Data[i];
            temp[i].add = [];
          }
          this.setState({
            'search_result': temp
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
  
  //选择省市
  // handleProvinceChange(e) {
  //   this.setState({
  //     'province': provinceList[e.target.value - 1].name,
  //     'proId': e.target.value
  //   })
  //   let temp = [];
  //   for(var i=0;i<cityLength;i++) {
  //     if(cityList[i].ProID == e.target.value) {
  //       temp.push(cityList[i])
  //     }
  //   }
  //   this.setState({
  //     'cityList': temp,
  //     'city': temp.length == 1 ? temp[0].name : '' 
  //   })
  // },
  // //选择市
  // handleCityChange(e) {
  //   this.setState({
  //     'city': cityList[e.target.value - 2].name
  //   })
  // },
  
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
  
  //重名检测
  // handleIsExist(e) {
  //   if(!this.state.province) {
  //     this.setState({
  //       'alert_title': '重名检测',
  //       'alert_label': '请输入省（市）名',
  //       'alert_show': true
  //     })
  //     return false;
  //   }
  //   if(!this.state.city) {
  //     this.setState({
  //       'alert_title': '重名检测',
  //       'alert_label': '请输入城市名',
  //       'alert_show': true
  //     })
  //     return false;
  //   }
  //   if(!this.refs.existName.value) {
  //     this.setState({
  //       'alert_title': '重名检测',
  //       'alert_label': '请输入小区名',
  //       'alert_show': true
  //     })
  //     return false;
  //   }
  //   $.ajax({
  //     type:"post",
  //     data:{
  //       'buildingName': this.refs.existName.value,
  //       'cityName': this.state.province,
  //       'disName': this.state.city
  //     },
  //     url:config.ajaxBaseUrl + 'vote/IsExist',
  //     success:(result)=> {
  //       if(result.State ===0) {
  //         this.setState({
  //           'toast_icon': 'toast',
  //           'toast_label': '该小区可以创建',
  //           'toast_show': true
  //         })
  //         setTimeout(()=>{
  //           this.setState({
  //             'toast_show': false
  //           })
  //         },2000)
  //       } else {
  //         this.setState({
  //           'alert_title': '提示消息',
  //           'alert_label': result.Msg,
  //           'alert_show': true
  //         })
  //       }    
  //       console.log(result)   
  //     },
  //     error:()=> {
  //       console.log('s')
  //       config.ajaxError(this);
  //     }
  //   })
  // },
  
  //获取焦点
  handleFoucs(e) {
    const windowHeight = $(window).height();
    this.setState({
      'body_top': -windowHeight/2
    })
  },
  handleBlur(e) {
    this.setState({
      'body_top': 0
    })
  },
  
  //开通小区
  handleApplyAdd() {
    if(!this.state.province && !this.state.city && !this.state.area) {
      this.setState({
        'alert_title': '创建自家小区失败',
        'alert_label': '请选择地区',
        'alert_show': true
      })
      return false;
    }
    if(!this.refs.existName.value) {
      this.setState({
        'alert_title': '创建自家小区失败',
        'alert_label': '请输入小区名',
        'alert_show': true
      })
      return false;
    }
    $.ajax({
      type:"post",
      data:{
        'buildingName': this.refs.existName.value,
        'cityName': this.state.city ? this.state.city : this.state.province,
        'disName': this.state.area
      },
      url:config.ajaxBaseUrl + 'vote/ApplyAdd',
      success:(result)=> {
        if(result.State ===0) {
          this.setState({
            'alert_title': '创建成功',
            'alert_show': true,
            'alert_label': '您的小区已创建成功，请等待审核'
          })
          // setTimeout(()=>{
          //   this.setState({
          //     'toast_show': false,
          //   })
          // },2000);
        } else {
          this.setState({
            'alert_title': '创建失败',
            'alert_show': true,
            'alert_label': result.Msg
          })
        }    
      },
      error:()=> {
        console.log('s')
        config.ajaxError(this);
      }
    })
  },
  
  //关闭订阅
  handleSubscribeClose() {
    this.setState({
      'subscribe': 'false'
    })
  },
  //关闭注册
  handleRegisterClose() {
    this.setState({
      'register': 'false'
    })
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
    const bodyStyle = {
      'top': this.state.body_top + 'px'
    }
    return(
      <div className='body-wrap' style={bodyStyle}>
      
        <div className='module-1'>
          <div className='banner'>
            <img className={this.state.banner_show ? 'animated bounceIn' : ''} src="img/banner.png" alt="全国和谐小区人气大赛" width='100%'/>
             {/*<a className='hdgz' href="#hdgz">查看活动规则？</a> */}
          </div>
          <div className='activity-data'>
            <div className='tprs'>
              <h1 className='data-title'>
                <i className='data-icon'></i><span className='data-label'>投票人数</span>
              </h1>
              <span className='number' id='total_votes'>{this.state.total_votes}</span>
            </div>
            <div className='bmsq'>
              <h1 className='data-title'>
                <i className='data-icon'></i><span className='data-label'>报名社区</span>
              </h1>
              <span className='number' id='total_village'>{this.state.total_village}</span>
            </div>
            <div className='fwl'>
              <h1 className='data-title'><i className='data-icon'></i><span className='data-label'>访问量</span>  
              </h1>
              <span className='number' id='total_pv'>{this.state.total_pv}</span>
            </div>
          </div>
        </div>
        
        <div className='module-2' style={moduleStyle}>
          <div className='swipe' id='slider'>
            <div className="swipe-wrap">
              {this.state.votes.map((voteTable,i)=>{
                return(
                  <table className='voteData'>
                    <thead>
                      <tr>
                        <th className='vote'>投票</th>
                        <th className='rank'>排名</th>
                        <th className='votes'>票数</th>
                        <th className='vname'>小区名</th>
                        <th className='city'>所在地</th>
                      </tr>
                    </thead>
                    <tbody>
                      {voteTable.map((vote,j)=>{
                        return(
                          <tr>
                            <td className='vote'><Button onClick={this.handleVoteSend.bind(this,vote.bid,i,j,1)}>投票</Button></td>
                            <td className='rank'>{i*10+j+1}</td>
                            <td className='votes' key={'votes'+i+j}>
                              <span className={vote.color ? 'color-green' : ''}>{vote.votes}</span>
                             {/*} {this.state.votes_add.map((vote,i)=>{
                                return (
                                  <div className='votes_add animated zoomIn' style={{'display': vote.show ? 'bloack' : 'none'}}></div>
                                )
                              })} */}
                              {vote.add.map((add,i)=>{
                                return (
                                  <i className='votes_add animated zoomIn' style={{'display': add.show ? 'bloack' : 'none'}}></i>
                                )
                              })}
                            </td>
                            <td className='vname'><div className='vname' style={vnameStyle}>{vote.title}</div></td>
                            <td className='city'><div className='city'>{vote.cityName}</div></td>
                          </tr>
                        )
                      })}
                    </tbody>
                  </table>
                )      
              })} 
            </div>
          </div>  
        </div>
        
        <div className='pagination'>
          <div className='prev' onClick={this.handlePrev}></div>
          <div className='next' onClick={this.handleNext}></div>
          <div>第{this.state.votes_page}页<span className='total'>(共10页)</span></div>
        </div>
        
        <div className='module-3' id='hdgz'>
          <div className='rule-1'>
            <div className='module-title'></div>
            <ol>
              <li>1、即日起关注“邻信”官方微信即可参与“和谐社区”投票，为您的小区投票。</li>
              <li>2、投票时社区已存在可以直接投票，若社区信息未录入，则可点击“创建自家小区”发起自家小区投票。</li>
              <li>3、每人每天可投10票，关注“邻信”官方公众号可查询和领取活动奖励。</li>
              <li>4、活动时间：2016年3月15日-4月15日。</li>
            </ol>
          </div>
          <div className='rule-2'>
            <div className='module-title'></div>
            <ol>
              <li>1、截止投票结束，人气排名前三的小区，所在其小区的每位用户将获得用户红包奖励：
                <table>
                  <tr>
                    <td>第1名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>50元</span>用户红包</td>
                  </tr>
                  <tr>
                    <td>第2名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>40元</span>用户红包</td>
                  </tr>
                  <tr>
                    <td>第3名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>30元</span>用户红包</td>
                  </tr>
                </table>
              </li>
              <li>2、人气排名第4到第100的小区，所在其小区的每位用户将获得<span className='color-red'>5~10元</span>不等的奖励：
                <table>
                  <tr>
                    <td>第4-10名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>10元</span>用户红包</td>
                  </tr>
                  <tr>
                    <td>第10-30名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>8元</span>用户红包</td>
                  </tr>
                  <tr>
                    <td>第30-60名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>6元</span>用户红包</td>
                  </tr>
                  <tr>
                    <td>第40-100名</td>
                    <td><span className='color-red'>每位</span>用户获得<span className='color-red'>5元</span>用户红包</td>
                  </tr>
                </table>
              </li>
              <li>
                3、奖励发布详情请关注“邻信”官方微信。
                <img className='code' src="img/code.png" alt="二维码" width='100%' />
              </li>
            </ol>
          </div>
          <div className='rule-footer'></div>
        </div>
        
        <div className='module-4' ref='module'>
          <div className='module-title'></div>
          {/*
            <div className='sszjxq' ref='sszjxq'>
              <div className='label' ref='sszjxq_label' >搜索自家小区</div>
              <div className='input-box' style={sszjxqStlye}><input className='activity_input' type="text" ref='buildingName'/></div>
              <Button onClick={this.handleSearchBuild}>搜索</Button>
              <div className='clearfix'></div>
            </div>
          */}
          <div style={{'position': 'relative'}} >
            {/* <table className='voteData' style={{'display':this.state.search_result.length >0 ? 'table' : 'none'}}> */} 
            <table className='voteData'>
              <thead>
                <tr>
                  <th className='vote'>投票</th>
                  <th className='rank'>排名</th>
                  <th className='votes'>票数</th>
                  <th className='vName'>小区名</th>
                  <th className='city'>所在地</th>
                </tr>
              </thead>
              <tbody>
                {this.state.search_result.map((vote,i)=>{
                  return (
                    <tr ket={'search'+i}>
                      <td className='vote'><Button onClick={this.handleVoteSend.bind(this,vote.bid,i,0,2)}>投票</Button></td>
                      <td className='rank'>{vote.rank}</td>
                       <td className='votes' key={'votes'+i}>
                        <span className={vote.color ? 'color-green' : ''}>{vote.votes}</span>
                        {vote.add.map((add,i)=>{
                          return (
                            <i className='votes_add animated zoomIn' style={{'display': add.show ? 'bloack' : 'none'}}></i>
                          )
                        })}
                      </td>
                      <td className='vName'><div className='vname' style={vnameStyle}>{vote.title}</div></td>
                      <td className='city'><div className='city'>{vote.cityName}</div></td>
                    </tr>
                  )
                })}     
              </tbody>
            </table>
            <div className='nodata' style={{'display':this.state.search_result.length <=0 ? 'block' : 'none'}}>{this.state.search_nodata}</div>
            {/*  */} 
            {this.state.search_add.map((vote,i)=>{
              return (
                <div className='votes_add animated zoomIn' style={{'display': vote.show ? 'bloack' : 'none'}}></div>
              )
            })}
          </div>
          
          <div className='cjzjxq'>
            <div className='title' ref='module_title'>搜索/创建自家小区</div>
            <div>
              <div className='label' ref='address_label'>地区：</div>
              {/* 
                <div className='sheng'>
                  <div>
                    <input className='activity_input' type="text" readOnly='true' value={this.state.province}/>
                    <select onChange={this.handleProvinceChange}>
                      {provinceList.map((province,i)=>{
                        return(
                          <option value={province.ProID} key={i}>{province.name}</option>
                        )
                      })}
                    </select>
                    <span className='txt'>省/市</span>
                  </div>
                </div>
                <div className='shi'>
                  <div>
                    <input className='activity_input' type="text" readOnly='true' value={this.state.city}/>
                    <select onChange={this.handleCityChange} style={{'display':this.state.cityList.length > 0 ? 'block': 'none'}}>
                      {this.state.cityList.map((city,i)=>{
                        return(
                          <option value={city.CityID} key={i}>{city.name}</option>
                        )
                      })}
                    </select>
                    <select style={{'display':this.state.cityList.length <= 0 ? 'block': 'none'}}>
                      <option value="">请先选择省（直辖市）</option>
                    </select>
                    <span className='txt'>市</span>
                  </div>
                </div>
                <div className='clearfix'></div>
              */}
               <div className='activity_input address_input'  ref='address_input' style={addressStyle}
                    onClick={this.handleSelectCityOpen} >
                 {this.state.province + this.state.city + this.state.area}
               </div>
              <div className='clearfix'></div>
            </div>
          </div>
          
          <div className='cmjc'>
            <div className='label' ref='cmjc_label'>小区：</div>
            <div className='inputBox' ref='cmjc_input' style={cmjcStyle}>
              <input className='activity_input' 
                type="text" ref='existName' onFocus={this.handleFoucs.bind(this)} onBlur={this.handleBlur.bind(this)}/>
              </div>
            <Button onClick={this.handleSearchBuild}>搜索小区</Button> 
            <div className='clearfix'></div>
          </div>
          <Button className='cjxqBtn' onClick={this.handleApplyAdd}>创建自家小区</Button>
        </div>
        
        <div className='home-footer'></div>
        <Alert title={this.state.alert_title} show={this.state.alert_show} buttons={this.state.alert_buttons}>{this.state.alert_label}</Alert>
        <Toast icon={this.state.toast_icon} show={this.state.toast_show}>{this.state.toast_label}</Toast>
        <Mask style={{'display': this.state.subscribe === 'false' ? 'none' : 'block' }}>
          <div className='subscribe'>
            <div className='modal-close' onClick={this.handleSubscribeClose}></div>
          </div>
        </Mask>
        <Mask style={{'display': this.state.register === 'false' ? 'none' : 'block' }}>
          <div className='register'>
            <div className='modal-close' onClick={this.handleRegisterClose}></div>
          </div>
        </Mask>
        <MultipleSelect show={this.state.address_show} closeCallBack={this.handleSelectCityClose} completeCallBack={this.handleSelectCity}></MultipleSelect>
      </div>
    )
  }
});
 
ReactDOM.render(<Index />,document.getElementById('jglx-index-wrap'))