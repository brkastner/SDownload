var pageMod = require("sdk/page-mod");
var self = require("sdk/self");

pageMod.PageMod({
    include: "*.soundcloud.com",
    contentScriptFile: [self.data.url("jquery-1.7.1.min.js"),
                        self.data.url("sdownload.js")]
});