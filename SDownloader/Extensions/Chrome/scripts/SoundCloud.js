/// <reference path="jquery-1.7.1-vsdoc.js" />

$(function () {
    function addDownloadButton() {

        // Single Song
        $(".listenContent .sound.single").not(".playlist").not(".sd-added").each(function() {

            var self = $(this);

            var buttonGroup = self.find(".sc-button-group").first();
            
            // return if button exists
            if (buttonGroup.find(".sdownload-button").length > 0)
                return;

            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-medium").addClass("sc-button-responsive").addClass("sc-button-download");
            downloadButton.addClass("sdownload-button");
            downloadButton.attr("title", "SDownload");
            downloadButton.html("SDownload");

            var url = window.location.href.split("?")[0];

            downloadButton.attr("data", url);

            downloadButton.appendTo(buttonGroup);

            downloadButton.on("click", function(event) {
                event.stopPropagation();
                downloadClick(event.target);
            });

        });
        
        // Stream
        $(".sound.streamContext").each(function () {

            var self = $(this);

            var buttonGroup = self.find(".sc-button-group").first();
            
            // return if button exists
            if (buttonGroup.find(".sdownload-button").length > 0)
                return;

            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-small").addClass("sc-button-responsive").addClass("sc-button-download");
          
            downloadButton.attr("title", "SDownload");
            downloadButton.html("SDownload");

            var artCoverLink = self.find(".sound__coverArt").first();
            
            if (artCoverLink != undefined && artCoverLink.attr("href") != undefined) {
                var url = artCoverLink.attr("href");
                url = "https://soundcloud.com" + url;

                downloadButton.attr("data", url);
                downloadButton.addClass("sdownload-button");

                downloadButton.appendTo(buttonGroup);

                downloadButton.on("click", function(event) {
                    event.stopPropagation();
                    downloadClick(event.target);
                });
            }
        });
        
        // Individual songs from a set
        $(".trackList__item").each(function () {
            var self = $(this);

            var buttonGroup = self.find(".sc-button-group-small").first();
            
            // return if button exists
            if (buttonGroup.find(".sdownload-button").length > 0)
                return;
            
            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-small").addClass("sc-button-responsive").addClass("sc-button-icon").addClass("sc-button-download");
            downloadButton.attr("title", "SDownload");

            var coverLink = self.find(".soundBadge__avatarLink").first();

            if (coverLink != undefined && coverLink.attr("href") != undefined) {
                var url = "https://soundcloud.com" + coverLink.attr("href").split("?")[0];
                downloadButton.attr("data", url);
                downloadButton.addClass("sdownload-button");

                downloadButton.on("click", function(event) {
                    event.stopPropagation();
                    downloadClick(event.target);
                });

                downloadButton.appendTo(buttonGroup);
            }
        });


        function downloadClick(target) {

            var songHref = $(target).attr("data");

            $(target).off("click");
            $(target).html("Loading");
            $(target).addClass("sc-button-selected");

            if (songHref && songHref.length > 0) {
                var server = new WebSocket("ws://localhost:7030");
                // Connection to helper application sucessful
                server.onopen = function () {
                    $(target).html("Processing");
                    server.send(songHref);
                    logDownload();
                };

                // Helper application was not running
                server.onerror = function () {
                    window.location = "sdownload://" + songHref;
                    $(target).html("Sent to Application!");
                    logDownload();
                };

                // Message was received from the helper extension
                server.onmessage = function (msg) {
                    var data = msg.data;
                    if (data.indexOf("CLOSE") != -1) {
                        server.close();
                        $(target).on("click");
                    } else {
                        $(target).html(data);
                        $(target).attr("title", "SDownload: " + data);
                    }
                };
            }
        }
    }
    
    function logDownload() {
        port.postMessage({ type: "pageview" });
    }

    // Repeatedly attempt to add download buttons while the page dynamically loads
    setInterval(function() {
        addDownloadButton();
    }, 100);

    // Connect with the background script
    var port = chrome.runtime.connect({ name: "sdownload" });
    
    // Test the connection with the helper application
    var ws = new WebSocket("ws://localhost:7030");
    
    ws.onopen = function () {
        // Close the test connection, it isn't needed
        ws.close();
    };

    /*
    ws.onerror = function () {
        // Launch the server
        window.open("sdownload://launch");
        
        // Wait 20 seconds then try again, if still failed
        // confirm that the user has the application installed
        // and up to date
        window.setTimeout(function() {
            var confirm = new WebSocket("ws://localhost:7030");
            // User does not have it installed or isn't v2+
            confirm.onerror = function() {
                var box = confirm("SDownload is having trouble confirming you have the full application installed and up to date. " +
                    "Click okay to navigate to Sourceforge in order to download SDownload.");
                if (box == true)
                    chrome.tabs.create({ url: "http://sourceforge.net/projects/sdownload" });
            };
            
            // User has it installed
            confirm.onopen = function() {
                confirm.close();
            };
        }, 20000);
    };
    */
});