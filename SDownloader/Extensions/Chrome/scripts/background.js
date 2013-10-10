/// <reference path="jquery-1.7.1-vsdoc.js" />

// Load Google Analytics
(function () {
    var ga = document.createElement('script');
    ga.type = 'text/javascript';
    ga.async = true;
    ga.src = 'https://ssl.google-analytics.com/ga.js';
    var s = document.getElementsByTagName('script')[0];
    s.parentNode.insertBefore(ga, s);
})();
    
var _gaq = _gaq || [];
    
chrome.runtime.onConnect.addListener(function(port) {
    console.assert(port.name == "sdownload");
    port.onMessage.addListener(function (msg) {
        _gaq.push(['_setAccount', 'UA-44166717-2']);
        if (msg.type == "pageview") {
            // Keep track of downloads using pageviews
            _gaq.push(['_trackPageview']);
        }
    });
});