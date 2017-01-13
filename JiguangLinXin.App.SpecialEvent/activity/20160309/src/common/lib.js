"use strict";
// import 'skui-touch/lib/skui-touch.css';
//加载字体
// import 'skui-touch/lib/fonts/ratchicons.svg';
// import 'skui-touch/lib/fonts/ratchicons.ttf';
// import 'skui-touch/lib/fonts/ratchicons.woff';
//active伪类无效解决方案
import FastClick from 'fastclick';
document.body.addEventListener('touchstart', function () { });

window.addEventListener('load', () => {
  FastClick.attach(document.body);
});