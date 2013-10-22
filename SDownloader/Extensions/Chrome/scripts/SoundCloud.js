/// <reference path="jquery-1.7.1-vsdoc.js" />

$(function () {
    function addDownloadButton() {

        // Single Song
        $(".listenContent .sound.single").not(".playlist").not(".sd-added").each(function() {

            var self = $(this);
            self.addClass("sd-added");

            var buttonGroup = self.find(".sc-button-group").first();

            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-medium").addClass("sc-button-responsive").addClass("sc-button-download");
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
        $(".sound.streamContext").not(".sdownload-added").each(function () {

            var self = $(this);

            var buttonGroup = self.find(".sc-button-group").first();

            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-small").addClass("sc-button-responsive").addClass("sc-button-download");
            downloadButton.attr("title", "SDownload");
            downloadButton.html("SDownload");

            var artCoverLink = self.find(".sound__coverArt").first();
            
            if (artCoverLink != undefined && artCoverLink.attr("href") != undefined) {
                var url = artCoverLink.attr("href");
                url = "https://soundcloud.com" + url;

                downloadButton.attr("data", url);

                downloadButton.appendTo(buttonGroup);

                self.addClass("sdownload-added");

                downloadButton.on("click", function(event) {
                    event.stopPropagation();
                    downloadClick(event.target);
                });
            }
        });
        
        // Individual songs from a set
        $(".trackList__item").not(".sdownload-added").each(function () {
            var self = $(this);

            var buttonGroup = self.find(".sc-button-group-small").first();
            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-small").addClass("sc-button-responsive").addClass("sc-button-icon").addClass("sc-button-download");
            downloadButton.attr("title", "SDownload");

            var coverLink = self.find(".soundBadge__avatarLink").first();

            if (coverLink != undefined && coverLink.attr("href") != undefined) {
                var url = "https://soundcloud.com" + coverLink.attr("href").split("?")[0];
                downloadButton.attr("data", url);
                self.addClass("sdownload-added");

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
                    $(target).html(data);
                    $(target).attr("title", "SDownload: " + data);
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

    ws.onerror = function () {
        // Launch the server
        window.location = "sdownload://launch";
    };
});