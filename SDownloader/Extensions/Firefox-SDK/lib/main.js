var pageMod = require("sdk/page-mod");
var self = require("sdk/self");

pageMod.PageMod({
    include: "*.soundcloud.com",
    contentScriptFile: [self.data.url("jquery-1.7.1.min.js"),
                        self.data.url("sdownload.js")],
    onAttach: function (worker) {
        worker.port.on('download', function (url) {
            pageWorker = require("sdk/page-worker").Page({
                contentURL: self.data.url("background.html")
            });
            pageWorker.port.emit('download_url', url);
            pageWorker.port.on(url, function (info) {
                worker.port.emit(url, info);  
            };
        });
    }
});