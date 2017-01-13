'use strict';

import React from 'react';
import {Mask} from 'react-weui';
// var ReactDOM = require('react-dom');
import classNames from 'classnames';
// import city from '../common/allcity';

let city = [];
let defaultTabWidth = 65;
const MultipleSelect = React.createClass({

  // propTypes: {
  //   theme: React.PropTypes.oneOf(['default', 'basic', 'gapped']),
  //   data: React.PropTypes.array,
  //   activeKey: React.PropTypes.any,
  //   defaultActiveKey: React.PropTypes.any
  // },

  getDefaultProps() {
    return {
      'show': false,
      'closeCallBack': '',
      'completeCallBack': '',
    };
  },

  getInitialState: function() {
    return {
      'list_height': 0,
      
      'open_mask': false,
      'close_mask': false,
      'open_body': false,
      'close_body': false,
      'mask_show': false,
      
      'tabs':[{name:'请选择'}],
      'tabs_hr_width': 65,
      'tabs_hr_move': 0,
      'tabs_level':1,
      'tabs_ref': false,
      'list': [],
      
      'list_current_1': -1,
      'list_current_2': -1,
      'list_current_3': -1,
    };
  },
  
  componentWillMount() {
    $.ajax({
      type:"get",
      url:'js/allcity.html',
      success:(result)=> {
        city = JSON.parse(result);
        const length = city.length;
        let temp = [];
        for(var i=0;i<length;i++) {
          temp.push({
            'name':city[i].name,
            'level': 1,
            'yi': i,
            'er': -1,
            'san': -1
          });
        }
        this.setState({
          'list': temp
        })
      },
      error:()=> {
        // config.ajaxError(this);
      }
    })
    
  },
  
  componentDidMount() {
    const deviceHeight = $(window).height();
    const bodyHeight = this.refs.body.clientHeight;
    const titleHeight = this.refs.title.clientHeight;
    const tabHeight = this.refs.tab.clientHeight;
    const tabItemFirstWidth = defaultTabWidth = this.refs.tab_item_1.clientWidth;
    this.setState({
      'list_height': deviceHeight - bodyHeight - titleHeight - tabHeight,
      'tbs_hr_width': tabItemFirstWidth
    });
  },
  componentWillReceiveProps(nextProps) {
    if(nextProps.show) {
      this.setState({
        'open_mask': nextProps.show,
        'close_mask': false,
        'mask_show': true
      })
      setTimeout(()=>{
        this.setState({
          'close_body': false,
          'open_body': nextProps.show
        })
        $(document.body).css({
          'overflow':'hidden'
        });
      },150)
    }
  },
  
  //选择条目
  handleSelectItem(level,yi,er,san) {
    let temp = [];
    if(level == 1) {
      const length = city[yi].city.length;
      for(var i=0;i<length;i++) {
        temp.push({
          'name': city[yi].city[i].name,
          'level': 2,
          'yi': yi,
          'er': i,
          'san': -1,
        })
      }
      this.setState({
        'list': temp,
        'tabs': [{'name':city[yi].name},{'name':'请选择'}],
        'tabs_hr_width': defaultTabWidth,
        'tabs_hr_move': this.refs['item_'+ yi].clientWidth + 20,
        'list_current_1': yi,
        'list_current_2': -1,
        'list_current_3': -1,
      })
    }
    if(level == 2) {
      const length = city[yi].city[er].area.length;
      for(var i=0;i<length;i++) {
        temp.push({
          'name': city[yi].city[er].area[i],
          'level': 3,
          'yi': yi,
          'er': er,
          'san': i
        })
      }
      this.setState({
        'list': temp,
        'tabs': [{'name':city[yi].name},{'name':city[yi].city[er].name},{'name':'请选择'}],
        'tabs_hr_width': defaultTabWidth,
        'tabs_hr_move': this.refs.tab_item_1.clientWidth + this.refs['item_' + er].clientWidth + 20,
        'list_current_1': yi,
        'list_current_2': er,
        'list_current_3': i,
      })
      // console.log(this.refs.tab_item_1.clientWidth,this.refs['item_' + er].clientWidth + 20)
    }
    if(level == 3) {
      this.setState({
        'tabs': [{'name':city[yi].name},{'name':city[yi].city[er].name},{'name':city[yi].city[er].area[san]}],
        'tabs_hr_width': this.refs['item_'+ san].clientWidth + 20,
        'list_current_1': yi,
        'list_current_2': er,
        'list_current_3': san,
      })
      this.handleClose();
      this.props.completeCallBack([
        city[this.state.list_current_1].name,
        city[this.state.list_current_1].city[this.state.list_current_2].name,
        city[this.state.list_current_1].city[this.state.list_current_2].area[san]
      ])
    }
  },
  
  //选择tab
  handleSelectTab(index) {
    let temp = [];
    if(index == 0) {
      const length = city.length;
      for(var i=0;i<length;i++) {
        temp.push({
          'name': city[i].name,
          'level': 1,
          'yi': i,
          'er': -1,
          'san': -1,
        })
      }
      this.setState({
        'list': temp,
        'tabs_hr_move': 0,
        'tabs_hr_width': this.refs.tab_item_1.clientWidth
      })
    }
    if(index == 1) {
      const length = city[this.state.list_current_1].city.length;
      for(var i=0;i<length;i++) {
        temp.push({
          'name': city[this.state.list_current_1].city[i].name,
          'level': 2,
          'yi': this.state.list_current_1,
          'er': i,
          'san': -1,
        })
      }
      this.setState({
        'list': temp,
        'tabs_hr_move': this.refs.tab_item_1.clientWidth,
        'tabs_hr_width': this.refs.tab_item_2.clientWidth
      })
    }
    if(index == 2) {
      const length = city[this.state.list_current_1].city[this.state.list_current_2].area.length;
      for(var i=0;i<length;i++) {
        temp.push({
          'name': city[this.state.list_current_1].city[this.state.list_current_2].area[i],
          'level': 3,
          'yi': this.state.list_current_1,
          'er': this.state.list_current_2,
          'san': i,
        })
      }
      this.setState({
        'list': temp,
        'tabs_hr_move': this.refs.tab_item_2.clientWidth + this.refs.tab_item_1.clientWidth,
        'tabs_hr_width': this.refs.tab_item_3.clientWidth
      })
    }
  },   
  
  handleClose() {
    this.setState({
      'open_body': false,
      'close_body': true
    })
    setTimeout(()=>{
      this.setState({
        'open_mask': false,
        'close_mask': true,
        // 'mask_show': false,
      })
      $(document.body).css({
        'overflow':'visible'
      });
    },150);
    if(this.props.closeCallBack) {
      this.props.closeCallBack();
    }
  },
  
  
  handleeStopPropagation(e) {
    e.stopPropagation(); 
  }, 

  render() {
    const listStyle = {
      'height': this.state.list_height
    }
    const maskClass = classNames({
      'weui_mask': true,
      'animated': true,
      'slideInUp': this.state.open_mask,
      'slideInDown2': this.state.close_mask
    });
    const mainClass = classNames({
      'MuiltipleSelect': true, 
      'animated': true,
      'slideInUp': this.state.open_body,
      'slideInDown2': this.state.close_body
    });
    const hrStyle = {
      'width': this.state.tabs_hr_width,
      'transform': `translate3d(${this.state.tabs_hr_move}px,0,0)`,
      '-webkit-transform': `translate3d(${this.state.tabs_hr_move}px,0,0)`,
    }
    const maskStyle = {
      'visibility': this.state.mask_show ? 'visible' : 'hidden'
    }
    return (
      <Mask className={maskClass} onClick={this.handleClose} style={maskStyle}>
        <div className={mainClass} {...this.props} ref='body' onClick={this.handleeStopPropagation}>
          <div className='MuiltipleSelect-title' ref='title'>所在地区</div>
          <div className='MuiltipleSelect-tab' ref='tab'>
            {this.state.tabs.map((tab,i)=>{
              return (
                <div className='MuiltipleSelect-tab-item' ref={`tab_item_${i+1}`} key={i} 
                     onClick={this.handleSelectTab.bind(this,i)}>{tab.name}</div>
              )})
            }
            <div className='MuiltipleSelect-tab-hr' style={hrStyle}></div>
          </div>
          <ul className='MuiltipleSelect-list' style={listStyle}>
           {this.state.list.map((item,i)=>{
             return(
               <li onClick={this.handleSelectItem.bind(this,item.level,item.yi,item.er,item.san)} key={i}>
                <span ref={`item_${i}`}>{item.name}</span>
               </li>
             )})
           }
          </ul>
        </div>
      </Mask>
    );
  }
});

export default MultipleSelect;
