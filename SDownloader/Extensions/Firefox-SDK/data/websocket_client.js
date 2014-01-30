/// <reference path="jquery-1.7.1-vsdoc.js" />
addon.port.on("download_url", function(songHref) {
    if (songHref && songHref.length > 0) {
        var server = new WebSocket("ws://localhost:7030");
        var opened = false;
        
        // Connection to helper application sucessful
        server.onopen = function () {
            opened = true;
            addon.port.emit(songHref, "Processing");
            server.send(songHref);
        };

        // Helper application was not running
        server.onerror = function () {
            if (!opened) {
                addon.port.emit(songHref, "Failure");
            } else {
                addon.port.emit(songHref, "Error!");
            }
        };

        // Message was received from the helper extension
        server.onmessage = function (msg) {
            var data = msg.data;
            addon.port.emit(songHref, msg);
            if (data.indexOf("CLOSE") != -1) {
                server.close();
            }
        };
    }
};