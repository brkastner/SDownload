/// <reference path="jquery-1.7.1-vsdoc.js" />
function downloadSong(target) {
    var songHref = $(target).attr("data");

    $(target).off("click");
    $(target).html("Sent to Application!");
    $(target).addClass("sc-button-selected");
    console.log("redirecting to sdownload://" + songHref);
    window.location.href = 'sdownload://' + songHref;
    // logDownload();
};

function addDownloadButton() {
    // Single Song
    $(".listenContent .sound.single").each(function() {
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
            downloadSong(event.target);
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
                downloadSong(event.target);
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
                downloadSong(event.target);
            });

            downloadButton.appendTo(buttonGroup);
        }
    });
};

setInterval(function() {
    addDownloadButton();
}, 100);