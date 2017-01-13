"use strict";

var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) { return typeof obj; } : function (obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol ? "symbol" : typeof obj; };

(function e(t, n, r) {
  function s(o, u) {
    if (!n[o]) {
      if (!t[o]) {
        var a = typeof require == "function" && require;if (!u && a) return a(o, !0);if (i) return i(o, !0);var f = new Error("Cannot find module '" + o + "'");throw f.code = "MODULE_NOT_FOUND", f;
      }var l = n[o] = { exports: {} };t[o][0].call(l.exports, function (e) {
        var n = t[o][1][e];return s(n ? n : e);
      }, l, l.exports, e, t, n, r);
    }return n[o].exports;
  }var i = typeof require == "function" && require;for (var o = 0; o < r.length; o++) {
    s(r[o]);
  }return s;
})({ 1: [function (require, module, exports) {
    (function (process, global) {
      /*!
       * @overview es6-promise - a tiny implementation of Promises/A+.
       * @copyright Copyright (c) 2014 Yehuda Katz, Tom Dale, Stefan Penner and contributors (Conversion to ES6 API by Jake Archibald)
       * @license   Licensed under MIT license
       *            See https://raw.githubusercontent.com/jakearchibald/es6-promise/master/LICENSE
       * @version   3.2.1
       */

      (function () {
        "use strict";

        function lib$es6$promise$utils$$objectOrFunction(x) {
          return typeof x === 'function' || (typeof x === "undefined" ? "undefined" : _typeof(x)) === 'object' && x !== null;
        }

        function lib$es6$promise$utils$$isFunction(x) {
          return typeof x === 'function';
        }

        function lib$es6$promise$utils$$isMaybeThenable(x) {
          return (typeof x === "undefined" ? "undefined" : _typeof(x)) === 'object' && x !== null;
        }

        var lib$es6$promise$utils$$_isArray;
        if (!Array.isArray) {
          lib$es6$promise$utils$$_isArray = function lib$es6$promise$utils$$_isArray(x) {
            return Object.prototype.toString.call(x) === '[object Array]';
          };
        } else {
          lib$es6$promise$utils$$_isArray = Array.isArray;
        }

        var lib$es6$promise$utils$$isArray = lib$es6$promise$utils$$_isArray;
        var lib$es6$promise$asap$$len = 0;
        var lib$es6$promise$asap$$vertxNext;
        var lib$es6$promise$asap$$customSchedulerFn;

        var lib$es6$promise$asap$$asap = function asap(callback, arg) {
          lib$es6$promise$asap$$queue[lib$es6$promise$asap$$len] = callback;
          lib$es6$promise$asap$$queue[lib$es6$promise$asap$$len + 1] = arg;
          lib$es6$promise$asap$$len += 2;
          if (lib$es6$promise$asap$$len === 2) {
            // If len is 2, that means that we need to schedule an async flush.
            // If additional callbacks are queued before the queue is flushed, they
            // will be processed by this flush that we are scheduling.
            if (lib$es6$promise$asap$$customSchedulerFn) {
              lib$es6$promise$asap$$customSchedulerFn(lib$es6$promise$asap$$flush);
            } else {
              lib$es6$promise$asap$$scheduleFlush();
            }
          }
        };

        function lib$es6$promise$asap$$setScheduler(scheduleFn) {
          lib$es6$promise$asap$$customSchedulerFn = scheduleFn;
        }

        function lib$es6$promise$asap$$setAsap(asapFn) {
          lib$es6$promise$asap$$asap = asapFn;
        }

        var lib$es6$promise$asap$$browserWindow = typeof window !== 'undefined' ? window : undefined;
        var lib$es6$promise$asap$$browserGlobal = lib$es6$promise$asap$$browserWindow || {};
        var lib$es6$promise$asap$$BrowserMutationObserver = lib$es6$promise$asap$$browserGlobal.MutationObserver || lib$es6$promise$asap$$browserGlobal.WebKitMutationObserver;
        var lib$es6$promise$asap$$isNode = typeof self === 'undefined' && typeof process !== 'undefined' && {}.toString.call(process) === '[object process]';

        // test for web worker but not in IE10
        var lib$es6$promise$asap$$isWorker = typeof Uint8ClampedArray !== 'undefined' && typeof importScripts !== 'undefined' && typeof MessageChannel !== 'undefined';

        // node
        function lib$es6$promise$asap$$useNextTick() {
          // node version 0.10.x displays a deprecation warning when nextTick is used recursively
          // see https://github.com/cujojs/when/issues/410 for details
          return function () {
            process.nextTick(lib$es6$promise$asap$$flush);
          };
        }

        // vertx
        function lib$es6$promise$asap$$useVertxTimer() {
          return function () {
            lib$es6$promise$asap$$vertxNext(lib$es6$promise$asap$$flush);
          };
        }

        function lib$es6$promise$asap$$useMutationObserver() {
          var iterations = 0;
          var observer = new lib$es6$promise$asap$$BrowserMutationObserver(lib$es6$promise$asap$$flush);
          var node = document.createTextNode('');
          observer.observe(node, { characterData: true });

          return function () {
            node.data = iterations = ++iterations % 2;
          };
        }

        // web worker
        function lib$es6$promise$asap$$useMessageChannel() {
          var channel = new MessageChannel();
          channel.port1.onmessage = lib$es6$promise$asap$$flush;
          return function () {
            channel.port2.postMessage(0);
          };
        }

        function lib$es6$promise$asap$$useSetTimeout() {
          return function () {
            setTimeout(lib$es6$promise$asap$$flush, 1);
          };
        }

        var lib$es6$promise$asap$$queue = new Array(1000);
        function lib$es6$promise$asap$$flush() {
          for (var i = 0; i < lib$es6$promise$asap$$len; i += 2) {
            var callback = lib$es6$promise$asap$$queue[i];
            var arg = lib$es6$promise$asap$$queue[i + 1];

            callback(arg);

            lib$es6$promise$asap$$queue[i] = undefined;
            lib$es6$promise$asap$$queue[i + 1] = undefined;
          }

          lib$es6$promise$asap$$len = 0;
        }

        function lib$es6$promise$asap$$attemptVertx() {
          try {
            var r = require;
            var vertx = r('vertx');
            lib$es6$promise$asap$$vertxNext = vertx.runOnLoop || vertx.runOnContext;
            return lib$es6$promise$asap$$useVertxTimer();
          } catch (e) {
            return lib$es6$promise$asap$$useSetTimeout();
          }
        }

        var lib$es6$promise$asap$$scheduleFlush;
        // Decide what async method to use to triggering processing of queued callbacks:
        if (lib$es6$promise$asap$$isNode) {
          lib$es6$promise$asap$$scheduleFlush = lib$es6$promise$asap$$useNextTick();
        } else if (lib$es6$promise$asap$$BrowserMutationObserver) {
          lib$es6$promise$asap$$scheduleFlush = lib$es6$promise$asap$$useMutationObserver();
        } else if (lib$es6$promise$asap$$isWorker) {
          lib$es6$promise$asap$$scheduleFlush = lib$es6$promise$asap$$useMessageChannel();
        } else if (lib$es6$promise$asap$$browserWindow === undefined && typeof require === 'function') {
          lib$es6$promise$asap$$scheduleFlush = lib$es6$promise$asap$$attemptVertx();
        } else {
          lib$es6$promise$asap$$scheduleFlush = lib$es6$promise$asap$$useSetTimeout();
        }
        function lib$es6$promise$then$$then(onFulfillment, onRejection) {
          var parent = this;

          var child = new this.constructor(lib$es6$promise$$internal$$noop);

          if (child[lib$es6$promise$$internal$$PROMISE_ID] === undefined) {
            lib$es6$promise$$internal$$makePromise(child);
          }

          var state = parent._state;

          if (state) {
            var callback = arguments[state - 1];
            lib$es6$promise$asap$$asap(function () {
              lib$es6$promise$$internal$$invokeCallback(state, child, callback, parent._result);
            });
          } else {
            lib$es6$promise$$internal$$subscribe(parent, child, onFulfillment, onRejection);
          }

          return child;
        }
        var lib$es6$promise$then$$default = lib$es6$promise$then$$then;
        function lib$es6$promise$promise$resolve$$resolve(object) {
          /*jshint validthis:true */
          var Constructor = this;

          if (object && (typeof object === "undefined" ? "undefined" : _typeof(object)) === 'object' && object.constructor === Constructor) {
            return object;
          }

          var promise = new Constructor(lib$es6$promise$$internal$$noop);
          lib$es6$promise$$internal$$resolve(promise, object);
          return promise;
        }
        var lib$es6$promise$promise$resolve$$default = lib$es6$promise$promise$resolve$$resolve;
        var lib$es6$promise$$internal$$PROMISE_ID = Math.random().toString(36).substring(16);

        function lib$es6$promise$$internal$$noop() {}

        var lib$es6$promise$$internal$$PENDING = void 0;
        var lib$es6$promise$$internal$$FULFILLED = 1;
        var lib$es6$promise$$internal$$REJECTED = 2;

        var lib$es6$promise$$internal$$GET_THEN_ERROR = new lib$es6$promise$$internal$$ErrorObject();

        function lib$es6$promise$$internal$$selfFulfillment() {
          return new TypeError("You cannot resolve a promise with itself");
        }

        function lib$es6$promise$$internal$$cannotReturnOwn() {
          return new TypeError('A promises callback cannot return that same promise.');
        }

        function lib$es6$promise$$internal$$getThen(promise) {
          try {
            return promise.then;
          } catch (error) {
            lib$es6$promise$$internal$$GET_THEN_ERROR.error = error;
            return lib$es6$promise$$internal$$GET_THEN_ERROR;
          }
        }

        function lib$es6$promise$$internal$$tryThen(then, value, fulfillmentHandler, rejectionHandler) {
          try {
            then.call(value, fulfillmentHandler, rejectionHandler);
          } catch (e) {
            return e;
          }
        }

        function lib$es6$promise$$internal$$handleForeignThenable(promise, thenable, then) {
          lib$es6$promise$asap$$asap(function (promise) {
            var sealed = false;
            var error = lib$es6$promise$$internal$$tryThen(then, thenable, function (value) {
              if (sealed) {
                return;
              }
              sealed = true;
              if (thenable !== value) {
                lib$es6$promise$$internal$$resolve(promise, value);
              } else {
                lib$es6$promise$$internal$$fulfill(promise, value);
              }
            }, function (reason) {
              if (sealed) {
                return;
              }
              sealed = true;

              lib$es6$promise$$internal$$reject(promise, reason);
            }, 'Settle: ' + (promise._label || ' unknown promise'));

            if (!sealed && error) {
              sealed = true;
              lib$es6$promise$$internal$$reject(promise, error);
            }
          }, promise);
        }

        function lib$es6$promise$$internal$$handleOwnThenable(promise, thenable) {
          if (thenable._state === lib$es6$promise$$internal$$FULFILLED) {
            lib$es6$promise$$internal$$fulfill(promise, thenable._result);
          } else if (thenable._state === lib$es6$promise$$internal$$REJECTED) {
            lib$es6$promise$$internal$$reject(promise, thenable._result);
          } else {
            lib$es6$promise$$internal$$subscribe(thenable, undefined, function (value) {
              lib$es6$promise$$internal$$resolve(promise, value);
            }, function (reason) {
              lib$es6$promise$$internal$$reject(promise, reason);
            });
          }
        }

        function lib$es6$promise$$internal$$handleMaybeThenable(promise, maybeThenable, then) {
          if (maybeThenable.constructor === promise.constructor && then === lib$es6$promise$then$$default && constructor.resolve === lib$es6$promise$promise$resolve$$default) {
            lib$es6$promise$$internal$$handleOwnThenable(promise, maybeThenable);
          } else {
            if (then === lib$es6$promise$$internal$$GET_THEN_ERROR) {
              lib$es6$promise$$internal$$reject(promise, lib$es6$promise$$internal$$GET_THEN_ERROR.error);
            } else if (then === undefined) {
              lib$es6$promise$$internal$$fulfill(promise, maybeThenable);
            } else if (lib$es6$promise$utils$$isFunction(then)) {
              lib$es6$promise$$internal$$handleForeignThenable(promise, maybeThenable, then);
            } else {
              lib$es6$promise$$internal$$fulfill(promise, maybeThenable);
            }
          }
        }

        function lib$es6$promise$$internal$$resolve(promise, value) {
          if (promise === value) {
            lib$es6$promise$$internal$$reject(promise, lib$es6$promise$$internal$$selfFulfillment());
          } else if (lib$es6$promise$utils$$objectOrFunction(value)) {
            lib$es6$promise$$internal$$handleMaybeThenable(promise, value, lib$es6$promise$$internal$$getThen(value));
          } else {
            lib$es6$promise$$internal$$fulfill(promise, value);
          }
        }

        function lib$es6$promise$$internal$$publishRejection(promise) {
          if (promise._onerror) {
            promise._onerror(promise._result);
          }

          lib$es6$promise$$internal$$publish(promise);
        }

        function lib$es6$promise$$internal$$fulfill(promise, value) {
          if (promise._state !== lib$es6$promise$$internal$$PENDING) {
            return;
          }

          promise._result = value;
          promise._state = lib$es6$promise$$internal$$FULFILLED;

          if (promise._subscribers.length !== 0) {
            lib$es6$promise$asap$$asap(lib$es6$promise$$internal$$publish, promise);
          }
        }

        function lib$es6$promise$$internal$$reject(promise, reason) {
          if (promise._state !== lib$es6$promise$$internal$$PENDING) {
            return;
          }
          promise._state = lib$es6$promise$$internal$$REJECTED;
          promise._result = reason;

          lib$es6$promise$asap$$asap(lib$es6$promise$$internal$$publishRejection, promise);
        }

        function lib$es6$promise$$internal$$subscribe(parent, child, onFulfillment, onRejection) {
          var subscribers = parent._subscribers;
          var length = subscribers.length;

          parent._onerror = null;

          subscribers[length] = child;
          subscribers[length + lib$es6$promise$$internal$$FULFILLED] = onFulfillment;
          subscribers[length + lib$es6$promise$$internal$$REJECTED] = onRejection;

          if (length === 0 && parent._state) {
            lib$es6$promise$asap$$asap(lib$es6$promise$$internal$$publish, parent);
          }
        }

        function lib$es6$promise$$internal$$publish(promise) {
          var subscribers = promise._subscribers;
          var settled = promise._state;

          if (subscribers.length === 0) {
            return;
          }

          var child,
              callback,
              detail = promise._result;

          for (var i = 0; i < subscribers.length; i += 3) {
            child = subscribers[i];
            callback = subscribers[i + settled];

            if (child) {
              lib$es6$promise$$internal$$invokeCallback(settled, child, callback, detail);
            } else {
              callback(detail);
            }
          }

          promise._subscribers.length = 0;
        }

        function lib$es6$promise$$internal$$ErrorObject() {
          this.error = null;
        }

        var lib$es6$promise$$internal$$TRY_CATCH_ERROR = new lib$es6$promise$$internal$$ErrorObject();

        function lib$es6$promise$$internal$$tryCatch(callback, detail) {
          try {
            return callback(detail);
          } catch (e) {
            lib$es6$promise$$internal$$TRY_CATCH_ERROR.error = e;
            return lib$es6$promise$$internal$$TRY_CATCH_ERROR;
          }
        }

        function lib$es6$promise$$internal$$invokeCallback(settled, promise, callback, detail) {
          var hasCallback = lib$es6$promise$utils$$isFunction(callback),
              value,
              error,
              succeeded,
              failed;

          if (hasCallback) {
            value = lib$es6$promise$$internal$$tryCatch(callback, detail);

            if (value === lib$es6$promise$$internal$$TRY_CATCH_ERROR) {
              failed = true;
              error = value.error;
              value = null;
            } else {
              succeeded = true;
            }

            if (promise === value) {
              lib$es6$promise$$internal$$reject(promise, lib$es6$promise$$internal$$cannotReturnOwn());
              return;
            }
          } else {
            value = detail;
            succeeded = true;
          }

          if (promise._state !== lib$es6$promise$$internal$$PENDING) {
            // noop
          } else if (hasCallback && succeeded) {
              lib$es6$promise$$internal$$resolve(promise, value);
            } else if (failed) {
              lib$es6$promise$$internal$$reject(promise, error);
            } else if (settled === lib$es6$promise$$internal$$FULFILLED) {
              lib$es6$promise$$internal$$fulfill(promise, value);
            } else if (settled === lib$es6$promise$$internal$$REJECTED) {
              lib$es6$promise$$internal$$reject(promise, value);
            }
        }

        function lib$es6$promise$$internal$$initializePromise(promise, resolver) {
          try {
            resolver(function resolvePromise(value) {
              lib$es6$promise$$internal$$resolve(promise, value);
            }, function rejectPromise(reason) {
              lib$es6$promise$$internal$$reject(promise, reason);
            });
          } catch (e) {
            lib$es6$promise$$internal$$reject(promise, e);
          }
        }

        var lib$es6$promise$$internal$$id = 0;
        function lib$es6$promise$$internal$$nextId() {
          return lib$es6$promise$$internal$$id++;
        }

        function lib$es6$promise$$internal$$makePromise(promise) {
          promise[lib$es6$promise$$internal$$PROMISE_ID] = lib$es6$promise$$internal$$id++;
          promise._state = undefined;
          promise._result = undefined;
          promise._subscribers = [];
        }

        function lib$es6$promise$promise$all$$all(entries) {
          return new lib$es6$promise$enumerator$$default(this, entries).promise;
        }
        var lib$es6$promise$promise$all$$default = lib$es6$promise$promise$all$$all;
        function lib$es6$promise$promise$race$$race(entries) {
          /*jshint validthis:true */
          var Constructor = this;

          if (!lib$es6$promise$utils$$isArray(entries)) {
            return new Constructor(function (resolve, reject) {
              reject(new TypeError('You must pass an array to race.'));
            });
          } else {
            return new Constructor(function (resolve, reject) {
              var length = entries.length;
              for (var i = 0; i < length; i++) {
                Constructor.resolve(entries[i]).then(resolve, reject);
              }
            });
          }
        }
        var lib$es6$promise$promise$race$$default = lib$es6$promise$promise$race$$race;
        function lib$es6$promise$promise$reject$$reject(reason) {
          /*jshint validthis:true */
          var Constructor = this;
          var promise = new Constructor(lib$es6$promise$$internal$$noop);
          lib$es6$promise$$internal$$reject(promise, reason);
          return promise;
        }
        var lib$es6$promise$promise$reject$$default = lib$es6$promise$promise$reject$$reject;

        function lib$es6$promise$promise$$needsResolver() {
          throw new TypeError('You must pass a resolver function as the first argument to the promise constructor');
        }

        function lib$es6$promise$promise$$needsNew() {
          throw new TypeError("Failed to construct 'Promise': Please use the 'new' operator, this object constructor cannot be called as a function.");
        }

        var lib$es6$promise$promise$$default = lib$es6$promise$promise$$Promise;
        /**
          Promise objects represent the eventual result of an asynchronous operation. The
          primary way of interacting with a promise is through its `then` method, which
          registers callbacks to receive either a promise's eventual value or the reason
          why the promise cannot be fulfilled.
           Terminology
          -----------
           - `promise` is an object or function with a `then` method whose behavior conforms to this specification.
          - `thenable` is an object or function that defines a `then` method.
          - `value` is any legal JavaScript value (including undefined, a thenable, or a promise).
          - `exception` is a value that is thrown using the throw statement.
          - `reason` is a value that indicates why a promise was rejected.
          - `settled` the final resting state of a promise, fulfilled or rejected.
           A promise can be in one of three states: pending, fulfilled, or rejected.
           Promises that are fulfilled have a fulfillment value and are in the fulfilled
          state.  Promises that are rejected have a rejection reason and are in the
          rejected state.  A fulfillment value is never a thenable.
           Promises can also be said to *resolve* a value.  If this value is also a
          promise, then the original promise's settled state will match the value's
          settled state.  So a promise that *resolves* a promise that rejects will
          itself reject, and a promise that *resolves* a promise that fulfills will
          itself fulfill.
            Basic Usage:
          ------------
           ```js
          var promise = new Promise(function(resolve, reject) {
            // on success
            resolve(value);
             // on failure
            reject(reason);
          });
           promise.then(function(value) {
            // on fulfillment
          }, function(reason) {
            // on rejection
          });
          ```
           Advanced Usage:
          ---------------
           Promises shine when abstracting away asynchronous interactions such as
          `XMLHttpRequest`s.
           ```js
          function getJSON(url) {
            return new Promise(function(resolve, reject){
              var xhr = new XMLHttpRequest();
               xhr.open('GET', url);
              xhr.onreadystatechange = handler;
              xhr.responseType = 'json';
              xhr.setRequestHeader('Accept', 'application/json');
              xhr.send();
               function handler() {
                if (this.readyState === this.DONE) {
                  if (this.status === 200) {
                    resolve(this.response);
                  } else {
                    reject(new Error('getJSON: `' + url + '` failed with status: [' + this.status + ']'));
                  }
                }
              };
            });
          }
           getJSON('/posts.json').then(function(json) {
            // on fulfillment
          }, function(reason) {
            // on rejection
          });
          ```
           Unlike callbacks, promises are great composable primitives.
           ```js
          Promise.all([
            getJSON('/posts'),
            getJSON('/comments')
          ]).then(function(values){
            values[0] // => postsJSON
            values[1] // => commentsJSON
             return values;
          });
          ```
           @class Promise
          @param {function} resolver
          Useful for tooling.
          @constructor
        */
        function lib$es6$promise$promise$$Promise(resolver) {
          this[lib$es6$promise$$internal$$PROMISE_ID] = lib$es6$promise$$internal$$nextId();
          this._result = this._state = undefined;
          this._subscribers = [];

          if (lib$es6$promise$$internal$$noop !== resolver) {
            typeof resolver !== 'function' && lib$es6$promise$promise$$needsResolver();
            this instanceof lib$es6$promise$promise$$Promise ? lib$es6$promise$$internal$$initializePromise(this, resolver) : lib$es6$promise$promise$$needsNew();
          }
        }

        lib$es6$promise$promise$$Promise.all = lib$es6$promise$promise$all$$default;
        lib$es6$promise$promise$$Promise.race = lib$es6$promise$promise$race$$default;
        lib$es6$promise$promise$$Promise.resolve = lib$es6$promise$promise$resolve$$default;
        lib$es6$promise$promise$$Promise.reject = lib$es6$promise$promise$reject$$default;
        lib$es6$promise$promise$$Promise._setScheduler = lib$es6$promise$asap$$setScheduler;
        lib$es6$promise$promise$$Promise._setAsap = lib$es6$promise$asap$$setAsap;
        lib$es6$promise$promise$$Promise._asap = lib$es6$promise$asap$$asap;

        lib$es6$promise$promise$$Promise.prototype = {
          constructor: lib$es6$promise$promise$$Promise,

          /**
            The primary way of interacting with a promise is through its `then` method,
            which registers callbacks to receive either a promise's eventual value or the
            reason why the promise cannot be fulfilled.
             ```js
            findUser().then(function(user){
              // user is available
            }, function(reason){
              // user is unavailable, and you are given the reason why
            });
            ```
             Chaining
            --------
             The return value of `then` is itself a promise.  This second, 'downstream'
            promise is resolved with the return value of the first promise's fulfillment
            or rejection handler, or rejected if the handler throws an exception.
             ```js
            findUser().then(function (user) {
              return user.name;
            }, function (reason) {
              return 'default name';
            }).then(function (userName) {
              // If `findUser` fulfilled, `userName` will be the user's name, otherwise it
              // will be `'default name'`
            });
             findUser().then(function (user) {
              throw new Error('Found user, but still unhappy');
            }, function (reason) {
              throw new Error('`findUser` rejected and we're unhappy');
            }).then(function (value) {
              // never reached
            }, function (reason) {
              // if `findUser` fulfilled, `reason` will be 'Found user, but still unhappy'.
              // If `findUser` rejected, `reason` will be '`findUser` rejected and we're unhappy'.
            });
            ```
            If the downstream promise does not specify a rejection handler, rejection reasons will be propagated further downstream.
             ```js
            findUser().then(function (user) {
              throw new PedagogicalException('Upstream error');
            }).then(function (value) {
              // never reached
            }).then(function (value) {
              // never reached
            }, function (reason) {
              // The `PedgagocialException` is propagated all the way down to here
            });
            ```
             Assimilation
            ------------
             Sometimes the value you want to propagate to a downstream promise can only be
            retrieved asynchronously. This can be achieved by returning a promise in the
            fulfillment or rejection handler. The downstream promise will then be pending
            until the returned promise is settled. This is called *assimilation*.
             ```js
            findUser().then(function (user) {
              return findCommentsByAuthor(user);
            }).then(function (comments) {
              // The user's comments are now available
            });
            ```
             If the assimliated promise rejects, then the downstream promise will also reject.
             ```js
            findUser().then(function (user) {
              return findCommentsByAuthor(user);
            }).then(function (comments) {
              // If `findCommentsByAuthor` fulfills, we'll have the value here
            }, function (reason) {
              // If `findCommentsByAuthor` rejects, we'll have the reason here
            });
            ```
             Simple Example
            --------------
             Synchronous Example
             ```javascript
            var result;
             try {
              result = findResult();
              // success
            } catch(reason) {
              // failure
            }
            ```
             Errback Example
             ```js
            findResult(function(result, err){
              if (err) {
                // failure
              } else {
                // success
              }
            });
            ```
             Promise Example;
             ```javascript
            findResult().then(function(result){
              // success
            }, function(reason){
              // failure
            });
            ```
             Advanced Example
            --------------
             Synchronous Example
             ```javascript
            var author, books;
             try {
              author = findAuthor();
              books  = findBooksByAuthor(author);
              // success
            } catch(reason) {
              // failure
            }
            ```
             Errback Example
             ```js
             function foundBooks(books) {
             }
             function failure(reason) {
             }
             findAuthor(function(author, err){
              if (err) {
                failure(err);
                // failure
              } else {
                try {
                  findBoooksByAuthor(author, function(books, err) {
                    if (err) {
                      failure(err);
                    } else {
                      try {
                        foundBooks(books);
                      } catch(reason) {
                        failure(reason);
                      }
                    }
                  });
                } catch(error) {
                  failure(err);
                }
                // success
              }
            });
            ```
             Promise Example;
             ```javascript
            findAuthor().
              then(findBooksByAuthor).
              then(function(books){
                // found books
            }).catch(function(reason){
              // something went wrong
            });
            ```
             @method then
            @param {Function} onFulfilled
            @param {Function} onRejected
            Useful for tooling.
            @return {Promise}
          */
          then: lib$es6$promise$then$$default,

          /**
            `catch` is simply sugar for `then(undefined, onRejection)` which makes it the same
            as the catch block of a try/catch statement.
             ```js
            function findAuthor(){
              throw new Error('couldn't find that author');
            }
             // synchronous
            try {
              findAuthor();
            } catch(reason) {
              // something went wrong
            }
             // async with promises
            findAuthor().catch(function(reason){
              // something went wrong
            });
            ```
             @method catch
            @param {Function} onRejection
            Useful for tooling.
            @return {Promise}
          */
          'catch': function _catch(onRejection) {
            return this.then(null, onRejection);
          }
        };
        var lib$es6$promise$enumerator$$default = lib$es6$promise$enumerator$$Enumerator;
        function lib$es6$promise$enumerator$$Enumerator(Constructor, input) {
          this._instanceConstructor = Constructor;
          this.promise = new Constructor(lib$es6$promise$$internal$$noop);

          if (!this.promise[lib$es6$promise$$internal$$PROMISE_ID]) {
            lib$es6$promise$$internal$$makePromise(this.promise);
          }

          if (lib$es6$promise$utils$$isArray(input)) {
            this._input = input;
            this.length = input.length;
            this._remaining = input.length;

            this._result = new Array(this.length);

            if (this.length === 0) {
              lib$es6$promise$$internal$$fulfill(this.promise, this._result);
            } else {
              this.length = this.length || 0;
              this._enumerate();
              if (this._remaining === 0) {
                lib$es6$promise$$internal$$fulfill(this.promise, this._result);
              }
            }
          } else {
            lib$es6$promise$$internal$$reject(this.promise, lib$es6$promise$enumerator$$validationError());
          }
        }

        function lib$es6$promise$enumerator$$validationError() {
          return new Error('Array Methods must be provided an Array');
        }

        lib$es6$promise$enumerator$$Enumerator.prototype._enumerate = function () {
          var length = this.length;
          var input = this._input;

          for (var i = 0; this._state === lib$es6$promise$$internal$$PENDING && i < length; i++) {
            this._eachEntry(input[i], i);
          }
        };

        lib$es6$promise$enumerator$$Enumerator.prototype._eachEntry = function (entry, i) {
          var c = this._instanceConstructor;
          var resolve = c.resolve;

          if (resolve === lib$es6$promise$promise$resolve$$default) {
            var then = lib$es6$promise$$internal$$getThen(entry);

            if (then === lib$es6$promise$then$$default && entry._state !== lib$es6$promise$$internal$$PENDING) {
              this._settledAt(entry._state, i, entry._result);
            } else if (typeof then !== 'function') {
              this._remaining--;
              this._result[i] = entry;
            } else if (c === lib$es6$promise$promise$$default) {
              var promise = new c(lib$es6$promise$$internal$$noop);
              lib$es6$promise$$internal$$handleMaybeThenable(promise, entry, then);
              this._willSettleAt(promise, i);
            } else {
              this._willSettleAt(new c(function (resolve) {
                resolve(entry);
              }), i);
            }
          } else {
            this._willSettleAt(resolve(entry), i);
          }
        };

        lib$es6$promise$enumerator$$Enumerator.prototype._settledAt = function (state, i, value) {
          var promise = this.promise;

          if (promise._state === lib$es6$promise$$internal$$PENDING) {
            this._remaining--;

            if (state === lib$es6$promise$$internal$$REJECTED) {
              lib$es6$promise$$internal$$reject(promise, value);
            } else {
              this._result[i] = value;
            }
          }

          if (this._remaining === 0) {
            lib$es6$promise$$internal$$fulfill(promise, this._result);
          }
        };

        lib$es6$promise$enumerator$$Enumerator.prototype._willSettleAt = function (promise, i) {
          var enumerator = this;

          lib$es6$promise$$internal$$subscribe(promise, undefined, function (value) {
            enumerator._settledAt(lib$es6$promise$$internal$$FULFILLED, i, value);
          }, function (reason) {
            enumerator._settledAt(lib$es6$promise$$internal$$REJECTED, i, reason);
          });
        };
        function lib$es6$promise$polyfill$$polyfill() {
          var local;

          if (typeof global !== 'undefined') {
            local = global;
          } else if (typeof self !== 'undefined') {
            local = self;
          } else {
            try {
              local = Function('return this')();
            } catch (e) {
              throw new Error('polyfill failed because global object is unavailable in this environment');
            }
          }

          var P = local.Promise;

          if (P && Object.prototype.toString.call(P.resolve()) === '[object Promise]' && !P.cast) {
            return;
          }

          local.Promise = lib$es6$promise$promise$$default;
        }
        var lib$es6$promise$polyfill$$default = lib$es6$promise$polyfill$$polyfill;

        var lib$es6$promise$umd$$ES6Promise = {
          'Promise': lib$es6$promise$promise$$default,
          'polyfill': lib$es6$promise$polyfill$$default
        };

        /* global define:true module:true window: true */
        if (typeof define === 'function' && define['amd']) {
          define(function () {
            return lib$es6$promise$umd$$ES6Promise;
          });
        } else if (typeof module !== 'undefined' && module['exports']) {
          module['exports'] = lib$es6$promise$umd$$ES6Promise;
        } else if (typeof this !== 'undefined') {
          this['ES6Promise'] = lib$es6$promise$umd$$ES6Promise;
        }

        lib$es6$promise$polyfill$$default();
      }).call(this);
    }).call(this, require('_process'), typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {});
  }, { "_process": 4 }], 2: [function (require, module, exports) {
    ;(function () {
      'use strict';

      /**
       * @preserve FastClick: polyfill to remove click delays on browsers with touch UIs.
       *
       * @codingstandard ftlabs-jsv2
       * @copyright The Financial Times Limited [All Rights Reserved]
       * @license MIT License (see LICENSE.txt)
       */

      /*jslint browser:true, node:true*/
      /*global define, Event, Node*/

      /**
       * Instantiate fast-clicking listeners on the specified layer.
       *
       * @constructor
       * @param {Element} layer The layer to listen on
       * @param {Object} [options={}] The options to override the defaults
       */

      function FastClick(layer, options) {
        var oldOnClick;

        options = options || {};

        /**
         * Whether a click is currently being tracked.
         *
         * @type boolean
         */
        this.trackingClick = false;

        /**
         * Timestamp for when click tracking started.
         *
         * @type number
         */
        this.trackingClickStart = 0;

        /**
         * The element being tracked for a click.
         *
         * @type EventTarget
         */
        this.targetElement = null;

        /**
         * X-coordinate of touch start event.
         *
         * @type number
         */
        this.touchStartX = 0;

        /**
         * Y-coordinate of touch start event.
         *
         * @type number
         */
        this.touchStartY = 0;

        /**
         * ID of the last touch, retrieved from Touch.identifier.
         *
         * @type number
         */
        this.lastTouchIdentifier = 0;

        /**
         * Touchmove boundary, beyond which a click will be cancelled.
         *
         * @type number
         */
        this.touchBoundary = options.touchBoundary || 10;

        /**
         * The FastClick layer.
         *
         * @type Element
         */
        this.layer = layer;

        /**
         * The minimum time between tap(touchstart and touchend) events
         *
         * @type number
         */
        this.tapDelay = options.tapDelay || 200;

        /**
         * The maximum time for a tap
         *
         * @type number
         */
        this.tapTimeout = options.tapTimeout || 700;

        if (FastClick.notNeeded(layer)) {
          return;
        }

        // Some old versions of Android don't have Function.prototype.bind
        function bind(method, context) {
          return function () {
            return method.apply(context, arguments);
          };
        }

        var methods = ['onMouse', 'onClick', 'onTouchStart', 'onTouchMove', 'onTouchEnd', 'onTouchCancel'];
        var context = this;
        for (var i = 0, l = methods.length; i < l; i++) {
          context[methods[i]] = bind(context[methods[i]], context);
        }

        // Set up event handlers as required
        if (deviceIsAndroid) {
          layer.addEventListener('mouseover', this.onMouse, true);
          layer.addEventListener('mousedown', this.onMouse, true);
          layer.addEventListener('mouseup', this.onMouse, true);
        }

        layer.addEventListener('click', this.onClick, true);
        layer.addEventListener('touchstart', this.onTouchStart, false);
        layer.addEventListener('touchmove', this.onTouchMove, false);
        layer.addEventListener('touchend', this.onTouchEnd, false);
        layer.addEventListener('touchcancel', this.onTouchCancel, false);

        // Hack is required for browsers that don't support Event#stopImmediatePropagation (e.g. Android 2)
        // which is how FastClick normally stops click events bubbling to callbacks registered on the FastClick
        // layer when they are cancelled.
        if (!Event.prototype.stopImmediatePropagation) {
          layer.removeEventListener = function (type, callback, capture) {
            var rmv = Node.prototype.removeEventListener;
            if (type === 'click') {
              rmv.call(layer, type, callback.hijacked || callback, capture);
            } else {
              rmv.call(layer, type, callback, capture);
            }
          };

          layer.addEventListener = function (type, callback, capture) {
            var adv = Node.prototype.addEventListener;
            if (type === 'click') {
              adv.call(layer, type, callback.hijacked || (callback.hijacked = function (event) {
                if (!event.propagationStopped) {
                  callback(event);
                }
              }), capture);
            } else {
              adv.call(layer, type, callback, capture);
            }
          };
        }

        // If a handler is already declared in the element's onclick attribute, it will be fired before
        // FastClick's onClick handler. Fix this by pulling out the user-defined handler function and
        // adding it as listener.
        if (typeof layer.onclick === 'function') {

          // Android browser on at least 3.2 requires a new reference to the function in layer.onclick
          // - the old one won't work if passed to addEventListener directly.
          oldOnClick = layer.onclick;
          layer.addEventListener('click', function (event) {
            oldOnClick(event);
          }, false);
          layer.onclick = null;
        }
      }

      /**
      * Windows Phone 8.1 fakes user agent string to look like Android and iPhone.
      *
      * @type boolean
      */
      var deviceIsWindowsPhone = navigator.userAgent.indexOf("Windows Phone") >= 0;

      /**
       * Android requires exceptions.
       *
       * @type boolean
       */
      var deviceIsAndroid = navigator.userAgent.indexOf('Android') > 0 && !deviceIsWindowsPhone;

      /**
       * iOS requires exceptions.
       *
       * @type boolean
       */
      var deviceIsIOS = /iP(ad|hone|od)/.test(navigator.userAgent) && !deviceIsWindowsPhone;

      /**
       * iOS 4 requires an exception for select elements.
       *
       * @type boolean
       */
      var deviceIsIOS4 = deviceIsIOS && /OS 4_\d(_\d)?/.test(navigator.userAgent);

      /**
       * iOS 6.0-7.* requires the target element to be manually derived
       *
       * @type boolean
       */
      var deviceIsIOSWithBadTarget = deviceIsIOS && /OS [6-7]_\d/.test(navigator.userAgent);

      /**
       * BlackBerry requires exceptions.
       *
       * @type boolean
       */
      var deviceIsBlackBerry10 = navigator.userAgent.indexOf('BB10') > 0;

      /**
       * Determine whether a given element requires a native click.
       *
       * @param {EventTarget|Element} target Target DOM element
       * @returns {boolean} Returns true if the element needs a native click
       */
      FastClick.prototype.needsClick = function (target) {
        switch (target.nodeName.toLowerCase()) {

          // Don't send a synthetic click to disabled inputs (issue #62)
          case 'button':
          case 'select':
          case 'textarea':
            if (target.disabled) {
              return true;
            }

            break;
          case 'input':

            // File inputs need real clicks on iOS 6 due to a browser bug (issue #68)
            if (deviceIsIOS && target.type === 'file' || target.disabled) {
              return true;
            }

            break;
          case 'label':
          case 'iframe': // iOS8 homescreen apps can prevent events bubbling into frames
          case 'video':
            return true;
        }

        return (/\bneedsclick\b/.test(target.className)
        );
      };

      /**
       * Determine whether a given element requires a call to focus to simulate click into element.
       *
       * @param {EventTarget|Element} target Target DOM element
       * @returns {boolean} Returns true if the element requires a call to focus to simulate native click.
       */
      FastClick.prototype.needsFocus = function (target) {
        switch (target.nodeName.toLowerCase()) {
          case 'textarea':
            return true;
          case 'select':
            return !deviceIsAndroid;
          case 'input':
            switch (target.type) {
              case 'button':
              case 'checkbox':
              case 'file':
              case 'image':
              case 'radio':
              case 'submit':
                return false;
            }

            // No point in attempting to focus disabled inputs
            return !target.disabled && !target.readOnly;
          default:
            return (/\bneedsfocus\b/.test(target.className)
            );
        }
      };

      /**
       * Send a click event to the specified element.
       *
       * @param {EventTarget|Element} targetElement
       * @param {Event} event
       */
      FastClick.prototype.sendClick = function (targetElement, event) {
        var clickEvent, touch;

        // On some Android devices activeElement needs to be blurred otherwise the synthetic click will have no effect (#24)
        if (document.activeElement && document.activeElement !== targetElement) {
          document.activeElement.blur();
        }

        touch = event.changedTouches[0];

        // Synthesise a click event, with an extra attribute so it can be tracked
        clickEvent = document.createEvent('MouseEvents');
        clickEvent.initMouseEvent(this.determineEventType(targetElement), true, true, window, 1, touch.screenX, touch.screenY, touch.clientX, touch.clientY, false, false, false, false, 0, null);
        clickEvent.forwardedTouchEvent = true;
        targetElement.dispatchEvent(clickEvent);
      };

      FastClick.prototype.determineEventType = function (targetElement) {

        //Issue #159: Android Chrome Select Box does not open with a synthetic click event
        if (deviceIsAndroid && targetElement.tagName.toLowerCase() === 'select') {
          return 'mousedown';
        }

        return 'click';
      };

      /**
       * @param {EventTarget|Element} targetElement
       */
      FastClick.prototype.focus = function (targetElement) {
        var length;

        // Issue #160: on iOS 7, some input elements (e.g. date datetime month) throw a vague TypeError on setSelectionRange. These elements don't have an integer value for the selectionStart and selectionEnd properties, but unfortunately that can't be used for detection because accessing the properties also throws a TypeError. Just check the type instead. Filed as Apple bug #15122724.
        if (deviceIsIOS && targetElement.setSelectionRange && targetElement.type.indexOf('date') !== 0 && targetElement.type !== 'time' && targetElement.type !== 'month') {
          length = targetElement.value.length;
          targetElement.setSelectionRange(length, length);
        } else {
          targetElement.focus();
        }
      };

      /**
       * Check whether the given target element is a child of a scrollable layer and if so, set a flag on it.
       *
       * @param {EventTarget|Element} targetElement
       */
      FastClick.prototype.updateScrollParent = function (targetElement) {
        var scrollParent, parentElement;

        scrollParent = targetElement.fastClickScrollParent;

        // Attempt to discover whether the target element is contained within a scrollable layer. Re-check if the
        // target element was moved to another parent.
        if (!scrollParent || !scrollParent.contains(targetElement)) {
          parentElement = targetElement;
          do {
            if (parentElement.scrollHeight > parentElement.offsetHeight) {
              scrollParent = parentElement;
              targetElement.fastClickScrollParent = parentElement;
              break;
            }

            parentElement = parentElement.parentElement;
          } while (parentElement);
        }

        // Always update the scroll top tracker if possible.
        if (scrollParent) {
          scrollParent.fastClickLastScrollTop = scrollParent.scrollTop;
        }
      };

      /**
       * @param {EventTarget} targetElement
       * @returns {Element|EventTarget}
       */
      FastClick.prototype.getTargetElementFromEventTarget = function (eventTarget) {

        // On some older browsers (notably Safari on iOS 4.1 - see issue #56) the event target may be a text node.
        if (eventTarget.nodeType === Node.TEXT_NODE) {
          return eventTarget.parentNode;
        }

        return eventTarget;
      };

      /**
       * On touch start, record the position and scroll offset.
       *
       * @param {Event} event
       * @returns {boolean}
       */
      FastClick.prototype.onTouchStart = function (event) {
        var targetElement, touch, selection;

        // Ignore multiple touches, otherwise pinch-to-zoom is prevented if both fingers are on the FastClick element (issue #111).
        if (event.targetTouches.length > 1) {
          return true;
        }

        targetElement = this.getTargetElementFromEventTarget(event.target);
        touch = event.targetTouches[0];

        if (deviceIsIOS) {

          // Only trusted events will deselect text on iOS (issue #49)
          selection = window.getSelection();
          if (selection.rangeCount && !selection.isCollapsed) {
            return true;
          }

          if (!deviceIsIOS4) {

            // Weird things happen on iOS when an alert or confirm dialog is opened from a click event callback (issue #23):
            // when the user next taps anywhere else on the page, new touchstart and touchend events are dispatched
            // with the same identifier as the touch event that previously triggered the click that triggered the alert.
            // Sadly, there is an issue on iOS 4 that causes some normal touch events to have the same identifier as an
            // immediately preceeding touch event (issue #52), so this fix is unavailable on that platform.
            // Issue 120: touch.identifier is 0 when Chrome dev tools 'Emulate touch events' is set with an iOS device UA string,
            // which causes all touch events to be ignored. As this block only applies to iOS, and iOS identifiers are always long,
            // random integers, it's safe to to continue if the identifier is 0 here.
            if (touch.identifier && touch.identifier === this.lastTouchIdentifier) {
              event.preventDefault();
              return false;
            }

            this.lastTouchIdentifier = touch.identifier;

            // If the target element is a child of a scrollable layer (using -webkit-overflow-scrolling: touch) and:
            // 1) the user does a fling scroll on the scrollable layer
            // 2) the user stops the fling scroll with another tap
            // then the event.target of the last 'touchend' event will be the element that was under the user's finger
            // when the fling scroll was started, causing FastClick to send a click event to that layer - unless a check
            // is made to ensure that a parent layer was not scrolled before sending a synthetic click (issue #42).
            this.updateScrollParent(targetElement);
          }
        }

        this.trackingClick = true;
        this.trackingClickStart = event.timeStamp;
        this.targetElement = targetElement;

        this.touchStartX = touch.pageX;
        this.touchStartY = touch.pageY;

        // Prevent phantom clicks on fast double-tap (issue #36)
        if (event.timeStamp - this.lastClickTime < this.tapDelay) {
          event.preventDefault();
        }

        return true;
      };

      /**
       * Based on a touchmove event object, check whether the touch has moved past a boundary since it started.
       *
       * @param {Event} event
       * @returns {boolean}
       */
      FastClick.prototype.touchHasMoved = function (event) {
        var touch = event.changedTouches[0],
            boundary = this.touchBoundary;

        if (Math.abs(touch.pageX - this.touchStartX) > boundary || Math.abs(touch.pageY - this.touchStartY) > boundary) {
          return true;
        }

        return false;
      };

      /**
       * Update the last position.
       *
       * @param {Event} event
       * @returns {boolean}
       */
      FastClick.prototype.onTouchMove = function (event) {
        if (!this.trackingClick) {
          return true;
        }

        // If the touch has moved, cancel the click tracking
        if (this.targetElement !== this.getTargetElementFromEventTarget(event.target) || this.touchHasMoved(event)) {
          this.trackingClick = false;
          this.targetElement = null;
        }

        return true;
      };

      /**
       * Attempt to find the labelled control for the given label element.
       *
       * @param {EventTarget|HTMLLabelElement} labelElement
       * @returns {Element|null}
       */
      FastClick.prototype.findControl = function (labelElement) {

        // Fast path for newer browsers supporting the HTML5 control attribute
        if (labelElement.control !== undefined) {
          return labelElement.control;
        }

        // All browsers under test that support touch events also support the HTML5 htmlFor attribute
        if (labelElement.htmlFor) {
          return document.getElementById(labelElement.htmlFor);
        }

        // If no for attribute exists, attempt to retrieve the first labellable descendant element
        // the list of which is defined here: http://www.w3.org/TR/html5/forms.html#category-label
        return labelElement.querySelector('button, input:not([type=hidden]), keygen, meter, output, progress, select, textarea');
      };

      /**
       * On touch end, determine whether to send a click event at once.
       *
       * @param {Event} event
       * @returns {boolean}
       */
      FastClick.prototype.onTouchEnd = function (event) {
        var forElement,
            trackingClickStart,
            targetTagName,
            scrollParent,
            touch,
            targetElement = this.targetElement;

        if (!this.trackingClick) {
          return true;
        }

        // Prevent phantom clicks on fast double-tap (issue #36)
        if (event.timeStamp - this.lastClickTime < this.tapDelay) {
          this.cancelNextClick = true;
          return true;
        }

        if (event.timeStamp - this.trackingClickStart > this.tapTimeout) {
          return true;
        }

        // Reset to prevent wrong click cancel on input (issue #156).
        this.cancelNextClick = false;

        this.lastClickTime = event.timeStamp;

        trackingClickStart = this.trackingClickStart;
        this.trackingClick = false;
        this.trackingClickStart = 0;

        // On some iOS devices, the targetElement supplied with the event is invalid if the layer
        // is performing a transition or scroll, and has to be re-detected manually. Note that
        // for this to function correctly, it must be called *after* the event target is checked!
        // See issue #57; also filed as rdar://13048589 .
        if (deviceIsIOSWithBadTarget) {
          touch = event.changedTouches[0];

          // In certain cases arguments of elementFromPoint can be negative, so prevent setting targetElement to null
          targetElement = document.elementFromPoint(touch.pageX - window.pageXOffset, touch.pageY - window.pageYOffset) || targetElement;
          targetElement.fastClickScrollParent = this.targetElement.fastClickScrollParent;
        }

        targetTagName = targetElement.tagName.toLowerCase();
        if (targetTagName === 'label') {
          forElement = this.findControl(targetElement);
          if (forElement) {
            this.focus(targetElement);
            if (deviceIsAndroid) {
              return false;
            }

            targetElement = forElement;
          }
        } else if (this.needsFocus(targetElement)) {

          // Case 1: If the touch started a while ago (best guess is 100ms based on tests for issue #36) then focus will be triggered anyway. Return early and unset the target element reference so that the subsequent click will be allowed through.
          // Case 2: Without this exception for input elements tapped when the document is contained in an iframe, then any inputted text won't be visible even though the value attribute is updated as the user types (issue #37).
          if (event.timeStamp - trackingClickStart > 100 || deviceIsIOS && window.top !== window && targetTagName === 'input') {
            this.targetElement = null;
            return false;
          }

          this.focus(targetElement);
          this.sendClick(targetElement, event);

          // Select elements need the event to go through on iOS 4, otherwise the selector menu won't open.
          // Also this breaks opening selects when VoiceOver is active on iOS6, iOS7 (and possibly others)
          if (!deviceIsIOS || targetTagName !== 'select') {
            this.targetElement = null;
            event.preventDefault();
          }

          return false;
        }

        if (deviceIsIOS && !deviceIsIOS4) {

          // Don't send a synthetic click event if the target element is contained within a parent layer that was scrolled
          // and this tap is being used to stop the scrolling (usually initiated by a fling - issue #42).
          scrollParent = targetElement.fastClickScrollParent;
          if (scrollParent && scrollParent.fastClickLastScrollTop !== scrollParent.scrollTop) {
            return true;
          }
        }

        // Prevent the actual click from going though - unless the target node is marked as requiring
        // real clicks or if it is in the whitelist in which case only non-programmatic clicks are permitted.
        if (!this.needsClick(targetElement)) {
          event.preventDefault();
          this.sendClick(targetElement, event);
        }

        return false;
      };

      /**
       * On touch cancel, stop tracking the click.
       *
       * @returns {void}
       */
      FastClick.prototype.onTouchCancel = function () {
        this.trackingClick = false;
        this.targetElement = null;
      };

      /**
       * Determine mouse events which should be permitted.
       *
       * @param {Event} event
       * @returns {boolean}
       */
      FastClick.prototype.onMouse = function (event) {

        // If a target element was never set (because a touch event was never fired) allow the event
        if (!this.targetElement) {
          return true;
        }

        if (event.forwardedTouchEvent) {
          return true;
        }

        // Programmatically generated events targeting a specific element should be permitted
        if (!event.cancelable) {
          return true;
        }

        // Derive and check the target element to see whether the mouse event needs to be permitted;
        // unless explicitly enabled, prevent non-touch click events from triggering actions,
        // to prevent ghost/doubleclicks.
        if (!this.needsClick(this.targetElement) || this.cancelNextClick) {

          // Prevent any user-added listeners declared on FastClick element from being fired.
          if (event.stopImmediatePropagation) {
            event.stopImmediatePropagation();
          } else {

            // Part of the hack for browsers that don't support Event#stopImmediatePropagation (e.g. Android 2)
            event.propagationStopped = true;
          }

          // Cancel the event
          event.stopPropagation();
          event.preventDefault();

          return false;
        }

        // If the mouse event is permitted, return true for the action to go through.
        return true;
      };

      /**
       * On actual clicks, determine whether this is a touch-generated click, a click action occurring
       * naturally after a delay after a touch (which needs to be cancelled to avoid duplication), or
       * an actual click which should be permitted.
       *
       * @param {Event} event
       * @returns {boolean}
       */
      FastClick.prototype.onClick = function (event) {
        var permitted;

        // It's possible for another FastClick-like library delivered with third-party code to fire a click event before FastClick does (issue #44). In that case, set the click-tracking flag back to false and return early. This will cause onTouchEnd to return early.
        if (this.trackingClick) {
          this.targetElement = null;
          this.trackingClick = false;
          return true;
        }

        // Very odd behaviour on iOS (issue #18): if a submit element is present inside a form and the user hits enter in the iOS simulator or clicks the Go button on the pop-up OS keyboard the a kind of 'fake' click event will be triggered with the submit-type input element as the target.
        if (event.target.type === 'submit' && event.detail === 0) {
          return true;
        }

        permitted = this.onMouse(event);

        // Only unset targetElement if the click is not permitted. This will ensure that the check for !targetElement in onMouse fails and the browser's click doesn't go through.
        if (!permitted) {
          this.targetElement = null;
        }

        // If clicks are permitted, return true for the action to go through.
        return permitted;
      };

      /**
       * Remove all FastClick's event listeners.
       *
       * @returns {void}
       */
      FastClick.prototype.destroy = function () {
        var layer = this.layer;

        if (deviceIsAndroid) {
          layer.removeEventListener('mouseover', this.onMouse, true);
          layer.removeEventListener('mousedown', this.onMouse, true);
          layer.removeEventListener('mouseup', this.onMouse, true);
        }

        layer.removeEventListener('click', this.onClick, true);
        layer.removeEventListener('touchstart', this.onTouchStart, false);
        layer.removeEventListener('touchmove', this.onTouchMove, false);
        layer.removeEventListener('touchend', this.onTouchEnd, false);
        layer.removeEventListener('touchcancel', this.onTouchCancel, false);
      };

      /**
       * Check whether FastClick is needed.
       *
       * @param {Element} layer The layer to listen on
       */
      FastClick.notNeeded = function (layer) {
        var metaViewport;
        var chromeVersion;
        var blackberryVersion;
        var firefoxVersion;

        // Devices that don't support touch don't need FastClick
        if (typeof window.ontouchstart === 'undefined') {
          return true;
        }

        // Chrome version - zero for other browsers
        chromeVersion = +(/Chrome\/([0-9]+)/.exec(navigator.userAgent) || [, 0])[1];

        if (chromeVersion) {

          if (deviceIsAndroid) {
            metaViewport = document.querySelector('meta[name=viewport]');

            if (metaViewport) {
              // Chrome on Android with user-scalable="no" doesn't need FastClick (issue #89)
              if (metaViewport.content.indexOf('user-scalable=no') !== -1) {
                return true;
              }
              // Chrome 32 and above with width=device-width or less don't need FastClick
              if (chromeVersion > 31 && document.documentElement.scrollWidth <= window.outerWidth) {
                return true;
              }
            }

            // Chrome desktop doesn't need FastClick (issue #15)
          } else {
              return true;
            }
        }

        if (deviceIsBlackBerry10) {
          blackberryVersion = navigator.userAgent.match(/Version\/([0-9]*)\.([0-9]*)/);

          // BlackBerry 10.3+ does not require Fastclick library.
          // https://github.com/ftlabs/fastclick/issues/251
          if (blackberryVersion[1] >= 10 && blackberryVersion[2] >= 3) {
            metaViewport = document.querySelector('meta[name=viewport]');

            if (metaViewport) {
              // user-scalable=no eliminates click delay.
              if (metaViewport.content.indexOf('user-scalable=no') !== -1) {
                return true;
              }
              // width=device-width (or less than device-width) eliminates click delay.
              if (document.documentElement.scrollWidth <= window.outerWidth) {
                return true;
              }
            }
          }
        }

        // IE10 with -ms-touch-action: none or manipulation, which disables double-tap-to-zoom (issue #97)
        if (layer.style.msTouchAction === 'none' || layer.style.touchAction === 'manipulation') {
          return true;
        }

        // Firefox version - zero for other browsers
        firefoxVersion = +(/Firefox\/([0-9]+)/.exec(navigator.userAgent) || [, 0])[1];

        if (firefoxVersion >= 27) {
          // Firefox 27+ does not have tap delay if the content is not zoomable - https://bugzilla.mozilla.org/show_bug.cgi?id=922896

          metaViewport = document.querySelector('meta[name=viewport]');
          if (metaViewport && (metaViewport.content.indexOf('user-scalable=no') !== -1 || document.documentElement.scrollWidth <= window.outerWidth)) {
            return true;
          }
        }

        // IE11: prefixed -ms-touch-action is no longer supported and it's recomended to use non-prefixed version
        // http://msdn.microsoft.com/en-us/library/windows/apps/Hh767313.aspx
        if (layer.style.touchAction === 'none' || layer.style.touchAction === 'manipulation') {
          return true;
        }

        return false;
      };

      /**
       * Factory method for creating a FastClick object
       *
       * @param {Element} layer The layer to listen on
       * @param {Object} [options={}] The options to override the defaults
       */
      FastClick.attach = function (layer, options) {
        return new FastClick(layer, options);
      };

      if (typeof define === 'function' && _typeof(define.amd) === 'object' && define.amd) {

        // AMD. Register as an anonymous module.
        define(function () {
          return FastClick;
        });
      } else if (typeof module !== 'undefined' && module.exports) {
        module.exports = FastClick.attach;
        module.exports.FastClick = FastClick;
      } else {
        window.FastClick = FastClick;
      }
    })();
  }, {}], 3: [function (require, module, exports) {
    // the whatwg-fetch polyfill installs the fetch() function
    // on the global object (window or self)
    //
    // Return that as the export for use in Webpack, Browserify etc.
    require('whatwg-fetch');
    module.exports = self.fetch.bind(self);
  }, { "whatwg-fetch": 5 }], 4: [function (require, module, exports) {
    // shim for using process in browser

    var process = module.exports = {};

    // cached from whatever global is present so that test runners that stub it
    // don't break things.  But we need to wrap it in a try catch in case it is
    // wrapped in strict mode code which doesn't define any globals.  It's inside a
    // function because try/catches deoptimize in certain engines.

    var cachedSetTimeout;
    var cachedClearTimeout;

    (function () {
      try {
        cachedSetTimeout = setTimeout;
      } catch (e) {
        cachedSetTimeout = function cachedSetTimeout() {
          throw new Error('setTimeout is not defined');
        };
      }
      try {
        cachedClearTimeout = clearTimeout;
      } catch (e) {
        cachedClearTimeout = function cachedClearTimeout() {
          throw new Error('clearTimeout is not defined');
        };
      }
    })();
    var queue = [];
    var draining = false;
    var currentQueue;
    var queueIndex = -1;

    function cleanUpNextTick() {
      if (!draining || !currentQueue) {
        return;
      }
      draining = false;
      if (currentQueue.length) {
        queue = currentQueue.concat(queue);
      } else {
        queueIndex = -1;
      }
      if (queue.length) {
        drainQueue();
      }
    }

    function drainQueue() {
      if (draining) {
        return;
      }
      var timeout = cachedSetTimeout(cleanUpNextTick);
      draining = true;

      var len = queue.length;
      while (len) {
        currentQueue = queue;
        queue = [];
        while (++queueIndex < len) {
          if (currentQueue) {
            currentQueue[queueIndex].run();
          }
        }
        queueIndex = -1;
        len = queue.length;
      }
      currentQueue = null;
      draining = false;
      cachedClearTimeout(timeout);
    }

    process.nextTick = function (fun) {
      var args = new Array(arguments.length - 1);
      if (arguments.length > 1) {
        for (var i = 1; i < arguments.length; i++) {
          args[i - 1] = arguments[i];
        }
      }
      queue.push(new Item(fun, args));
      if (queue.length === 1 && !draining) {
        cachedSetTimeout(drainQueue, 0);
      }
    };

    // v8 likes predictible objects
    function Item(fun, array) {
      this.fun = fun;
      this.array = array;
    }
    Item.prototype.run = function () {
      this.fun.apply(null, this.array);
    };
    process.title = 'browser';
    process.browser = true;
    process.env = {};
    process.argv = [];
    process.version = ''; // empty string to avoid regexp issues
    process.versions = {};

    function noop() {}

    process.on = noop;
    process.addListener = noop;
    process.once = noop;
    process.off = noop;
    process.removeListener = noop;
    process.removeAllListeners = noop;
    process.emit = noop;

    process.binding = function (name) {
      throw new Error('process.binding is not supported');
    };

    process.cwd = function () {
      return '/';
    };
    process.chdir = function (dir) {
      throw new Error('process.chdir is not supported');
    };
    process.umask = function () {
      return 0;
    };
  }, {}], 5: [function (require, module, exports) {
    (function (self) {
      'use strict';

      if (self.fetch) {
        return;
      }

      var support = {
        searchParams: 'URLSearchParams' in self,
        iterable: 'Symbol' in self && 'iterator' in Symbol,
        blob: 'FileReader' in self && 'Blob' in self && function () {
          try {
            new Blob();
            return true;
          } catch (e) {
            return false;
          }
        }(),
        formData: 'FormData' in self,
        arrayBuffer: 'ArrayBuffer' in self
      };

      function normalizeName(name) {
        if (typeof name !== 'string') {
          name = String(name);
        }
        if (/[^a-z0-9\-#$%&'*+.\^_`|~]/i.test(name)) {
          throw new TypeError('Invalid character in header field name');
        }
        return name.toLowerCase();
      }

      function normalizeValue(value) {
        if (typeof value !== 'string') {
          value = String(value);
        }
        return value;
      }

      // Build a destructive iterator for the value list
      function iteratorFor(items) {
        var iterator = {
          next: function next() {
            var value = items.shift();
            return { done: value === undefined, value: value };
          }
        };

        if (support.iterable) {
          iterator[Symbol.iterator] = function () {
            return iterator;
          };
        }

        return iterator;
      }

      function Headers(headers) {
        this.map = {};

        if (headers instanceof Headers) {
          headers.forEach(function (value, name) {
            this.append(name, value);
          }, this);
        } else if (headers) {
          Object.getOwnPropertyNames(headers).forEach(function (name) {
            this.append(name, headers[name]);
          }, this);
        }
      }

      Headers.prototype.append = function (name, value) {
        name = normalizeName(name);
        value = normalizeValue(value);
        var list = this.map[name];
        if (!list) {
          list = [];
          this.map[name] = list;
        }
        list.push(value);
      };

      Headers.prototype['delete'] = function (name) {
        delete this.map[normalizeName(name)];
      };

      Headers.prototype.get = function (name) {
        var values = this.map[normalizeName(name)];
        return values ? values[0] : null;
      };

      Headers.prototype.getAll = function (name) {
        return this.map[normalizeName(name)] || [];
      };

      Headers.prototype.has = function (name) {
        return this.map.hasOwnProperty(normalizeName(name));
      };

      Headers.prototype.set = function (name, value) {
        this.map[normalizeName(name)] = [normalizeValue(value)];
      };

      Headers.prototype.forEach = function (callback, thisArg) {
        Object.getOwnPropertyNames(this.map).forEach(function (name) {
          this.map[name].forEach(function (value) {
            callback.call(thisArg, value, name, this);
          }, this);
        }, this);
      };

      Headers.prototype.keys = function () {
        var items = [];
        this.forEach(function (value, name) {
          items.push(name);
        });
        return iteratorFor(items);
      };

      Headers.prototype.values = function () {
        var items = [];
        this.forEach(function (value) {
          items.push(value);
        });
        return iteratorFor(items);
      };

      Headers.prototype.entries = function () {
        var items = [];
        this.forEach(function (value, name) {
          items.push([name, value]);
        });
        return iteratorFor(items);
      };

      if (support.iterable) {
        Headers.prototype[Symbol.iterator] = Headers.prototype.entries;
      }

      function consumed(body) {
        if (body.bodyUsed) {
          return Promise.reject(new TypeError('Already read'));
        }
        body.bodyUsed = true;
      }

      function fileReaderReady(reader) {
        return new Promise(function (resolve, reject) {
          reader.onload = function () {
            resolve(reader.result);
          };
          reader.onerror = function () {
            reject(reader.error);
          };
        });
      }

      function readBlobAsArrayBuffer(blob) {
        var reader = new FileReader();
        reader.readAsArrayBuffer(blob);
        return fileReaderReady(reader);
      }

      function readBlobAsText(blob) {
        var reader = new FileReader();
        reader.readAsText(blob);
        return fileReaderReady(reader);
      }

      function Body() {
        this.bodyUsed = false;

        this._initBody = function (body) {
          this._bodyInit = body;
          if (typeof body === 'string') {
            this._bodyText = body;
          } else if (support.blob && Blob.prototype.isPrototypeOf(body)) {
            this._bodyBlob = body;
          } else if (support.formData && FormData.prototype.isPrototypeOf(body)) {
            this._bodyFormData = body;
          } else if (support.searchParams && URLSearchParams.prototype.isPrototypeOf(body)) {
            this._bodyText = body.toString();
          } else if (!body) {
            this._bodyText = '';
          } else if (support.arrayBuffer && ArrayBuffer.prototype.isPrototypeOf(body)) {
            // Only support ArrayBuffers for POST method.
            // Receiving ArrayBuffers happens via Blobs, instead.
          } else {
              throw new Error('unsupported BodyInit type');
            }

          if (!this.headers.get('content-type')) {
            if (typeof body === 'string') {
              this.headers.set('content-type', 'text/plain;charset=UTF-8');
            } else if (this._bodyBlob && this._bodyBlob.type) {
              this.headers.set('content-type', this._bodyBlob.type);
            } else if (support.searchParams && URLSearchParams.prototype.isPrototypeOf(body)) {
              this.headers.set('content-type', 'application/x-www-form-urlencoded;charset=UTF-8');
            }
          }
        };

        if (support.blob) {
          this.blob = function () {
            var rejected = consumed(this);
            if (rejected) {
              return rejected;
            }

            if (this._bodyBlob) {
              return Promise.resolve(this._bodyBlob);
            } else if (this._bodyFormData) {
              throw new Error('could not read FormData body as blob');
            } else {
              return Promise.resolve(new Blob([this._bodyText]));
            }
          };

          this.arrayBuffer = function () {
            return this.blob().then(readBlobAsArrayBuffer);
          };

          this.text = function () {
            var rejected = consumed(this);
            if (rejected) {
              return rejected;
            }

            if (this._bodyBlob) {
              return readBlobAsText(this._bodyBlob);
            } else if (this._bodyFormData) {
              throw new Error('could not read FormData body as text');
            } else {
              return Promise.resolve(this._bodyText);
            }
          };
        } else {
          this.text = function () {
            var rejected = consumed(this);
            return rejected ? rejected : Promise.resolve(this._bodyText);
          };
        }

        if (support.formData) {
          this.formData = function () {
            return this.text().then(decode);
          };
        }

        this.json = function () {
          return this.text().then(JSON.parse);
        };

        return this;
      }

      // HTTP methods whose capitalization should be normalized
      var methods = ['DELETE', 'GET', 'HEAD', 'OPTIONS', 'POST', 'PUT'];

      function normalizeMethod(method) {
        var upcased = method.toUpperCase();
        return methods.indexOf(upcased) > -1 ? upcased : method;
      }

      function Request(input, options) {
        options = options || {};
        var body = options.body;
        if (Request.prototype.isPrototypeOf(input)) {
          if (input.bodyUsed) {
            throw new TypeError('Already read');
          }
          this.url = input.url;
          this.credentials = input.credentials;
          if (!options.headers) {
            this.headers = new Headers(input.headers);
          }
          this.method = input.method;
          this.mode = input.mode;
          if (!body) {
            body = input._bodyInit;
            input.bodyUsed = true;
          }
        } else {
          this.url = input;
        }

        this.credentials = options.credentials || this.credentials || 'omit';
        if (options.headers || !this.headers) {
          this.headers = new Headers(options.headers);
        }
        this.method = normalizeMethod(options.method || this.method || 'GET');
        this.mode = options.mode || this.mode || null;
        this.referrer = null;

        if ((this.method === 'GET' || this.method === 'HEAD') && body) {
          throw new TypeError('Body not allowed for GET or HEAD requests');
        }
        this._initBody(body);
      }

      Request.prototype.clone = function () {
        return new Request(this);
      };

      function decode(body) {
        var form = new FormData();
        body.trim().split('&').forEach(function (bytes) {
          if (bytes) {
            var split = bytes.split('=');
            var name = split.shift().replace(/\+/g, ' ');
            var value = split.join('=').replace(/\+/g, ' ');
            form.append(decodeURIComponent(name), decodeURIComponent(value));
          }
        });
        return form;
      }

      function headers(xhr) {
        var head = new Headers();
        var pairs = (xhr.getAllResponseHeaders() || '').trim().split('\n');
        pairs.forEach(function (header) {
          var split = header.trim().split(':');
          var key = split.shift().trim();
          var value = split.join(':').trim();
          head.append(key, value);
        });
        return head;
      }

      Body.call(Request.prototype);

      function Response(bodyInit, options) {
        if (!options) {
          options = {};
        }

        this.type = 'default';
        this.status = options.status;
        this.ok = this.status >= 200 && this.status < 300;
        this.statusText = options.statusText;
        this.headers = options.headers instanceof Headers ? options.headers : new Headers(options.headers);
        this.url = options.url || '';
        this._initBody(bodyInit);
      }

      Body.call(Response.prototype);

      Response.prototype.clone = function () {
        return new Response(this._bodyInit, {
          status: this.status,
          statusText: this.statusText,
          headers: new Headers(this.headers),
          url: this.url
        });
      };

      Response.error = function () {
        var response = new Response(null, { status: 0, statusText: '' });
        response.type = 'error';
        return response;
      };

      var redirectStatuses = [301, 302, 303, 307, 308];

      Response.redirect = function (url, status) {
        if (redirectStatuses.indexOf(status) === -1) {
          throw new RangeError('Invalid status code');
        }

        return new Response(null, { status: status, headers: { location: url } });
      };

      self.Headers = Headers;
      self.Request = Request;
      self.Response = Response;

      self.fetch = function (input, init) {
        return new Promise(function (resolve, reject) {
          var request;
          if (Request.prototype.isPrototypeOf(input) && !init) {
            request = input;
          } else {
            request = new Request(input, init);
          }

          var xhr = new XMLHttpRequest();

          function responseURL() {
            if ('responseURL' in xhr) {
              return xhr.responseURL;
            }

            // Avoid security warnings on getResponseHeader when not allowed by CORS
            if (/^X-Request-URL:/m.test(xhr.getAllResponseHeaders())) {
              return xhr.getResponseHeader('X-Request-URL');
            }

            return;
          }

          xhr.onload = function () {
            var options = {
              status: xhr.status,
              statusText: xhr.statusText,
              headers: headers(xhr),
              url: responseURL()
            };
            var body = 'response' in xhr ? xhr.response : xhr.responseText;
            resolve(new Response(body, options));
          };

          xhr.onerror = function () {
            reject(new TypeError('Network request failed'));
          };

          xhr.ontimeout = function () {
            reject(new TypeError('Network request failed'));
          };

          xhr.open(request.method, request.url, true);

          if (request.credentials === 'include') {
            xhr.withCredentials = true;
          }

          if ('responseType' in xhr && support.blob) {
            xhr.responseType = 'blob';
          }

          request.headers.forEach(function (value, name) {
            xhr.setRequestHeader(name, value);
          });

          xhr.send(typeof request._bodyInit === 'undefined' ? null : request._bodyInit);
        });
      };
      self.fetch.polyfill = true;
    })(typeof self !== 'undefined' ? self : this);
  }, {}], 6: [function (require, module, exports) {
    module.exports = { "apiUrl": "http://192.168.3.95:8123/lx/" };
  }, {}], 7: [function (require, module, exports) {
    function onOk(callback) {
      if (typeof callback === 'function') {
        callback();
      }
      document.getElementById('weui_dialog_alert').style.display = 'none';
    }
    function Alert(content) {
      var title = arguments.length <= 1 || arguments[1] === undefined ? '提示' : arguments[1];

      var template = "\n    <div class=\"weui_dialog_alert\" id=\"weui_dialog_alert\">\n      <div class=\"weui_mask\"></div>\n      <div class=\"weui_dialog\">\n          <div class=\"weui_dialog_hd\">\n            <strong class=\"weui_dialog_title\">" + title + "</strong>\n        </div>\n          <div class=\"weui_dialog_bd\">" + content + "</div>\n          <div class=\"weui_dialog_ft\">\n              <a href=\"javascript:;\" class=\"weui_btn_dialog primary\" id=\"weui_dialog_alert_onOk\">确定</a>\n          </div>\n      </div>\n    </div>";
      var alertId = document.getElementById('weui_dialog_alert');
      if (alertId) {
        alertId.querySelector('.weui_dialog_title').textContent = title;
        alertId.querySelector('.weui_dialog_bd').textContent = content;
        alertId.style.display = 'block';
      } else {
        document.body.insertAdjacentHTML('beforeend', template);
      }
      document.getElementById('weui_dialog_alert_onOk').addEventListener('click', onOk);
    }

    module.exports = Alert;
  }, {}], 8: [function (require, module, exports) {
    function downloadAppInit(util) {
      var template = "<div class=\"appDownload downloadAppEnter\" id=\"appDownload\">\n    <div class=\"logo\" id=\"appDownloadLogo\"></div>\n    <div class=\"appDesc\">入驻自家小区，和邻居抢红包！</div>\n    <a class=\"justDownload\" href=\"http://a.app.qq.com/o/simple.jsp?pkgname=com.linxin.lx\">立即下载</a>\n    <div class=\"closeModal\" id=\"closeAppDownload\"></div>\n    <div class=\"clearfix\"></div>\n  </div>";
      if (util.param('tip') !== 'undefined') {
        (function () {
          document.body.insertAdjacentHTML('beforeend', template);
          var appDownload = document.getElementById('appDownload');
          var logoElem = document.getElementById('appDownloadLogo');
          var closeAppDownload = document.getElementById('closeAppDownload');
          if (window.spriteImgSize) {
            util.spriteAdaptation(logoElem, 0.5);
            util.spriteAdaptation(closeAppDownload, 0.5);
          }
          closeAppDownload.addEventListener('click', function () {
            appDownload.style.display = 'none';
          });
        })();
      }
    }

    module.exports = downloadAppInit;
  }, {}], 9: [function (require, module, exports) {
    require('es6-promise').polyfill();
    require('isomorphic-fetch');
    var util = require('../lib/util');

    var _require = require('../common/config');

    var apiUrl = _require.apiUrl;

    var Alert = require('../component/alert');

    var buildingId = util.param('buildingId');
    var top = document.getElementById('top');
    var appDownloadLink = 'http://a.app.qq.com/o/simple.jsp?pkgname=com.linxin.lx';
    var i = void 0;

    //设置banner默认高度
    top.style.height = document.body.clientWidth * 0.52 + 'px';

    //社区相册
    function albumList(list) {
      var html = '';
      for (i = 0; i < 5; i++) {
        if (list[i]) {
          html += "<a class=\"photo\" style=\"background-image:url(" + list[i].coverImg + ")\" href=\"" + appDownloadLink + "\">\n        <div class=\"img\"></div></a>";
        } else {
          html += '<div class="photo"><div class="img"></div></div>';
        }
      }
      return html;
    }

    //社区泡泡
    function interactiveList(list) {
      var html = '';
      for (i = 0; i < 3; i++) {
        if (list[i]) {
          html += "<a class=\"photo\" style=\"background-image:url(" + list[i].imgUrl + ")\" href=\"" + appDownloadLink + "\">\n        <div class=\"img\"></div><div class=\"label\">" + list[i].title + "</div></a>";
        } else {
          html += '<div class="photo"><div class="img"></div></div>';
        }
      }
      return html;
    }

    //我的邻居
    function neighborList(list) {
      var html = '';
      for (i = 0; i < 5; i++) {
        if (list[i]) {
          html += "<a class=\"photo\" style=\"background-image:url(" + list[i].logo + ")\" href=\"" + appDownloadLink + "\">\n        <div class=\"img\"></div></a>";
        } else {
          html += '<div class="photo"><div class="img"></div></div>';
        }
      }
      return html;
    }

    //社区公告
    function noticeList(list) {
      var html = '';
      list.forEach(function (value) {
        html += "\n    <div class=\"weui_cell\">\n      <div class=\"weui_cell_hd\"><div class=\"laba\"></div></div>\n      <a class=\"weui_cell_bd weui_cell_primary\" href=\"" + appDownloadLink + "\">" + value.title + "</a>\n      <div class=\"weui_cell_ft\"> </div>\n    </div>";
      });
      return html;
    }

    //便捷电话
    function hotlineList(list) {
      var html = '';
      list.forEach(function (value, index) {
        var isTel = parseInt(value.phone.substr(0, 1));
        var telHtml = isTel ? "<a href=\"tel:" + value.phone + "\">" + value.phone + "</a>" : value.phone;
        html += "\n    <div class=\"weui_cell\">\n      <div class=\"weui_cell_hd\">\n        <div class=\"flag" + value.flag + "\" id=\"hotline_" + index + "\"></div>\n      </div>\n      <div class=\"weui_cell_bd weui_cell_primary\"><span style=\"margin-right: 15px\">" + value.title + "</span>" + telHtml + "</div>\n      <div class=\"weui_cell_ft\"></div>\n    </div>";
      });
      return html;
    }
    function hotlineSprite(list) {
      list.forEach(function (value, index) {
        util.spriteAdaptation(document.getElementById("hotline_" + index), 0.5, window.spriteImgSize);
      });
    }

    //社区服务
    function serviceList(list) {
      var html = '';
      for (i = 0; i < 4; i++) {
        if (list[i]) {
          html += "<a class=\"service\" href=\"" + appDownloadLink + "\"><img class=\"img\" src=\"" + list[i].img + "\" /></a>";
        } else {
          html += '<div class="service"></div>';
        }
      }
      return html;
    }

    fetch(apiUrl + "GroupCenter/Main", {
      method: 'post',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
      },
      body: "buildingId=" + buildingId
    }).then(function (response) {
      return response.json();
    }).then(function (result) {
      if (result.State === 0) {
        (function () {
          console.log(result);
          var _result$Data = result.Data;
          var bgImg = _result$Data.bgImg;
          var logo = _result$Data.logo;
          var eventCount = _result$Data.eventCount;
          var lifeCount = _result$Data.lifeCount;
          var album = _result$Data.album;
          var interactive = _result$Data.interactive;
          var neighbor = _result$Data.neighbor;
          var notice = _result$Data.notice;
          var hotline = _result$Data.hotline;
          var serviceItem = _result$Data.serviceItem;
          var shareTitle = _result$Data.shareTitle;

          var bgImgObj = new Image();
          bgImgObj.src = bgImg;
          bgImgObj.onload = function () {
            document.getElementById('bgImg').src = bgImg;
            top.style.height = 'auto';
          };
          document.getElementById('communityLogo').style.backgroundImage = "url(" + logo + ")";
          document.getElementById('eventCount').textContent = eventCount;
          document.getElementById('lifeCount').textContent = lifeCount;
          document.getElementById('albumCount').textContent = album.count;
          document.getElementById('albumList').innerHTML = albumList(album.list);
          document.getElementById('interactiveCount').textContent = interactive.count;
          document.getElementById('interactiveList').innerHTML = interactiveList(interactive.list);
          document.getElementById('neighborCount').textContent = neighbor.count;
          document.getElementById('neighborList').innerHTML = neighborList(neighbor.list);
          document.getElementById('noticeCount').textContent = notice.count;
          document.getElementById('noticeList').innerHTML = noticeList(notice.list);
          document.getElementById('hotline').innerHTML = hotlineList(hotline);
          document.getElementById('serviceList').innerHTML = serviceList(serviceItem);
          hotlineSprite(hotline);
          document.title = shareTitle;
        })();
      } else {
        Alert(result.Msg);
      }
      util.pageComplete();
    }).catch(function () {
      Alert('网络异常');
    });
  }, { "../common/config": 6, "../component/alert": 7, "../lib/util": 10, "es6-promise": 1, "isomorphic-fetch": 3 }], 10: [function (require, module, exports) {
    var attachFastClick = require('fastclick');
    document.body.addEventListener('touchstart', function () {});
    attachFastClick(document.body);
    var downloadAppInit = require('../component/downloadApp');
    var util = {
      param: function param(key) {
        var url = location.search.replace(/^\?/, '').split('&');
        var paramsObj = {};
        var iLen = url.length;

        var i = void 0;
        for (i = 0; i < iLen; i++) {
          var param = url[i].split('=');
          paramsObj[param[0]] = param[1];
        }
        if (key) {
          return decodeURI(paramsObj[key]) || '';
        }
        return paramsObj;
      },
      pageComplete: function pageComplete() {
        var pageLoaderElem = document.getElementById('pageLoader');
        var pageHiddenElem = document.getElementById('pageHidden');
        pageLoaderElem.querySelector('.square-spin').classList.add('loaderLeave');
        setTimeout(function () {
          document.body.classList.remove('pageLoader');
          pageHiddenElem.classList.remove('pageHidden');
          document.getElementById('pageTextHidden').style.cssText = '';
          pageHiddenElem.classList.add('pageEnter');
          pageLoaderElem.style.display = 'none';
        }, 500);
        setTimeout(function () {
          document.getElementById('contentBody').className = '';
          document.getElementById('contentBody').classList.remove('contentBody');
          pageHiddenElem.classList.remove('pageEnter');
        }, 1000);
        setTimeout(function () {
          downloadAppInit(util);
        }, 2000);
      },
      formData: function formData(params) {
        var key = '';
        var data = '';

        for (key in params) {
          data += "&" + key + "=" + params[key];
        }
        return data.replace('&', '');
      },
      spriteAdaptation: function spriteAdaptation(elem, scale, spriteImgSize) {
        var width = spriteImgSize ? spriteImgSize[0] : window.spriteImgSize[0];
        var height = spriteImgSize ? spriteImgSize[1] : window.spriteImgSize[1];
        var bpx = parseInt(document.defaultView.getComputedStyle(elem, 'null').backgroundPositionX.replace('px', ''));
        var bpy = parseInt(document.defaultView.getComputedStyle(elem, 'null').backgroundPositionY.replace('px', ''));
        elem.style.backgroundPosition = Math.ceil(bpx * scale) + "px " + Math.ceil(bpy * scale) + "px";
        elem.style.webkitBackgroundSize = Math.ceil(width * scale) + "px " + Math.ceil(height * scale) + "px";
        elem.style.backgroundSize = Math.ceil(width * scale) + "px " + Math.ceil(height * scale) + "px";
      },
      imgSuffix: function imgSuffix(src) {
        if (window.isWebpSupport) {
          return src.replace(/\.(jpg|gif|png)$/i, '.webp');
        }
        return src;
      },

      //获取元素的left值
      getElementLeft: function getElementLeft(element) {
        var actualLeft = element.offsetLeft;
        var current = element.offsetParent;
        while (current !== null) {
          actualLeft += current.offsetLeft;
          current = current.offsetParent;
        }
        return actualLeft;
      }
    };

    module.exports = util;
  }, { "../component/downloadApp": 8, "fastclick": 2 }] }, {}, [9]);
//# sourceMappingURL=groupCenter.js.map
